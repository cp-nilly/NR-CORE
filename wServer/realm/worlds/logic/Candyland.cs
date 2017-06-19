using System.Collections;
using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.networking;

namespace wServer.realm.worlds.logic
{
    class Candyland : World
    {
        private IEnumerable<Entity> _candySpawners;
        private Entity _candyBossSpawner;

        public Candyland(ProtoWorld proto, Client client = null) : base(proto)
        {
        }

        protected override void Init()
        {
            base.Init();

            if (IsLimbo) return;

            _candySpawners = Enemies.Values.Where(e => e.ObjectType == 0x5e31);
            _candyBossSpawner = Enemies.Values.SingleOrDefault(e => e.ObjectType == 0x5e43);

            foreach (var cs in _candySpawners)
                cs.TickStateManually = true;
            
            if (_candyBossSpawner != null)
                _candyBossSpawner.TickStateManually = true;
        }

        public override void Tick(RealmTime time)
        {
            base.Tick(time);

            if (IsLimbo || Deleted || _candySpawners == null || _candyBossSpawner == null) 
                return;

            foreach (var cs in _candySpawners)
                cs.TickState(time);
            _candyBossSpawner.TickState(time);
        }
    }
}
