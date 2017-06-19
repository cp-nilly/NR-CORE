using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.logic;

namespace wServer.realm.entities
{
    class Placeholder : StaticObject
    {
        public Placeholder(RealmManager manager, int life)
            : base(manager, 0x070f, life, true, true, false)
        {
            SetDefaultSize(0);
        }
    }
}
