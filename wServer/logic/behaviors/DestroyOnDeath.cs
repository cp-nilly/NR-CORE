using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class DestroyOnDeath : Behavior
    {
        private readonly string _target;

        public DestroyOnDeath(string target)
        {
            _target = target;
        }

        protected internal override void Resolve(State parent)
        {
            parent.Death += (sender, e) =>
            {
                var owner = e.Host.Owner;
                var entities = e.Host.GetNearestEntitiesByName(250, _target);

                if (entities != null)
                {
                    foreach (Entity ent in entities)
                    {
                        owner.LeaveWorld(ent);
                    }
                }
            };
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
        }
    }
}
