using common;
using common.resources;

namespace wServer.networking.packets.outgoing
{
    public class NewAbilityMessage : OutgoingMessage
    {
        public PAbility Type { get; set; }

        public override PacketId ID => PacketId.NEW_ABILITY;
        public override Packet CreateInstance() { return new NewAbilityMessage(); }

        protected override void Read(NReader rdr)
        {
            Type = (PAbility)rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write((int)Type);
        }
    }
}