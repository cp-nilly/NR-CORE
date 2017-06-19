using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetShoot : PetBehavior
    {
        public PetShoot(PAbility ability) : base(ability, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
        }
    }
}
