// Server receive/send networking code based around
// the code provided in Stan Kirk's article,
// "C# SocketAsyncEventArgs High Performance Socket Code."
// That artical can be found here:
// http://www.codeproject.com/Articles/83102/C-SocketAsyncEventArgs-High-Performance-Socket-Cod

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using wServer.networking.packets;
using wServer.realm;

namespace wServer.networking.server
{
    class ReceiveToken
    {
        public const int PrefixLength = 5;

        public readonly int BufferOffset;

        public int BytesRead;
        public int PacketLength;
        public readonly byte[] PacketBytes;
        
        public ReceiveToken(int offset)
        {
            BufferOffset = offset;
            PacketBytes = new byte[Server.BufferSize];
            PacketLength = PrefixLength;
        }

        public byte[] GetPacketBody()
        {
            if (BytesRead < PrefixLength)
                throw new Exception("Packet prefix not read yet.");

            var packetBody = new byte[PacketLength - PrefixLength];
            Array.Copy(PacketBytes, PrefixLength, packetBody, 0, packetBody.Length);
            return packetBody;
        }

        public PacketId GetPacketId()
        {
            if (BytesRead < PrefixLength)
                throw new Exception("Packet id not read yet.");

            return (PacketId) PacketBytes[4];
        }

        public void Reset()
        {
            PacketLength = PrefixLength;
            BytesRead = 0;
        }
    }

    class SendToken
    {
        public readonly int BufferOffset;

        public int BytesAvailable;
        public int BytesSent;

        public readonly byte[] Data;

        public SendToken(int offset)
        {
            BufferOffset = offset;
            Data = new byte[0x100000];
        }

        public void Reset()
        {
            BytesAvailable = 0;
            BytesSent = 0;
        }
    }

    public class Server
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Server));

        private const int MaxSimultaneousAcceptOps = 10;
        private const int Backlog = 1024;
        private const int OpsToPreAllocate = 2;
        public const int BufferSize = 0x20000;

        private readonly RealmManager _manager;
        private readonly int _port;
        private readonly int _maxConnections;
        private readonly byte[] _clientKey;

        private Socket _listenSocket;
        private readonly Semaphore _maxConnectionsEnforcer;
        
        readonly BufferManager _buffManager;
        private readonly SocketAsyncEventArgsPool _eventArgsPoolAccept;
        private readonly ClientPool _clientPool;

        public Server(RealmManager manager, int port, int maxConnections, byte[] clientKey) // think about making a settings class...
        {
            Log.Info("Starting server...");

            _manager = manager;
            _port = port;
            _maxConnections = maxConnections;
            _clientKey = clientKey;

            _buffManager = new BufferManager(
                (maxConnections + 1) * BufferSize * OpsToPreAllocate, BufferSize);
            _eventArgsPoolAccept = new SocketAsyncEventArgsPool(MaxSimultaneousAcceptOps);
            _clientPool = new ClientPool(maxConnections + 1);
            
            _maxConnectionsEnforcer = new Semaphore(maxConnections, maxConnections);

            Init();
        }

        private void Init()
        {
            _buffManager.InitBuffer();

            for (int i = 0; i < MaxSimultaneousAcceptOps; i++)
                _eventArgsPoolAccept.Push(CreateNewAcceptEventArgs());

            for (int i = 0; i < _maxConnections + 1; i++)
            {
                var send = CreateNewSendEventArgs();
                var receive = CreateNewReceiveEventArgs();
                _clientPool.Push(new Client(this, _manager, send, receive, _clientKey));
            }
        }

        private SocketAsyncEventArgs CreateNewAcceptEventArgs()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptEventArg_Completed;
            return acceptEventArg;
        }

        private SocketAsyncEventArgs CreateNewSendEventArgs()
        { // note: completed event not set here. Must be set before use.
            var eventArgs = new SocketAsyncEventArgs();
            _buffManager.SetBuffer(eventArgs);
            eventArgs.UserToken = new SendToken(eventArgs.Offset);
            return eventArgs;
        }

        private SocketAsyncEventArgs CreateNewReceiveEventArgs()
        { // note: completed event not set here. Must be set before use.
            var eventArgs = new SocketAsyncEventArgs();
            _buffManager.SetBuffer(eventArgs);
            eventArgs.UserToken = new ReceiveToken(eventArgs.Offset);
            return eventArgs;
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        internal void Start()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, _port);
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            _listenSocket.Listen(Backlog);

            Log.InfoFormat("Listening on port {0}...", _port);

            StartAccept();
        }

        private void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg;

            if (_eventArgsPoolAccept.Count > 1)
                try
                {
                    acceptEventArg = _eventArgsPoolAccept.Pop();
                }
                catch
                {
                    acceptEventArg = CreateNewAcceptEventArgs();
                }
            else
                acceptEventArg = CreateNewAcceptEventArgs();

            // wait for connection to open up if all available connections are used
            _maxConnectionsEnforcer.WaitOne();

            try
            {
                bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                    ProcessAccept(acceptEventArg);
            }
            catch { }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept();
                HandleBadAccept(acceptEventArgs);
                return;
            }

            // start up client
            acceptEventArgs.AcceptSocket.NoDelay = true;
            (_clientPool.Pop()).BeginHandling(acceptEventArgs.AcceptSocket);

            // recycle acceptEventArgs object
            acceptEventArgs.AcceptSocket = null;
            _eventArgsPoolAccept.Push(acceptEventArgs);

            // start listening for next connection
            StartAccept();
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            acceptEventArgs.AcceptSocket.Close();
            _eventArgsPoolAccept.Push(acceptEventArgs);
        }
      
        public void Disconnect(Client client)
        {
            try
            {
                client.Skt.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se == null || se.SocketErrorCode != SocketError.NotConnected)
                    Log.Error(e);
            }
            client.Skt.Close();

            client.Reset();
            _clientPool.Push(client);

            try
            {
                // increase the number of available connections
                _maxConnectionsEnforcer.Release();
            }
            catch (SemaphoreFullException e)
            {
                // This should happen only on server restart
                // If it doesn't need to handle the problem somwhere else
                Log.Error(e);
            }
        }

        public void Stop()
        {
            Log.Info("Stoping server...");

            try
            {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se == null || se.SocketErrorCode != SocketError.NotConnected)
                    Log.Error(e);
            }
            _listenSocket.Close();

            foreach (var i in _manager.Clients.Keys.ToArray())
                i.Disconnect();

            DisposeAllSaeaObjects();
        }

        private void DisposeAllSaeaObjects()
        {
            while (_eventArgsPoolAccept.Count > 0)
            {
                var eventArgs = _eventArgsPoolAccept.Pop();
                eventArgs.Dispose();
            }

            while (_clientPool.Count > 0)
            {
                var client = _clientPool.Pop();
                client.Dispose();
            }
        }
    }
}
