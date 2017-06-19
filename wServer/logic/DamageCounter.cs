using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common;
using wServer.logic.behaviors;
using wServer.realm.entities;
using wServer.realm;
using wServer.realm.worlds;
using wServer.realm.worlds.logic;

namespace wServer.logic
{
    public class DamageCounter
    {
        Enemy enemy;
        public Enemy Host { get { return enemy; } }
        public Projectile LastProjectile { get; private set; }
        public Player LastHitter { get; private set; }

        public DamageCounter Corpse { get; set; }
        public DamageCounter Parent { get; set; }

        WeakDictionary<Player, int> hitters = new WeakDictionary<Player, int>();
        public DamageCounter(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public void HitBy(Player player, RealmTime time, Projectile projectile, int dmg)
        {
            int totalDmg;
            if (!hitters.TryGetValue(player, out totalDmg))
                totalDmg = 0;
            totalDmg += dmg;
            hitters[player] = totalDmg;

            LastProjectile = projectile;
            LastHitter = player;

            player.FameCounter.Hit(projectile, enemy);
        }

        public Tuple<Player, int>[] GetPlayerData()
        {
            if (Parent != null)
                return Parent.GetPlayerData();
            List<Tuple<Player, int>> dat = new List<Tuple<Player, int>>();
            foreach (var i in hitters)
            {
                if (i.Key.Owner == null) continue;
                dat.Add(new Tuple<Player, int>(i.Key, i.Value));
            }
            return dat.ToArray();
        }

        public void Death(RealmTime time)
        {
            if (Corpse != null)
            {
                Corpse.Parent = this;
                return;
            }

            var enemy = (Parent ?? this).enemy;

            if (enemy.Owner is Realm)
                (enemy.Owner as Realm).EnemyKilled(enemy, (Parent ?? this).LastHitter);

            if (enemy.Spawned || enemy.Owner is Arena || enemy.Owner is ArenaSolo)
                return;

            int lvlUps = 0;
            foreach (var player in enemy.Owner.Players.Values
                .Where(p => enemy.Dist(p) < 25))
            {
                if (player.HasConditionEffect(common.resources.ConditionEffects.Paused))
                    continue;
                float xp = enemy.GivesNoXp ? 0 : 1;
                xp *= enemy.ObjectDesc.MaxHP / 10f * 
                    (enemy.ObjectDesc.ExpMultiplier ?? 1);
                float upperLimit = player.ExperienceGoal * 0.1f;
                if (player.Quest == enemy)
                    upperLimit = player.ExperienceGoal * 0.5f;

                float playerXp;
                if (upperLimit < xp)
                    playerXp = upperLimit;
                else
                    playerXp = xp;

                if (player.Owner is DeathArena || player.Owner.GetDisplayName().Contains("Theatre"))
                    playerXp *= .33f;

                if (player.XPBoostTime != 0 && player.Level < 20)
                    playerXp *= 2;

                var killer = (Parent ?? this).LastHitter == player;
                if (player.EnemyKilled(
                        enemy,
                        (int)playerXp,
                        killer) && !killer)
                    lvlUps++;
            }

            if ((Parent ?? this).LastHitter != null)
                (Parent ?? this).LastHitter.FameCounter.LevelUpAssist(lvlUps);
        }

        /*public void Death(RealmTime time)
        {
            if (Corpse != null)
            {
                Corpse.Parent = this;
                return;
            }

            List<Tuple<Player, int>> eligiblePlayers = new List<Tuple<Player, int>>();
            int totalDamage = 0;
            int totalPlayer = 0;
            var enemy = (Parent ?? this).enemy;
            foreach (var i in (Parent ?? this).hitters)
            {
                if (i.Key.Owner == null) continue;
                totalDamage += i.Value;
                totalPlayer++;
                eligiblePlayers.Add(new Tuple<Player, int>(i.Key, i.Value));
            }
            if (totalPlayer != 0)
            {
                float totalExp = totalPlayer * (enemy.ObjectDesc.MaxHP / 10f) * (enemy.ObjectDesc.ExpMultiplier ?? 1);
                float lowerLimit = totalExp / totalPlayer * 0.1f;
                int lvUps = 0;
                foreach (var i in eligiblePlayers)
                {
                    float playerXp = totalExp * i.Item2 / totalDamage;

                    float upperLimit = i.Item1.ExperienceGoal * 0.1f;
                    if (i.Item1.Quest == enemy)
                        upperLimit = i.Item1.ExperienceGoal * 0.5f;

                    if (playerXp < lowerLimit) playerXp = lowerLimit;
                    if (playerXp > upperLimit) playerXp = upperLimit;

                    var killer = (Parent ?? this).LastHitter == i.Item1;
                    if (i.Item1.EnemyKilled(
                        enemy,
                        (int)playerXp,
                        killer) && !killer)
                        lvUps++;
                }
                (Parent ?? this).LastHitter.FameCounter.LevelUpAssist(lvUps);
            }

            if (enemy.Owner is Realm)
                (enemy.Owner as Realm).EnemyKilled(enemy, (Parent ?? this).LastHitter);
        }*/

        public void TransferData(DamageCounter dc)
        {
            dc.LastProjectile = LastProjectile;
            dc.LastHitter = LastHitter;

            foreach (var plr in hitters.Keys)
            {
                int totalDmg;
                int totalExistingDmg;

                if (!hitters.TryGetValue(plr, out totalDmg))
                    totalDmg = 0;
                if (!dc.hitters.TryGetValue(plr, out totalExistingDmg))
                    totalExistingDmg = 0;

                dc.hitters[plr] = totalDmg + totalExistingDmg;
            }
        }
    }
}
