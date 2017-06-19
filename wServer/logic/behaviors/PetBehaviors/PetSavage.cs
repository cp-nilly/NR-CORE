using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetSavage : PetBehavior
    {
        public PetSavage() : base(PAbility.Savage, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
        }
    }
}
