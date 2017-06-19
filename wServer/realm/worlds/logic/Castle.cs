using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.realm.terrain;
using wServer.networking;

namespace wServer.realm.worlds.logic
{
    class Castle : World
    {
        private int PlayersEntering;

        public Castle(ProtoWorld proto, Client client = null, int playersEntering = 100) : base(proto)
        {
            PlayersEntering = playersEntering;
        }

        public Castle(ProtoWorld proto, Client client = null) : base(proto)
        {
            // this is to get everyone in one spawn when the Castle comes from quaking
            PlayersEntering = 0;
        }

        public override KeyValuePair<IntPoint, TileRegion>[] GetSpawnPoints()
        {
            if (PlayersEntering < 20)
            {
                return Map.Regions.Where(t => t.Value == TileRegion.Spawn).Take(1).ToArray();
            }
            else if (PlayersEntering < 40)
            {
                return Map.Regions.Where(t => t.Value == TileRegion.Spawn).Take(2).ToArray();
            }
            else if (PlayersEntering < 60)
            {
                return Map.Regions.Where(t => t.Value == TileRegion.Spawn).Take(3).ToArray();
            }
            else 
            {
                return Map.Regions.Where(t => t.Value == TileRegion.Spawn).ToArray();
            }

        }


    }
}
