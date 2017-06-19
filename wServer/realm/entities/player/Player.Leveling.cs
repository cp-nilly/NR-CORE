using System;
using System.Collections.Generic;
using System.Linq;
using wServer.networking.packets.outgoing;

namespace wServer.realm.entities
{
    public partial class Player
    {
        public static int GetExpGoal(int level)
        {
            return 50 + (level - 1) * 100;
        }
        public static int GetLevelExp(int level)
        {
            if (level == 1) return 0;
            return 50 * (level - 1) + (level - 2) * (level - 1) * 50;
        }
        public static int GetFameGoal(int fame)
        {
            if (fame >= 2000) return 0;
            else if (fame >= 800) return 2000;
            else if (fame >= 400) return 800;
            else if (fame >= 150) return 400;
            else if (fame >= 20) return 150;
            else return 20;
        }

        public int GetStars()
        {
            int ret = 0;
            foreach (var i in FameCounter.ClassStats.AllKeys)
            {
                var entry = FameCounter.ClassStats[ushort.Parse(i)];
                if (entry.BestFame >= 2000) ret += 5;
                else if (entry.BestFame >= 800) ret += 4;
                else if (entry.BestFame >= 400) ret += 3;
                else if (entry.BestFame >= 150) ret += 2;
                else if (entry.BestFame >= 20) ret += 1;
            }
            return ret;
        }

        static readonly Dictionary<string, Tuple<int, int, int>> QuestDat =
            new Dictionary<string, Tuple<int, int, int>>()  //Priority, Min, Max
        {
            // wandering quest enemies
            { "Scorpion Queen",                 Tuple.Create(1, 1, 6) },
            { "Bandit Leader",                  Tuple.Create(1, 1, 6) },
            { "Hobbit Mage",                    Tuple.Create(3, 3, 8) },
            { "Undead Hobbit Mage",             Tuple.Create(3, 3, 8) },
            { "Giant Crab",                     Tuple.Create(3, 3, 8) },
            { "Desert Werewolf",                Tuple.Create(3, 3, 8) },
            { "Sandsman King",                  Tuple.Create(4, 4, 9) },
            { "Goblin Mage",                    Tuple.Create(4, 4, 9) },
            { "Elf Wizard",                     Tuple.Create(4, 4, 9) },
            { "Dwarf King",                     Tuple.Create(5, 5, 10) },
            { "Swarm",                          Tuple.Create(6, 6, 11) },
            { "Shambling Sludge",               Tuple.Create(6, 6, 11) },
            { "Great Lizard",                   Tuple.Create(7, 7, 12) },
            { "Wasp Queen",                     Tuple.Create(8, 7, 20) },
            { "Horned Drake",                   Tuple.Create(8, 7, 20) },

            // setpiece bosses
            { "Deathmage",                      Tuple.Create(5, 6, 11) },
            { "Great Coil Snake",               Tuple.Create(6, 6, 12) },
            { "Lich",                           Tuple.Create(8, 6, 20) },
            { "Actual Lich",                    Tuple.Create(8, 7, 20) },
            { "Ent Ancient",                    Tuple.Create(9, 7, 20) },
            { "Actual Ent Ancient",             Tuple.Create(9, 7, 20) },
            { "Oasis Giant",                    Tuple.Create(10, 8, 20) },
            { "Phoenix Lord",                   Tuple.Create(10, 9, 20) },
            { "Ghost King",                     Tuple.Create(11,10, 20) },
            { "Actual Ghost King",              Tuple.Create(11,10, 20) },
            { "Cyclops God",                    Tuple.Create(12,10, 20) },
            { "Kage Kami",                      Tuple.Create(12,10, 20) },
            { "Red Demon",                      Tuple.Create(13,15, 20) },

                // events
            { "shtrs Defense System",           Tuple.Create(14,15, 20) },
            { "Fanatic of Chaos",               Tuple.Create(14,15, 20) },
            { "Skull Shrine",                   Tuple.Create(14,15, 20) },
            { "Pentaract",                      Tuple.Create(14,15, 20) },
            { "Cube God",                       Tuple.Create(14,15, 20) },
            { "Grand Sphinx",                   Tuple.Create(14,15, 20) },
            { "Lord of the Lost Lands",         Tuple.Create(14,15, 20) },
            { "Hermit God",                     Tuple.Create(14,15, 20) },
            { "Ghost Ship",                     Tuple.Create(14,15, 20) },
            { "Dragon Head",                    Tuple.Create(14,15, 20) },
            { "Lucky Ent God",                  Tuple.Create(14,15, 20) },
            { "Lucky Djinn",                    Tuple.Create(14,15, 20) },
            { "Zombie Horde",                   Tuple.Create(14,15, 20) },

                // dungeon bosses
            { "Evil Chicken God",               Tuple.Create(15,1, 20) },
            { "Bonegrind the Butcher",          Tuple.Create(15,1, 20) },
            { "Dreadstump the Pirate King",     Tuple.Create(15,1, 20) },
            { "Mama Megamoth",                  Tuple.Create(15,1, 20) },
            { "Arachna the Spider Queen",       Tuple.Create(15,1, 20) },
            { "Stheno the Snake Queen",         Tuple.Create(15,1, 20) },
            { "Mixcoatl the Masked God",        Tuple.Create(15,1, 20) },
            { "Limon the Sprite God",           Tuple.Create(15,1, 20) },
            { "Septavius the Ghost God",        Tuple.Create(15,1, 20) },
            { "Davy Jones",                     Tuple.Create(15,1, 20) },
            { "Lord Ruthven",                   Tuple.Create(15,1, 20) },
            { "Archdemon Malphas",              Tuple.Create(15,1, 20) },
            { "Thessal the Mermaid Goddess",    Tuple.Create(15,1, 20) },
            { "Dr Terrible",                    Tuple.Create(15,1, 20) },
            { "Horrific Creation",              Tuple.Create(15,1, 20) },
            { "Masked Party God",               Tuple.Create(15,1, 20) },
            { "Oryx Stone Guardian Left",       Tuple.Create(15,1, 20) },
            { "Oryx Stone Guardian Right",      Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 1",             Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 2",             Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 3",             Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 4",             Tuple.Create(15,1, 20) },
            { "Gigacorn",                       Tuple.Create(15,1, 20) },
            { "Desire Troll",                   Tuple.Create(15,1, 20) },
            { "Spoiled Creampuff",              Tuple.Create(15,1, 20) },
            { "MegaRototo",                     Tuple.Create(15,1, 20) },
            { "Swoll Fairy",                    Tuple.Create(15,1, 20) },
            { "BedlamGod",                      Tuple.Create(15,1, 20) },
            { "Troll 3",                        Tuple.Create(15,1, 20) },
            { "Arena Ghost Bride",              Tuple.Create(15,1, 20) },
            { "Arena Statue Left",              Tuple.Create(15,1, 20) },
            { "Arena Statue Right",             Tuple.Create(15,1, 20) },
            { "Arena Grave Caretaker",          Tuple.Create(15,1, 20) },
            { "Ghost of Skuld",                 Tuple.Create(15,1, 20) },
            { "Tomb Defender",                  Tuple.Create(15,1, 20) },
            { "Tomb Support",                   Tuple.Create(15,1, 20) },
            { "Tomb Attacker",                  Tuple.Create(15,1, 20) },
            { "Active Sarcophagus",             Tuple.Create(15,1, 20) },
            { "shtrs Bridge Sentinel",          Tuple.Create(15,1, 20) },
            { "shtrs The Forgotten King",       Tuple.Create(15,1, 20) },
            { "shtrs Twilight Archmage",        Tuple.Create(15,1, 20) },
            { "NM Black Dragon God",            Tuple.Create(15,1, 20) },
            { "NM Black Dragon God Hardmode",   Tuple.Create(15,1, 20) },
            { "NM Red Dragon God",              Tuple.Create(15,1, 20) },
            { "NM Red Dragon God Hardmode",     Tuple.Create(15,1, 20) },
            { "NM Blue Dragon God",             Tuple.Create(15,1, 20) },
            { "NM Blue Dragon God Hardmode",    Tuple.Create(15,1, 20) },
            { "NM Green Dragon God",            Tuple.Create(15,1, 20) },
            { "NM Green Dragon God Hardmode",   Tuple.Create(15,1, 20) },
            { "lod Ivory Wyvern",               Tuple.Create(15,1, 20) },
            { "The Puppet Master",              Tuple.Create(15,1, 20) },
            { "Jon Bilgewater the Pirate King", Tuple.Create(15,1, 20) },
            { "Epic Larva",                     Tuple.Create(15,1, 20) },
            { "Epic Mama Megamoth",             Tuple.Create(15,1, 20) },
            { "Murderous Megamoth",             Tuple.Create(15,1, 20) },
            { "Son of Arachna",                 Tuple.Create(15,1, 20) },
            { "Golden Oryx Effigy",             Tuple.Create(15,1, 20) },
            { "Murderous Megamoth Deux",        Tuple.Create(15,1, 20) },
            { "Lord Ruthven Deux",              Tuple.Create(15,1, 20) },
            { "NM Green Dragon God Deux",       Tuple.Create(15,1, 20) },
            { "Archdemon Malphas Deux",         Tuple.Create(15,1, 20) },
            { "Stheno the Snake Queen Deux",    Tuple.Create(15,1, 20) },
            { "Golden Oryx Effigy Deux",        Tuple.Create(15,1, 20) },
            { "Oryx the Mad God Deux",          Tuple.Create(15,1, 20) },
            { "vlntns Botany Bella",            Tuple.Create(15,1, 20) },
            { "md1 Head of Shaitan",            Tuple.Create(15,1, 20) },
            { "Queen of Hearts",                Tuple.Create(15,1, 20) },
            { "Fabian the King of the Ossis",   Tuple.Create(15,1, 20) },
            { "TestChicken 2",                  Tuple.Create(15,1, 20) },

            // special events
            { "Megaman",                        Tuple.Create(50,20, 20) },
            { "Boshy",                          Tuple.Create(50,20, 20) },
            { "The Kid",                        Tuple.Create(50,20, 20) },
            { "Sanic",                          Tuple.Create(50,20, 20) }

        };

        Entity FindQuest(Position? destination = null)
        {
            Entity ret = null;
            double? bestScore = null;
            var pX = !destination.HasValue ? X : destination.Value.X;
            var pY = !destination.HasValue ? Y : destination.Value.Y;
            
            foreach (var i in Owner.Quests.Values
                .OrderBy(quest => MathsUtils.DistSqr(quest.X, quest.Y, pX, pY)))
            {
                if (i.ObjectDesc == null || !i.ObjectDesc.Quest) continue;

                Tuple<int, int, int> x;
                if (!QuestDat.TryGetValue(i.ObjectDesc.ObjectId, out x))
                    continue;

                if ((Level >= x.Item2 && Level <= x.Item3))
                {
                    var score = (20 - Math.Abs((i.ObjectDesc.Level ?? 0) - Level)) * x.Item1 -   //priority * level diff
                            this.Dist(i) / 100;    //minus 1 for every 100 tile distance
                    if (bestScore == null || score > bestScore)
                    {
                        bestScore = score;
                        ret = i;
                    }
                }
            }
            return ret;
        }

        Entity questEntity;
        public Entity Quest { get { return questEntity; } }
        public void HandleQuest(RealmTime time, bool force = false, Position? destination = null)
        {
            if (force || time.TickCount % 500 == 0 || questEntity == null || questEntity.Owner == null)
            {
                var newQuest = FindQuest(destination);
                if (newQuest != null && newQuest != questEntity)
                {
                    Owner.Timers.Add(new WorldTimer(100, (w, t) =>
                    {
                        _client.SendPacket(new QuestObjId()
                        {
                            ObjectId = newQuest.Id
                        });
                    }));
                    questEntity = newQuest;
                }
            }
        }

        public void CalculateFame()
        {
            var newFame = (Experience < 200 * 1000) ?
                Experience / 1000 :
                200 + (Experience - 200 * 1000) / 1000;

            if (newFame == Fame) 
                return;

            var stats = FameCounter.ClassStats[ObjectType];
            var newGoal = GetFameGoal(stats.BestFame > newFame ? stats.BestFame : newFame);

            if (newGoal > FameGoal)
            {
                BroadcastSync(new Notification()
                {
                    ObjectId = Id,
                    Color = new ARGB(0xFF00FF00),
                    Message = "{\"key\": \"server.class_quest_complete\"}"
                }, p => this.DistSqr(p) < RadiusSqr);
                Stars = GetStars();
            }
            else if (newFame != Fame)
            {
                BroadcastSync(new Notification()
                {
                    ObjectId = Id,
                    Color = new ARGB(0xFFE25F00),
                    Message = "+" + (newFame - Fame) + "Fame"
                }, p => this.DistSqr(p) < RadiusSqr);
            }

            Fame = newFame;
            FameGoal = newGoal;
        }

        bool CheckLevelUp()
        {
            if (Experience - GetLevelExp(Level) >= ExperienceGoal && Level < 20)
            {
                Level++;
                ExperienceGoal = GetExpGoal(Level);
                var statInfo = Manager.Resources.GameData.Classes[ObjectType].Stats;
                var rand = new Random();
                for (var i = 0; i < statInfo.Length; i++)
                {
                    var min = statInfo[i].MinIncrease;
                    var max = statInfo[i].MaxIncrease + 1;
                    Stats.Base[i] += rand.Next(min, max);
                    if (Stats.Base[i] > statInfo[i].MaxValue)
                        Stats.Base[i] = statInfo[i].MaxValue;
                }
                HP = Stats[0];
                MP = Stats[1];

                if (Level == 20)
                {
                    foreach (var i in Owner.Players.Values)
                    {
                        i.SendInfo(Name + " achieved level 20");
                    }
                }
                else
                {
                    // to get exp scaled to new exp goal
                    InvokeStatChange(StatsType.Experience, Experience - GetLevelExp(Level), true);
                }
                      
                questEntity = null;

                return true;
            }
            CalculateFame();
            return false;
        }

        public bool EnemyKilled(Enemy enemy, int exp, bool killer)
        {
            if (enemy == questEntity)
                BroadcastSync(new Notification()
                {
                    ObjectId = Id,
                    Color = new ARGB(0xFF00FF00),
                    Message = "{\"key\":\"server.quest_complete\"}"
                }, p => this.DistSqr(p) < RadiusSqr);
            if (exp != 0)
            {
                Experience += exp;
            }
            FameCounter.Killed(enemy, killer);
            return CheckLevelUp();
        }
    }
}
