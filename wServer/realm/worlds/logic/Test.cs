using System.IO;
using common.resources;
using terrain;
using wServer.networking;

namespace wServer.realm.worlds.logic
{
    public class Test : World
    {
        public bool JsonLoaded { get; private set; }

        public Test(ProtoWorld proto, Client client = null) : base(proto) { }

        protected override void Init() { }

        public void LoadJson(string json)
        {
            if (!JsonLoaded)
            {
                FromWorldMap(new MemoryStream(Json2Wmap.Convert(Manager.Resources.GameData, json)));
                JsonLoaded = true;
            }

            InitShops();
        }
    }
}