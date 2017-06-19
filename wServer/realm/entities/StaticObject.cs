using System.Collections.Generic;
using System.Xml.Linq;
using common;
using wServer.networking.packets.outgoing;

namespace wServer.realm.entities
{
    public class StaticObject : Entity
    {
        //Stats
        public bool Vulnerable { get; private set; }
        public bool Static { get; private set; }
        public bool Hittestable { get; private set; }
        public bool Dying { get; private set; }

        private readonly SV<int> _hp;
        public int HP
        {
            get { return _hp.GetValue(); }
            set { _hp.SetValue(value); }
        }

        public static int? GetHP(XElement elem)
        {
            var n = elem.Element("MaxHitPoints");
            if (n != null)
                return Utils.FromString(n.Value);
            else
                return null;
        }

        public StaticObject(RealmManager manager, ushort objType, int? life, bool stat, bool dying, bool hittestable)
            : base(manager, objType)
        {
            _hp = new SV<int>(this, StatsType.HP, 0, dying);
            if (Vulnerable = life.HasValue)
                HP = life.Value;
            Dying = dying;
            Static = stat;
            Hittestable = hittestable;
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            stats[StatsType.HP] = (!Vulnerable) ? int.MaxValue : HP;
            base.ExportStats(stats);
        }

        protected override void ImportStats(StatsType stats, object val)
        {
            if (stats == StatsType.HP) HP = (int)val;
            base.ImportStats(stats, val);
        }

        public override bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            if (Vulnerable && projectile.ProjectileOwner is Player)
            {
                var def = this.ObjectDesc.Defense;
                if (projectile.ProjDesc.ArmorPiercing)
                    def = 0;
                var dmg = (int)StatsManager.GetDefenseDamage(this, projectile.Damage, def);
                HP -= dmg;
                Owner.BroadcastPacketNearby(new Damage()
                {
                    TargetId = this.Id,
                    Effects = 0,
                    DamageAmount = (ushort)dmg,
                    Kill = !CheckHP(),
                    BulletId = projectile.ProjectileId,
                    ObjectId = projectile.ProjectileOwner.Self.Id
                }, this, (projectile.ProjectileOwner as Player), PacketPriority.Low);
            }
            return true;
        }

        protected bool CheckHP()
        {
            if (HP <= 0)
            {
                var x = (int) (X - 0.5);
                var y = (int) (Y - 0.5);
                if (Owner.Map.Contains(new IntPoint(x, y)))
                    if (ObjectDesc != null &&
                        Owner.Map[x, y].ObjType == ObjectType)
                    {
                        var tile = Owner.Map[x, y];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }

                Owner.LeaveWorld(this);
                return false;
            }
            return true;
        }

        public override void Tick(RealmTime time)
        {
            if (Vulnerable)
            {
                if (Dying)
                    HP -= time.ElaspedMsDelta;

                CheckHP();
            }

            base.Tick(time);
        }
    }
}
