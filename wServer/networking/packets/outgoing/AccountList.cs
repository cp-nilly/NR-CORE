using common;

namespace wServer.networking.packets.outgoing
{
    public class AccountList : OutgoingMessage
    {
        public int AccountListId { get; set; }
        public string[] AccountIds { get; set; }
        public int LockAction { get; set; }

        public override PacketId ID => PacketId.ACCOUNTLIST;
        public override Packet CreateInstance() { return new AccountList(); }

        protected override void Read(NReader rdr)
        {
            AccountListId = rdr.ReadInt32();
            AccountIds = new string[rdr.ReadInt16()];
            for (int i = 0; i < AccountIds.Length; i++)
                AccountIds[i] = rdr.ReadUTF();
            LockAction = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(AccountListId);
            wtr.Write((short)AccountIds.Length);
            foreach (var i in AccountIds)
                wtr.WriteUTF(i);
            wtr.Write(LockAction);
        }
    }
}
