using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class ReproduceChildren : Behavior
    {
        //State storage: Spawn state
        class SpawnState
        {
            public List<Enemy> livingChildren;
            public int RemainingTime;
        }

        private readonly int _maxChildren;
        private readonly int _initialSpawn;
        private Cooldown _coolDown;
        private readonly ushort[] _children;

        public ReproduceChildren(int maxChildren = 5, double initialSpawn = 0.5, Cooldown coolDown = new Cooldown(), params string[] children)
        {
            _children = children.Select(GetObjType).ToArray();
            _maxChildren = maxChildren;
            _initialSpawn = (int)(maxChildren * initialSpawn);
            _coolDown = coolDown.Normalize(0);
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {   
            state = new SpawnState()
            {
                livingChildren = new List<Enemy>(),
                RemainingTime = _coolDown.Next(Random)
            };
            for (int i = 0; i < _initialSpawn; i++)
            {
                Entity entity = Entity.Resolve(host.Manager, _children[Random.Next(0, _children.Count())]);
                entity.GivesNoXp = true;
                entity.Move(host.X, host.Y);

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
                (state as SpawnState).livingChildren.Add(enemyEntity);
                host.Owner.EnterWorld(entity);
            }
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            var spawn = state as SpawnState;

            if (spawn == null)
                return;

            List<Enemy> toRemove = new List<Enemy>();
            foreach(var child in spawn.livingChildren)
                if (child.HP<0)
                    toRemove.Add(child);
            foreach (var child in toRemove)
                spawn.livingChildren.Remove(child);

            if (spawn.RemainingTime <= 0 && spawn.livingChildren.Count() < _maxChildren)
            {
                Entity entity = Entity.Resolve(host.Manager, _children[Random.Next(0,_children.Count())]);
                entity.Move(host.X, host.Y);

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
                spawn.RemainingTime = _coolDown.Next(Random);
                spawn.livingChildren.Add(enemyEntity);
            }
            else
                spawn.RemainingTime -= time.ElaspedMsDelta;
        }
    }
}
