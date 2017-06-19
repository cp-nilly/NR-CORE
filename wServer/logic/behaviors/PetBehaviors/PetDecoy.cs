using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetDecoy : PetBehavior
    {
        public PetDecoy() : base(PAbility.Decoy, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
        }
    }
}
