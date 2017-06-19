using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using common.resources;
using log4net;

namespace wServer.realm.entities
{
    public interface IProjectileOwner
    {
        Projectile[] Projectiles { get; }
        Entity Self { get; }
    }

    public class Projectile : Entity
    {
        public IProjectileOwner ProjectileOwner { get; set; }
        public ushort Container { get; set; }
        public ProjectileDesc ProjDesc { get; }
        public long CreationTime { get; set; }
        private bool _used { get; set; }

        public byte ProjectileId { get; set; }
        public Position StartPos { get; set; }
        public float Angle { get; set; }
        public int Damage { get; set; }

        private readonly ConcurrentDictionary<Player, Tuple<int, int>> _startTime = 
            new ConcurrentDictionary<Player, Tuple<int, int>>(); 
        private readonly HashSet<Entity> _hit = new HashSet<Entity>(); 

        public Projectile(RealmManager manager, ProjectileDesc desc)
            : base(manager, manager.Resources.GameData.IdToObjectType[desc.ObjectId])
        {
            ProjDesc = desc;
        }
        
        public void Destroy()
        {
            Owner?.LeaveWorld(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            ProjectileOwner.Projectiles[ProjectileId] = null;
            //ProjectileOwner = null;
        }

        public override void Tick(RealmTime time)
        {
            var elapsed = time.TotalElapsedMs - CreationTime;
            if (elapsed > ProjDesc.LifetimeMS)
            {
                Destroy();
                return;
            }
            
            base.Tick(time);
        }

        public Position GetPosition(long elapsedTicks)
        {
            var x = (double)StartPos.X;
            var y = (double)StartPos.Y;

            var dist = elapsedTicks * ProjDesc.Speed / 10000.0;
            var period = ProjectileId % 2 == 0 ? 0 : Math.PI;

            if (ProjDesc.Wavy)
            {
                var theta = Angle + (Math.PI * 64) * Math.Sin(period + 6 * Math.PI * (elapsedTicks / 1000.0));
                x += dist * Math.Cos(theta);
                y += dist * Math.Sin(theta);
            }
            else if (ProjDesc.Parametric)
            {
                var theta = (double)elapsedTicks / ProjDesc.LifetimeMS * 2 * Math.PI;
                var a = Math.Sin(theta) * (ProjectileId % 2 != 0 ? 1 : -1);
                var b = Math.Sin(theta * 2) * (ProjectileId % 4 < 2 ? 1 : -1);
                var c = Math.Sin(Angle);
                var d = Math.Cos(Angle);
                x += (a * d - b * c) * ProjDesc.Magnitude;
                y += (a * c + b * d) * ProjDesc.Magnitude;
            }
            else
            {
                if (ProjDesc.Boomerang)
                {
                    var d = (ProjDesc.LifetimeMS * ProjDesc.Speed / 10000.0) / 2;
                    if (dist > d)
                        dist = d - (dist - d);
                }
                x += dist * Math.Cos(Angle);
                y += dist * Math.Sin(Angle);
                if (ProjDesc.Amplitude != 0)
                {
                    var d = ProjDesc.Amplitude * Math.Sin(period + (double)elapsedTicks / ProjDesc.LifetimeMS * ProjDesc.Frequency * 2 * Math.PI);
                    x += d * Math.Cos(Angle + Math.PI / 2);
                    y += d * Math.Sin(Angle + Math.PI / 2);
                }
            }

            return new Position() { X = (float)x, Y = (float)y };
        }

        public void ForceHit(Entity entity, RealmTime time)
        {
            if (!ProjDesc.MultiHit && _used && !(entity is Player))
                return;

            if (_hit.Add(entity))
                entity.HitByProjectile(this, time);
            
            _used = true;
        }

        public void AddPlayerStartTime(Player player, int serverTime, int clientTime)
        {
            _startTime.TryAdd(player, new Tuple<int, int>(serverTime, clientTime));
        }

        public int GetPlayerServerStartTime(Player player)
        {
            if (!_startTime.ContainsKey(player))
                return -1;

            return _startTime[player].Item1;
        }

        public int GetPlayerClientStartTime(Player player)
        {
            if (!_startTime.ContainsKey(player))
                return -1;

            return _startTime[player].Item2;
        }
    }
}
