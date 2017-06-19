using System;
using System.Collections.Generic;
using System.Linq;
using common;
using common.resources;
using wServer.realm;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer
{
    static class EntityUtils
    {
        public static double DistSqr(this Entity a, Entity b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static double Dist(this Entity a, Entity b)
        {
            return Math.Sqrt(a.DistSqr(b));
        }

        public static bool AnyPlayerNearby(this Entity entity, int radius = Player.Radius)
        {
            foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, radius).Where(e => e is Player))
            {
                if (i.HasConditionEffect(ConditionEffects.Hidden))
                    continue;

                var d = i.DistSqr(entity);
                if (d < radius * radius)
                    return true;
            }
            return false;
        }

        public static bool AnyPlayerNearby(this World world, double x, double y, int radius = Player.Radius)
        {
            foreach (var i in world.PlayersCollision.HitTest(x, y, radius).Where(e => e is Player))
            {
                if (i.HasConditionEffect(ConditionEffects.Hidden))
                    continue;

                var d = MathsUtils.DistSqr(i.X, i.Y, x, y);
                if (d < radius * radius)
                    return true;
            }
            return false;
        }

        public static bool AnyEnemyNearby(this Entity entity, int radius = Player.Radius)
        {
            foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, radius))
            {
                if (!(i is Enemy) || entity == i)
                    continue;

                var d = i.DistSqr(entity);
                if (d < radius * radius)
                    return true;
            }
            return false;
        }

        public static bool AnyEnemyNearby(this World world, double x, double y, int radius = Player.Radius)
        {
            foreach (var i in world.EnemiesCollision.HitTest(x, y, radius))
            {
                if (!(i is Enemy))
                    continue;

                var d = MathsUtils.DistSqr(i.X, i.Y, x, y);
                if (d < radius * radius)
                    return true;
            }
            return false;
        }

        public static Entity GetLowestHpEntity(this Entity entity, double dist, ushort? objType, bool seeInvis = false) // objType = null for player
        {
            var entities = entity.GetNearestEntities(dist, objType, seeInvis).OfType<Character>();
            if (!entities.Any())
                return null;

            var lowestHp = entities.Min(e => e.HP);
            return entities.FirstOrDefault(e => e.HP == lowestHp);
        }

        public static Entity GetNearestEntity(this Entity entity, double dist, ushort? objType, bool seeInvis = false)   //Null for player
        {
            //return entity.GetNearestEntities(dist, objType).FirstOrDefault();
            
            // function speed might be a problem
            var entities = entity.GetNearestEntities(dist, objType, seeInvis).ToArray();
            if (entities.Length <= 0)
                return null;
            return entities.Aggregate(
                (curmin, x) => (curmin == null || x.DistSqr(entity) < curmin.DistSqr(entity) ? x : curmin));
        }

        public static IEnumerable<Entity> GetNearestEntities(this Entity entity, double dist, ushort? objType, bool seeInvis = false)   //Null for player
        {
            if (entity.Owner == null) yield break;
            if (objType == null)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, dist).Where(e => e is IPlayer))
                {
                    if (!seeInvis && !(i as IPlayer).IsVisibleToEnemy()) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        yield return i;
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
                {
                    if (i.ObjectType != objType.Value) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        yield return i;
                }
        }

        public static IEnumerable<Entity> GetNearestEntitiesBySquare(this Entity entity, double dist, ushort? objType, bool seeInvis = false)   //Null for player
        {
            if (entity.Owner == null) yield break;
            if (objType == null)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, dist).Where(e => e is IPlayer))
                {
                    if (!seeInvis && !(i as IPlayer).IsVisibleToEnemy()) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        yield return i;
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
                {
                    if (i.ObjectType != objType.Value) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        yield return i;
                }
        }

        public static Entity GetNearestEntity(this Entity entity, double dist, bool players, Predicate<Entity> predicate = null)
        {
            if (entity.Owner == null) return null;
            Entity ret = null;
            if (players)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, dist).Where(e => e is IPlayer))
                {
                    if (!(i as IPlayer).IsVisibleToEnemy() ||
                        i == entity) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                    {
                        if (predicate != null && !predicate(i))
                            continue;
                        dist = d;
                        ret = i;
                    }
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
                {
                    if (i == entity) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                    {
                        if (predicate != null && !predicate(i))
                            continue;
                        dist = d;
                        ret = i;
                    }
                }
            return ret;
        }

        public static Entity GetNearestEntityByGroup(this Entity entity, double dist, string group)
        {
            //return entity.GetNearestEntitiesByGroup(dist, group).FirstOrDefault();

            // function speed might be a problem
            var entities = entity.GetNearestEntitiesByGroup(dist, group).ToArray();
            if (entities.Length <= 0)
                return null;
            return entities.Aggregate(
                (curmin, x) => (curmin == null || x.DistSqr(entity) < curmin.DistSqr(entity) ? x : curmin));
        }

        public static IEnumerable<Entity> GetNearestEntitiesByGroup(this Entity entity, double dist, string group)
        {
            if (entity.Owner == null)
                yield break;
            foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
            {
                if (i.ObjectDesc == null ||
                    i.ObjectDesc.Group == null ||
                    !i.ObjectDesc.Group.Equals(
                        group, StringComparison.InvariantCultureIgnoreCase)) 
                    continue;
                var d = i.Dist(entity);
                if (d < dist)
                    yield return i;
            }
        }

        public static Entity GetNearestEntityByName(this Entity entity, double dist, string id)
        {
            //return entity.GetNearestEntitiesByName(dist, id).FirstOrDefault();

            // function speed might be a problem
            var entities = entity.GetNearestEntitiesByName(dist, id).ToArray();
            if (entities.Length <= 0)
                return null;
            return entities.Aggregate(
                (curmin, x) => (curmin == null || x.DistSqr(entity) < curmin.DistSqr(entity) ? x : curmin));
        }

        public static IEnumerable<Entity> GetNearestEntitiesByName(this Entity entity, double dist, string id)
        {
            if (entity.Owner == null)
                yield break;
            foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
            {
                if (i.ObjectDesc == null || (id != null && !i.ObjectDesc.ObjectId.ContainsIgnoreCase(id)))
                    continue;

                var d = i.Dist(entity);
                if (d < dist)
                    yield return i;
            }
        }

        public static int CountEntity(this Entity entity, double dist, ushort? objType)
        {
            if (entity.Owner == null) return 0;
            int ret = 0;
            if (objType == null)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, dist).Where(e => e is Player))
                {
                    if (!(i as IPlayer).IsVisibleToEnemy()) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        ret++;
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
                {
                    if (i.ObjectType != objType.Value) continue;
                    var d = i.Dist(entity);
                    if (d < dist)
                        ret++;
                }
            return ret;
        }

        public static int CountEntity(this Entity entity, double dist, string group)
        {
            if (entity.Owner == null) return 0;
            int ret = 0;
            foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, dist))
            {
                if (i.ObjectDesc == null || i.ObjectDesc.Group != group) continue;
                var d = i.Dist(entity);
                if (d < dist)
                    ret++;
            }
            return ret;
        }

        public static float GetSpeed(this Entity entity, float spd)
        {
            return (entity.HasConditionEffect(ConditionEffects.Slowed)) ? (5.55f * spd + 0.74f) / 2 : 5.55f * spd + 0.74f;
        }

        public static void AOE(this Entity entity, float radius, ushort? objType, Action<Entity> callback)   //Null for player
        {
            if (objType == null)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, radius).Where(e => e is Player))
                {
                    var d = i.Dist(entity);
                    if (d < radius)
                        callback(i);
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, radius))
                {
                    if (i.ObjectType != objType.Value) continue;
                    var d = i.Dist(entity);
                    if (d < radius)
                        callback(i);
                }
        }

        public static void AOE(this Entity entity, float radius, bool players, Action<Entity> callback)   //Null for player
        {
            if (players)
                foreach (var i in entity.Owner.PlayersCollision.HitTest(entity.X, entity.Y, radius).Where(e => e is Player))
                {
                    var d = i.Dist(entity);
                    if (d < radius)
                        callback(i);
                }
            else
                foreach (var i in entity.Owner.EnemiesCollision.HitTest(entity.X, entity.Y, radius))
                {
                    if (!(i is Enemy)) continue;
                    var d = i.Dist(entity);
                    if (d < radius)
                        callback(i);
                }
        }

        public static void AOE(this World world, Position pos, float radius, bool players, Action<Entity> callback)   //Null for player
        {
            if (players)
                foreach (var i in world.PlayersCollision.HitTest(pos.X, pos.Y, radius).Where(e => e is Player))
                {
                    var d = MathsUtils.Dist(i.X, i.Y, pos.X, pos.Y);
                    if (d < radius)
                        callback(i);
                }
            else
                foreach (var i in world.EnemiesCollision.HitTest(pos.X, pos.Y, radius))
                {
                    var e = i as Enemy;
                    if (e == null || e.ObjectDesc.Static) 
                        continue;

                    var d = MathsUtils.Dist(i.X, i.Y, pos.X, pos.Y);
                    if (d < radius)
                        callback(i);
                }
        }

        public static void ForceUpdate(this Entity e, int slot)
        {
            if (e == null || (!(e is Player) && slot >= 8))
                return;
            
            switch (slot)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    e.InvokeStatChange(
                        (StatsType)((int)StatsType.Inventory0 + slot), 
                        (e as IContainer).Inventory[slot]?.ObjectType ?? -1);
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    e.InvokeStatChange(
                        (StatsType)((int)StatsType.BackPack0 + slot), 
                        (e as IContainer).Inventory[slot]?.ObjectType ?? -1);
                    break;
                case 254:
                    e.InvokeStatChange(
                        StatsType.HealthStackCount, (e as Player).HealthPots.Count);
                    break;
                case 255:
                    e.InvokeStatChange(
                        StatsType.MagicStackCount, (e as Player).MagicPots.Count);
                    break;
            }
        }
    }

    static class ItemUtils
    {
        public static bool AuditItem(this IContainer container, Item item, int slot)
        {
            if (container is OneWayContainer && item != null)
                return false;

            return item == null || container.SlotTypes[slot] == 0 || item.SlotType == container.SlotTypes[slot];
        }
    }
}
