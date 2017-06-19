using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.realm.entities
{
    public class Sign : StaticObject
    {
        public Sign(RealmManager manager, ushort objType) : base(manager, objType, null, true, false, false) { }

        public override bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            return false;
        }
    }
}
