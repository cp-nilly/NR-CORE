using common;

namespace wServer.networking.packets.outgoing
{
    public class MapInfo : OutgoingMessage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Difficulty { get; set; }
        public uint Seed { get; set; }
        public int Background { get; set; }
        public bool AllowPlayerTeleport { get; set; }
        public bool ShowDisplays { get; set; }
        public string[] ClientXML { get; set; }
        public string[] ExtraXML { get; set; }
        public string Music { get; set; }
        
        public override PacketId ID => PacketId.MAPINFO;
        public override Packet CreateInstance() { return new MapInfo(); }

        protected override void Read(NReader rdr)
        {
            Width = rdr.ReadInt32();
            Height = rdr.ReadInt32();
            Name = rdr.ReadUTF();
            DisplayName = rdr.ReadUTF();
            Seed = rdr.ReadUInt32();
            Background = rdr.ReadInt32();
            Difficulty = rdr.ReadInt32();
            AllowPlayerTeleport = rdr.ReadBoolean();
            ShowDisplays = rdr.ReadBoolean();
            
            ClientXML = new string[rdr.ReadInt16()];
            for (int i = 0; i < ClientXML.Length; i++)
                ClientXML[i] = rdr.Read32UTF();

            ExtraXML = new string[rdr.ReadInt16()];
            for (int i = 0; i < ExtraXML.Length; i++)
                ExtraXML[i] = rdr.Read32UTF();

            Music = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Width);
            wtr.Write(Height);
            wtr.WriteUTF(Name);
            wtr.WriteUTF(DisplayName);
            wtr.Write(Seed);
            wtr.Write(Background);
            wtr.Write(Difficulty);
            wtr.Write(AllowPlayerTeleport);
            wtr.Write(ShowDisplays);
            
            wtr.Write((short)ClientXML.Length);
            foreach (var i in ClientXML)
                wtr.Write32UTF(i);

            wtr.Write((short)ExtraXML.Length);
            foreach (var i in ExtraXML)
                wtr.Write32UTF(i);
            
            wtr.WriteUTF(Music);
        }
    }
}
