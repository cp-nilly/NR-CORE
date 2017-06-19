using common;

namespace wServer.networking.packets.outgoing.arena
{
    class ImminentArenaWave : OutgoingMessage
    {
        public int CurrentRuntime { get; set; }
        public int Wave { get; set; }

        public override PacketId ID => PacketId.IMMINENT_ARENA_WAVE;
        public override Packet CreateInstance() { return new ImminentArenaWave(); }

        protected override void Read(NReader rdr)
        {
            CurrentRuntime = rdr.ReadInt32();
            Wave = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(CurrentRuntime);
            wtr.Write(Wave);
        }
    }
}
