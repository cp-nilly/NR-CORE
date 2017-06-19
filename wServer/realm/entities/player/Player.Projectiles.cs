using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.logic;

namespace wServer.realm.entities
{
    public partial class Player
    {
        internal Projectile PlayerShootProjectile(
            byte id, ProjectileDesc desc, ushort objType,
            int time, Position position, float angle)
        {
            projectileId = id;
            var dmg = (int) Stats.GetAttackDamage(Stats[8], Stats[9]);
            return CreateProjectile(desc, objType, dmg,
                C2STime(time), position, angle);
        }

        //public void EnemyHit(RealmTime time, EnemyHit pkt)
        //{
        //    var entity = Owner.GetEntity(pkt.TargetId);
        //    if (entity != null && pkt.Kill)   //Tolerance
        //    {
        //        Projectile prj = (this as IProjectileOwner).Projectiles[pkt.BulletId];
        //        Position? entPos = entity.TryGetHistory((time.totalElapsedMs - tickMapping) - pkt.Time);
        //        Position? prjPos = prj == null ? null : (Position?)prj.GetPosition(pkt.Time + tickMapping - prj.BeginTime);
        //        var tol1 = (entPos == null || prjPos == null) ? 10 : (prjPos.Value.X - entPos.Value.X) * (prjPos.Value.X - entPos.Value.X) + (prjPos.Value.Y - entPos.Value.Y) * (prjPos.Value.Y - entPos.Value.Y);
        //        var tol2 = prj == null ? 10 : (prj.X - entity.X) * (prj.X - entity.X) + (prj.Y - entity.Y) * (prj.Y - entity.Y);
        //        if (prj != null && (tol1 < 1 || tol2 < 1))
        //        {
        //            prj.ForceHit(entity, time);
        //        }
        //        else
        //        {
        //            Console.WriteLine("CAN'T TOLERANT! " + tol1 + " " + tol2);
        //            client.SendPacket(new Update()
        //            {
        //                Tiles = new Update.TileData[0],
        //                NewObjs = new ObjectDef[] { entity.ToDefinition() },
        //                Drops = new int[] { pkt.TargetId }
        //            });
        //            clientEntities.Remove(entity);
        //        }
        //    }
        //    else if (pkt.Kill)
        //    {
        //        client.SendPacket(new Update()
        //        {
        //            Tiles = new Update.TileData[0],
        //            NewObjs = Empty<ObjectDef>.Array,
        //            Drops = new int[] { pkt.TargetId }
        //        });
        //    }
        //}
    }
}
