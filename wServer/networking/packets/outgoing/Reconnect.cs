using System;
using common;

namespace wServer.networking.packets.outgoing
{
    public class Reconnect : OutgoingMessage
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int GameId { get; set; }
        public int KeyTime { get; set; }
        public byte[] Key { get; private set; }
        public bool IsFromArena { get; set; }

        public override PacketId ID => PacketId.RECONNECT;
        public override Packet CreateInstance() { return new Reconnect(); }

        public Reconnect()
        {
            Key = Guid.NewGuid().ToByteArray();
        }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            Host = rdr.ReadUTF();
            Port = rdr.ReadInt32();
            GameId = rdr.ReadInt32();
            KeyTime = rdr.ReadInt32();
            IsFromArena = rdr.ReadBoolean();
            Key = rdr.ReadBytes(rdr.ReadInt16());
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.WriteUTF(Host);
            wtr.Write(Port);
            wtr.Write(GameId);
            wtr.Write(KeyTime);
            wtr.Write(IsFromArena);
            wtr.Write((short)Key.Length);
            wtr.Write(Key);
        }
    }
}
