using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetRisingFury : PetBehavior
    {
        public PetRisingFury() : base(PAbility.RisingFury, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
        }
    }
}
