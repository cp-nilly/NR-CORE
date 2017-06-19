using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class SpawnGroup : Behavior
    {
        //State storage: Spawn state
        class SpawnState
        {
            public int CurrentNumber;
            public int RemainingTime;
        }

        int maxChildren;
        int initialSpawn;
        Cooldown coolDown;
        ushort[] children;
        double radius;

        public SpawnGroup(string group, int maxChildren = 5, double initialSpawn = 0.5, Cooldown coolDown = new Cooldown(), double radius = 0)
        {
            this.children = BehaviorDb.InitGameData.ObjectDescs.Values
                .Where(x => x.Group == group)
                .Select(x => x.ObjectType).ToArray();
            this.maxChildren = maxChildren;
            this.initialSpawn = (int)(maxChildren * initialSpawn);
            this.coolDown = coolDown.Normalize(0);
            this.radius = radius;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            state = new SpawnState()
            {
                CurrentNumber = initialSpawn,
                RemainingTime = coolDown.Next(Random)
            };
            for (int i = 0; i < initialSpawn; i++)
            {
                var x = host.X + (float) (Random.NextDouble() * radius);
                var y = host.Y + (float) (Random.NextDouble() * radius);

                if (!host.Owner.IsPassable(x, y, true))
                    continue;
                
                Entity entity = Entity.Resolve(host.Manager, children[Random.Next(children.Length)]);
                entity.Move(x, y);

                var enemyHost = host as Enemy;
                var enemyEntity = entity as Enemy;
                if (enemyHost != null && enemyEntity != null)
                {
                    enemyEntity.Terrain = enemyHost.Terrain;
                    if (enemyHost.Spawned)
                    {
                        enemyEntity.Spawned = true;
                        enemyEntity.ApplyConditionEffect(new ConditionEffect()
                        {
                            Effect = ConditionEffectIndex.Invisible,
                            DurationMS = -1
                        });
                    }
                }

                host.Owner.EnterWorld(entity);
            }
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            var spawn = (SpawnState)state;

            if (spawn.RemainingTime <= 0 && spawn.CurrentNumber < maxChildren)
            {
                var x = host.X + (float)(Random.NextDouble() * radius);
                var y = host.Y + (float)(Random.NextDouble() * radius);

                if (!host.Owner.IsPassable(x, y, true))
                {
                    spawn.RemainingTime = coolDown.Next(Random);
                    spawn.CurrentNumber++;
                    return;
                }

                Entity entity = Entity.Resolve(host.Manager, children[Random.Next(children.Length)]);
                entity.Move(x, y);

                var enemyHost = host as Enemy;
                var enemyEntity = entity as Enemy;
                if (enemyHost != null && enemyEntity != null)
                {
                    enemyEntity.Terrain = enemyHost.Terrain;
                    if (enemyHost.Spawned)
                    {
                        enemyEntity.Spawned = true;
                        enemyEntity.ApplyConditionEffect(new ConditionEffect()
                        {
                            Effect = ConditionEffectIndex.Invisible,
                            DurationMS = -1
                        });
                    }
                }

                host.Owner.EnterWorld(entity);
                spawn.RemainingTime = coolDown.Next(Random);
                spawn.CurrentNumber++;
            }
            else
                spawn.RemainingTime -= time.ElaspedMsDelta;
        }
    }
}
