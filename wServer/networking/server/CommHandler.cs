using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using common;
using log4net;
using wServer.networking.packets;
using wServer.networking.packets.outgoing;
using wServer.realm;

namespace wServer.networking.server
{
    public enum SendState
    {
        Awaiting,
        Ready,
        Sending
    }

    public class CommHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CommHandler));
        private readonly int _prefixLength;
        private readonly int _bufferSize;
        private readonly Client _client;
        private readonly SocketAsyncEventArgs _send;
        private readonly SocketAsyncEventArgs _receive;
        private ConcurrentQueue<Packet>[] _pendings;

        public CommHandler(
            Client client,
            SocketAsyncEventArgs send, 
            SocketAsyncEventArgs receive)
        {
            _prefixLength = ReceiveToken.PrefixLength;
            _bufferSize = Server.BufferSize;
            _client = client;

            _receive = receive;
            _receive.Completed += ProcessReceive;

            _send = send;
            _send.Completed += ProcessSend;

            _pendings = new ConcurrentQueue<Packet>[3];
            for (int i = 0; i < 3; i++)
                _pendings[i] = new ConcurrentQueue<Packet>();
        }

        public void Reset()
        {
            ((SendToken) _send.UserToken).Reset();
            ((ReceiveToken) _receive.UserToken).Reset();
            
            _pendings = new ConcurrentQueue<Packet>[3];
            for (var i = 0; i < 3; i++)
                _pendings[i] = new ConcurrentQueue<Packet>();
        }

        public void BeginHandling(Socket skt)
        {
            _send.AcceptSocket = skt;
            _receive.AcceptSocket = skt;

            _client.State = ProtocolState.Connected;
            StartReceive(_receive);
            StartSend(_send);
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (_client.State == ProtocolState.Disconnected)
                return;

            //Set the buffer for the receive operation.
            e.SetBuffer(e.Offset, _bufferSize);

            // Post async receive operation on the socket.
            bool willRaiseEvent;
            try
            {
                willRaiseEvent = e.AcceptSocket.ReceiveAsync(e);
                
            }
            catch (Exception exception)
            {
                _client.Disconnect($"[{_client.Account?.Name}:{_client.Account?.AccountId} {_client.IP}] {exception}");
                return;
            }

            if (!willRaiseEvent)
                ProcessReceive(null, e);
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            var r = (ReceiveToken)e.UserToken;

            if (_client.State == ProtocolState.Disconnected)
            {
                r.Reset();
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                var msg = "";
                if (e.SocketError != SocketError.ConnectionReset) // don't show connection resets...
                    msg = "Receive SocketError = " + e.SocketError;

                _client.Disconnect(msg);
                return;
            }
            
            var bytesNotRead = e.BytesTransferred;

            // Client has finished sending data?
            if (bytesNotRead == 0)
            {
                _client.Disconnect();
                return;
            }
            
            while (bytesNotRead > 0)
            {
                // read in prefix / message body
                bytesNotRead = ReadPacketBytes(e, r, bytesNotRead);

                // set packet length when prefix read
                if (r.BytesRead == _prefixLength)
                {
                    r.PacketLength = IPAddress.NetworkToHostOrder(
                        BitConverter.ToInt32(r.PacketBytes, 0));

                    // check for policy file (kinda hackish code)
                    if (r.PacketLength == 1014001516)
                    {
                        SendPolicyFile();
                        r.Reset();
                        break;
                    }

                    // discard invalid packets
                    if (r.PacketLength < _prefixLength ||
                        r.PacketLength > _bufferSize)
                    {
                        r.Reset();
                        break;
                    }
                }

                if (r.BytesRead == r.PacketLength)
                {
                    if (_client.IsReady())
                        _client.Manager.Network
                            .AddPendingPacket(_client, r.GetPacketId(), r.GetPacketBody());
                    r.Reset();
                }
            }

            StartReceive(e);
        }

        private static int ReadPacketBytes(SocketAsyncEventArgs e, ReceiveToken r, int bytesNotRead)
        {
            var offset = r.BufferOffset + e.BytesTransferred - bytesNotRead;
            var remainingBytes = r.PacketLength - r.BytesRead;

            if (bytesNotRead < remainingBytes)
            {
                Buffer.BlockCopy(e.Buffer, offset,
                    r.PacketBytes, r.BytesRead, bytesNotRead);
                r.BytesRead += bytesNotRead;
                return 0;
            }

            Buffer.BlockCopy(e.Buffer, offset,
                r.PacketBytes, r.BytesRead, remainingBytes);
            r.BytesRead = r.PacketLength;
            return bytesNotRead - remainingBytes;
        }

        private async void StartSend(SocketAsyncEventArgs e, int msDelay = 0)
        {
            if (_client.State == ProtocolState.Disconnected)
                return;

            var s = (SendToken) e.UserToken;

            if (msDelay > 0)
                await Task.Delay(msDelay);

            if (s.BytesAvailable <= 0)
            {
                s.Reset();
                FlushPending(s);
            }

            int bytesToSend = s.BytesAvailable > _bufferSize ? 
                _bufferSize : s.BytesAvailable;
            
            e.SetBuffer(s.BufferOffset, bytesToSend);
            Buffer.BlockCopy(s.Data, s.BytesSent,
                e.Buffer, s.BufferOffset, bytesToSend);

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = e.AcceptSocket.SendAsync(e);
            }
            catch (Exception exception)
            {
                _client.Disconnect($"[{_client.Account?.Name}:{_client.Account?.AccountId} {_client.IP}] {exception}");
                return;
            }

            if (!willRaiseEvent)
                ProcessSend(null, e);
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            var s = (SendToken)e.UserToken;

            if (_client.State == ProtocolState.Disconnected)
            {
                s.Reset();
                return;
            }
            
            if (e.SocketError != SocketError.Success)
            {
                _client.Disconnect("Send SocketError = " + e.SocketError);
                return;
            }

            s.BytesSent += e.BytesTransferred;
            s.BytesAvailable -= s.BytesSent;

            var delay = 0;
            if (s.BytesAvailable <= 0)
                delay = _client.Manager.Logic.MsPT;
            
            StartSend(e, delay);
        }

        public void SendPacket(Packet pkt, PacketPriority priority)
        {
            //if (priority == PacketPriority.Low && _client.IsLagging)
            //    return;

            _pendings[(int) priority].Enqueue(pkt);
        }

        public void SendPackets(IEnumerable<Packet> pkts, PacketPriority priority)
        {
            //if (priority == PacketPriority.Low && _client.IsLagging)
            //    return;

            foreach (var i in pkts)
                _pendings[(int) priority].Enqueue(i);
        }

        private bool FlushPending(SendToken s)
        {
            Packet packet;
            for (var i = 0; i < 3; i++)
                while (_pendings[i].TryDequeue(out packet))
                {
                    var bytesWritten = packet.Write(_client, s.Data, s.BytesAvailable);

                    if (bytesWritten == 0)
                    {
                        _pendings[i].Enqueue(packet);
                        return true;
                    }

                    s.BytesAvailable += bytesWritten;
                }

            if (s.BytesAvailable <= 0) 
                return false;
                
            return true;
        }

        private void SendPolicyFile()
        { // temporary
            if (_client.Skt == null)
                return;

            try
            {
                var s = new NetworkStream(_client.Skt);
                var wtr = new NWriter(s);
                wtr.WriteNullTerminatedString(
                    @"<cross-domain-policy>" +
                    @"<allow-access-from domain=""*"" to-ports=""*"" />" +
                    @"</cross-domain-policy>");
                wtr.Write((byte)'\r');
                wtr.Write((byte)'\n');
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public bool IsLagging()
        {
            for (var i = 0; i < 3; i++)
                if (_pendings[i].OfType<NewTick>().Any())
                    return true;
            
            return false;
        }
    }
}
