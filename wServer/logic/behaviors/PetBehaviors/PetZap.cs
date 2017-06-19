using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetZap : PetBehavior
    {
        public PetZap() : base(PAbility.Electric, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
        }
    }
}
