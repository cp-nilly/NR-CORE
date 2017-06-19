using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using common;
using System.Net.Sockets;
using System.Net;
using wServer.networking.packets.outgoing;

namespace wServer.networking.packets
{
    public abstract class Packet
    {
        public static Dictionary<PacketId, Packet> Packets = new Dictionary<PacketId, Packet>();

        public Client Owner { get; private set; }

        static Packet()
        {
            foreach (var i in typeof(Packet).Assembly.GetTypes())
                if (typeof(Packet).IsAssignableFrom(i) && !i.IsAbstract)
                {
                    Packet pkt = (Packet)Activator.CreateInstance(i);
                    if (!(pkt is OutgoingMessage))
                        Packets.Add(pkt.ID, pkt);
                }
        }
        public abstract PacketId ID { get; }
        public abstract Packet CreateInstance();

        public void SetOwner(Client client)
        {
            Owner = client;
        }

        public abstract void Crypt(Client client, byte[] dat, int offset, int len);

        public void Read(Client client, byte[] body, int offset, int len)
        {
            Crypt(client, body, offset, len);
            Read(new NReader(new MemoryStream(body)));
        }

        public int Write(Client client, byte[] buff, int offset)
        {
            var s = new MemoryStream();
            Write(new NWriter(s));

            var bodyLength = (int) s.Position;
            var packetLength = bodyLength + 5;

            if (packetLength > buff.Length - offset)
                return 0;

            Buffer.BlockCopy(s.GetBuffer(), 0, buff, offset + 5, bodyLength);

            Crypt(client, buff, offset + 5, bodyLength);

            Buffer.BlockCopy(
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetLength)), 0,
                buff, offset, 4);

            buff[offset + 4] = (byte) ID;
            return packetLength;
        }

        protected abstract void Read(NReader rdr);
        protected abstract void Write(NWriter wtr);

        public override string ToString()
        {
            // buggy...
            var ret = new StringBuilder("{");
            var arr = GetType().GetProperties();
            for (var i = 0; i < arr.Length; i++)
            {
                if (i != 0) ret.Append(", ");
                ret.AppendFormat("{0}: {1}", arr[i].Name, arr[i].GetValue(this, null));
            }
            ret.Append("}");
            return ret.ToString();
        }
    }
}
