using System;
using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.realm.entities;
using wServer.realm.setpieces;
using wServer.realm.terrain;
using log4net;
using wServer.realm.worlds;
using wServer.realm.worlds.logic;

namespace wServer.realm
{
    //The mad god who look after the realm
    class Oryx
    {
        public bool Closing;

        private static readonly ILog Log = LogManager.GetLogger(typeof(Oryx));
        
        private readonly Realm _world;
        private readonly Random _rand = new Random();
        private readonly int[] _enemyMaxCounts = new int[12];
        private readonly int[] _enemyCounts = new int[12];
        private long _prevTick;
        private int _tenSecondTick;
        private RealmTime dummy_rt = new RealmTime();
        
        struct TauntData
        {
            public string[] Spawn;
            public string[] NumberOfEnemies;
            public string[] Final;
            public string[] Killed;
        }

        private readonly List<Tuple<string, ISetPiece>> _events = new List<Tuple<string, ISetPiece>>()
        {
            Tuple.Create("Skull Shrine", (ISetPiece) new SkullShrine()),
            Tuple.Create("Cube God", (ISetPiece) new CubeGod()),
            Tuple.Create("Pentaract", (ISetPiece) new Pentaract()),
            Tuple.Create("Grand Sphinx", (ISetPiece) new Sphinx()),
            Tuple.Create("Lord of the Lost Lands", (ISetPiece) new LordoftheLostLands()),
            Tuple.Create("Hermit God", (ISetPiece) new Hermit()),
            Tuple.Create("Ghost Ship", (ISetPiece) new GhostShip()),
            Tuple.Create("Fanatic of Chaos", (ISetPiece) new FanaticofChaos()),
            //Tuple.Create("Dragon Head", (ISetPiece) new RockDragon()),
            Tuple.Create("shtrs Defense System", (ISetPiece) new Avatar()),
            //Tuple.Create("Zombie Horde", (ISetPiece) new ZombieHorde())
            
        };

        private readonly List<Tuple<string, ISetPiece>> _rareEvents = new List<Tuple<string, ISetPiece>>()
        {
            Tuple.Create("Boshy", (ISetPiece) new Boshy()),
            Tuple.Create("Sanic", (ISetPiece) new Sanic()),
            Tuple.Create("The Kid", (ISetPiece) new TheKid()),
            Tuple.Create("Megaman", (ISetPiece) new Megaman())
        };

        #region "Taunt data"
        private static readonly Tuple<string, TauntData>[] CriticalEnemies = new Tuple<string, TauntData>[]
        {
            Tuple.Create("Lich", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "I am invincible while my {COUNT} Liches still stand!",
                    "My {COUNT} Liches will feast on your essence!"
                },
                Final = new string[] {
                    "My final Lich shall consume your souls!",
                    "My final Lich will protect me forever!"
                }
            }),
            Tuple.Create("Ent Ancient", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "Mortal scum! My {COUNT} Ent Ancients will defend me forever!",
                    "My forest of {COUNT} Ent Ancients is all the protection I need!"
                },
                Final = new string[] {
                    "My final Ent Ancient will destroy you all!",
                    "My final Ent Ancient shall crush you!"
                }
            }),
            Tuple.Create("Oasis Giant", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "My {COUNT} Oasis Giants will feast on your flesh!",
                    "You have no hope against my {COUNT} Oasis Giants!"
                },
                Final = new string[] {
                    "A powerful Oasis Giant still fights for me!",
                    "You will never defeat me while an Oasis Giant remains!"
                }
            }),
            Tuple.Create("Phoenix Lord", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "Maggots! My {COUNT} Phoenix Lord will burn you to ash!",
                    "My {COUNT} Phoenix Lords will serve me forever!"
                },
                Final = new string[] {
                    "My final Phoenix Lord will never fall!",
                    "My last Phoenix Lord will blacken your bones!"
                }
            }),
            Tuple.Create("Ghost King", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "My {COUNT} Ghost Kings give me more than enough protection!",
                    "Pathetic humans! My {COUNT} Ghost Kings shall destroy you utterly!"
                },
                Final = new string[] {
                    "A mighty Ghost King remains to guard me!",
                    "My final Ghost King is untouchable!"
                }
            }),
            Tuple.Create("Cyclops God", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "Cretins! I have {COUNT} Cyclops Gods to guard me!",
                    "My {COUNT} powerful Cyclops Gods will smash you!"
                },
                Final = new string[] {
                    "My last Cyclops God will smash you to pieces!",
                    "My final Cyclops God shall crush your puny skulls!"
                }
            }),
            Tuple.Create("Red Demon", new TauntData()
            {
                NumberOfEnemies = new string[] {
                    "Fools! There is no escape from my {COUNT} Red Demons!",
                    "My legion of {COUNT} Red Demons live only to serve me!"
                },
                Final = new string[] {
                    "My final Red Demon is unassailable!",
                    "A Red Demon still guards me!"
                }
            }),
            
            Tuple.Create("Skull Shrine", new TauntData()
            {
                Spawn = new string[] {
                    "Your futile efforts are no match for a Skull Shrine!"
                },
                NumberOfEnemies = new string[] {
                    "Insects!  {COUNT} Skull Shrines still protect me",
                    "You hairless apes will never overcome my {COUNT} Skull Shrines!",
                    "You frail humans will never defeat my {COUNT} Skull Shrines!",
                    "Miserable worms like you cannot stand against my {COUNT} Skull Shrines!",
                    "Imbeciles! My {COUNT} Skull Shrines make me invincible!"
                },
                Final = new string[] {
                    "Pathetic fools!  A Skull Shrine guards me!",
                    "Miserable scum!  My Skull Shrine is invincible!"
                },
                Killed = new string[] {
                    "You defaced a Skull Shrine!  Minions, to arms!",
                    "{PLAYER} razed one of my Skull Shrines -- I WILL HAVE MY REVENGE!",
                    "{PLAYER}, you will rue the day you dared to defile my Skull Shrine!",
                    "{PLAYER}, you contemptible pig! Ruining my Skull Shrine will be the last mistake you ever make!",
                    "{PLAYER}, you insignificant cur! The penalty for destroying a Skull Shrine is death!"
                }
            }),
            Tuple.Create("Cube God", new TauntData()
            {
                Spawn = new string[] {
                    "Your meager abilities cannot possibly challenge a Cube God!"
                },
                NumberOfEnemies = new string[] {
                    "Filthy vermin! My {COUNT} Cube Gods will exterminate you!",
                    "Loathsome slugs! My {COUNT} Cube Gods will defeat you!",
                    "You piteous cretins! {COUNT} Cube Gods still guard me!",
                    "Your pathetic rabble will never survive against my {COUNT} Cube Gods!",
                    "You feeble creatures have no hope against my {COUNT} Cube Gods!"
                },
                Final = new string[] {
                    "Worthless mortals! A mighty Cube God defends me!",
                    "Wretched mongrels!  An unconquerable Cube God is my bulwark!"
                },
                Killed = new string[] {
                    "You have dispatched my Cube God, but you will never escape my Realm!",
                    "{PLAYER}, you pathetic swine! How dare you assault my Cube God?",
                    "{PLAYER}, you wretched dog! You killed my Cube God!",
                    "{PLAYER}, you may have destroyed my Cube God but you will never defeat me!",
                    "I have many more Cube Gods, {PLAYER}!",
                }
            }),
            Tuple.Create("Pentaract", new TauntData()
            {
                Spawn = new string[] {
                    "Behold my Pentaract, and despair!"
                },
                NumberOfEnemies = new string[] {
                    "Wretched creatures! {COUNT} Pentaracts remain!",
                    "You detestable humans will never defeat my {COUNT} Pentaracts!",
                    "My {COUNT} Pentaracts will protect me forever!",
                    "Your weak efforts will never overcome my {COUNT} Pentaracts!",
                    "Defiance is useless! My {COUNT} Pentaracts will crush you!"
                },
                Final = new string[] {
                    "I am invincible while my Pentaract stands!",
                    "Ignorant fools! A Pentaract guards me still!"
                },
                Killed = new string[] {
                    "That was but one of many Pentaracts!",
                    "You have razed my Pentaract, but you will die here in my Realm!",
                    "{PLAYER}, you lowly scum!  You'll regret that you ever touched my Pentaract!",
                    "{PLAYER}, you flea-ridden animal! You destoryed my Pentaract!",
                    "{PLAYER}, by destroying my Pentaract you have sealed your own doom!"
                }
            }),
            Tuple.Create("Grand Sphinx", new TauntData()
            {
                Spawn = new string[] {
                    "At last, a Grand Sphinx will teach you to respect!"
                },
                NumberOfEnemies = new string[] {
                    "You dull-spirited apes! You shall pose no challenge for {COUNT} Grand Sphinxes!",
                    "Regret your choices, blasphemers! My {COUNT} Grand Sphinxes will teach you respect!",
                    "My {COUNT} Grand Sphinxes protect my Chamber with their lives!",
                    "My Grand Sphinxes will bewitch you with their beauty!"
                },
                Final = new string[] {
                    "A Grand Sphinx is more than a match for this rabble.",
                    "You festering rat-catchers! A Grand Sphinx will make you doubt your purpose!",
                    "Gaze upon the beauty of the Grand Sphinx and feel your last hopes drain away."
                },
                Killed = new string[] {
                    "The death of my Grand Sphinx shall be avenged!",
                    "My Grand Sphinx, she was so beautiful. I will kill you myself, {PLAYER}!",
                    "My Grand Sphinx had lived for thousands of years! You, {PLAYER}, will not survive the day!",
                    "{PLAYER}, you up-jumped goat herder! You shall pay for defeating my Grand Sphinx!",
                    "{PLAYER}, you pestiferous lout! I will not forget what you did to my Grand Sphinx!",
                    "{PLAYER}, you foul ruffian! Do not think I forget your defiling of my Grand Sphinx!"
                }
            }),
            Tuple.Create("Lord of the Lost Lands", new TauntData()
            {
                Spawn = new string[] {
                    "Cower in fear of my Lord of the Lost Lands!",
                    "My Lord of the Lost Lands will make short work of you!"
                },
                NumberOfEnemies = new string[] {
                    "Cower before your destroyer! You stand no chance against {COUNT} Lords of the Lost Lands!",
                    "Your pathetic band of fighters will be crushed under the might feet of my {COUNT} Lords of the Lost Lands!",
                    "Feel the awesome might of my {COUNT} Lords of the Lost Lands!",
                    "Together, my {COUNT} Lords of the Lost Lands will squash you like a bug!",
                    "Do not run! My {COUNT} Lords of the Lost Lands only wish to greet you!"
                },
                Final = new string[] {
                    "Give up now! You stand no chance against a Lord of the Lost Lands!",
                    "Pathetic fools! My Lord of the Lost Lands will crush you all!",
                    "You are nothing but disgusting slime to be scraped off the foot of my Lord of the Lost Lands!"
                },
                Killed = new string[] {
                    "How dare you foul-mouthed hooligans treat my Lord of the Lost Lands with such indignity!",
                    "What trickery is this?! My Lord of the Lost Lands was invincible!",
                    "You win this time, {PLAYER}, but mark my words:  You will fall before the day is done.",
                    "{PLAYER}, I will never forget you exploited my Lord of the Lost Lands' weakness!",
                    "{PLAYER}, you have done me a service! That Lord of the Lost Lands was not worthy of serving me.",
                    "You got lucky this time {PLAYER}, but you stand no chance against me!",
                }
            }),
            Tuple.Create("Hermit God", new TauntData()
            {
                Spawn = new string[] {
                    "My Hermit God's thousand tentacles shall drag you to a watery grave!"
                },
                NumberOfEnemies = new string[] {
                    "You will make a tasty snack for my Hermit Gods!",
                    "I will enjoy watching my {COUNT} Hermit Gods fight over your corpse!"
                },
                Final = new string[] {
                    "You will be pulled to the bottom of the sea by my mighty Hermit God.",
                    "Flee from my Hermit God, unless you desire a watery grave!",
                    "My Hermit God awaits more sacrifices for the majestic Thessal.",
                    "My Hermit God will pull you beneath the waves!",
                    "You will make a tasty snack for my Hermit God!",
                },
                Killed = new string[] {
                    "This is preposterous!  There is no way you could have defeated my Hermit God!",
                    "You were lucky this time, {PLAYER}!  You will rue this day that you killed my Hermit God!",
                    "You naive imbecile, {PLAYER}! Without my Hermit God, Dreadstump is free to roam the seas without fear!",
                    "My Hermit God was more than you'll ever be, {PLAYER}. I will kill you myself!",
                }
            }),
            Tuple.Create("Ghost Ship", new TauntData()
            {
                Spawn = new string[] {
                    "My Ghost Ship will terrorize you pathetic peasants!",
                    "A Ghost Ship has entered the Realm."
                },
                Final = new string[] {
                    "My Ghost Ship will send you to a watery grave.",
                    "You filthy mongrels stand no chance against my Ghost Ship!",
                    "My Ghost Ship's cannonballs will crush your pathetic Knights!"
                },
                Killed = new string[] {
                    "My Ghost Ship will return!",
                    "Alas, my beautiful Ghost Ship has sunk!",
                    "{PLAYER}, you foul creature.  I shall see to your death personally!",
                    "{PLAYER}, has crossed me for the last time! My Ghost Ship shall be avenged.",
                    "{PLAYER} is such a jerk!",
                    "How could a creature like {PLAYER} defeat my dreaded Ghost Ship?!",
                    "The spirits of the sea will seek revenge on your worthless soul, {PLAYER}!"
                }
            }),
            Tuple.Create("Dragon Head", new TauntData()
            {
                Spawn = new string[] {
                    "The Rock Dragon has been summoned.",
                    "Beware my Rock Dragon. All who face him shall perish.",
                },
                Final = new string[] {
                    "My Rock Dragon will end your pathetic existence!",
                    "Fools, no one can withstand the power of my Rock Dragon!",
                    "The Rock Dragon will guard his post until the bitter end.",
                    "The Rock Dragon will never let you enter the Lair of Draconis.",
                },
                Killed = new string[] {
                    "My Rock Dragon will return!",
                    "The Rock Dragon has failed me!",
                    "{PLAYER} knows not what he has done.  That Lair was guarded for the Realm's own protection!",
                    "{PLAYER}, you have angered me for the last time!",
                    "{PLAYER} will never survive the trials that lie ahead.",
                    "A filthy weakling like {PLAYER} could never have defeated my Rock Dragon!!!",
                    "You shall not live to see the next sunrise, {PLAYER}!",
                }
            }),
            Tuple.Create("shtrs Defense System", new TauntData()
            {
                Spawn = new string[] {
                    "The Shatters has been discovered!?!",
                    "The Forgotten King has raised his Avatar!",
                },
                Final = new string[] {
                    "Attacking the Avatar of the Forgotten King would be...unwise.",
                    "Kill the Avatar, and you risk setting free an abomination.",
                    "Before you enter the Shatters you must defeat the Avatar of the Forgotten King!",
                },
                Killed = new string[] {
                    "The Avatar has been defeated!",
                    "How could simpletons kill The Avatar of the Forgotten King!?",
                    "{PLAYER} has unleashed an evil upon this Realm.",
                    "{PLAYER}, you have awoken the Forgotten King. Enjoy a slow death!",
                    "{PLAYER} will never survive what lies in the depths of the Shatters.",
                    "Enjoy your little victory while it lasts, {PLAYER}!"
                }
            }),
            Tuple.Create("Zombie Horde", new TauntData()
            {
                Spawn = new string[] {
                    "At last, my Zombie Horde will eradicate you like the vermin that you are!",
                    "The full strength of my Zombie Horde has been unleashed!",
                    "Let the apocalypse begin!",
                    "Quiver with fear, peasants, my Zombie Horde has arrived!",
                },
                Final = new string[] {
                    "A small taste of my Zombie Horde should be enough to eliminate you!",
                    "My Zombie Horde will teach you the meaning of fear!",
                },
                Killed = new string[] {
                    "The death of my Zombie Horde is unacceptable! You will pay for your insolence!",
                    "{PLAYER}, I will kill you myself and turn you into the newest member of my Zombie Horde!",
                }
            }),
            Tuple.Create("Boshy", new TauntData()),
            Tuple.Create("The Kid", new TauntData()),
            Tuple.Create("Sanic", new TauntData())
        };
        #endregion

        #region "Spawn data"
        private static readonly Dictionary<WmapTerrain, Tuple<int, Tuple<string, double>[]>> RegionMobs = 
            new Dictionary<WmapTerrain, Tuple<int, Tuple<string, double>[]>>()
        {
            { WmapTerrain.ShoreSand, Tuple.Create(
                100, new []
                {
                    Tuple.Create("Pirate", 0.3),
                    Tuple.Create("Piratess", 0.1),
                    Tuple.Create("Snake", 0.2),
                    Tuple.Create("Scorpion Queen", 0.4),
                })
            },
            { WmapTerrain.ShorePlains, Tuple.Create(
                150, new []
                {
                    Tuple.Create("Bandit Leader", 0.4),
                    Tuple.Create("Red Gelatinous Cube", 0.2),
                    Tuple.Create("Purple Gelatinous Cube", 0.2),
                    Tuple.Create("Green Gelatinous Cube", 0.2),
                })
            },
            { WmapTerrain.LowPlains, Tuple.Create(
                200, new []
                {
                    Tuple.Create("Hobbit Mage", 0.5),
                    Tuple.Create("Undead Hobbit Mage", 0.4),
                    Tuple.Create("Sumo Master", 0.1),
                })
            },
            { WmapTerrain.LowForest, Tuple.Create(
                200, new []
                {
                    Tuple.Create("Elf Wizard", 0.2),
                    Tuple.Create("Goblin Mage", 0.2),
                    Tuple.Create("Easily Enraged Bunny", 0.3),
                    Tuple.Create("Forest Nymph", 0.3),
                })
            },
            { WmapTerrain.LowSand, Tuple.Create(
                200, new []
                {
                    Tuple.Create("Sandsman King", 0.4),
                    Tuple.Create("Giant Crab", 0.2),
                    Tuple.Create("Sand Devil", 0.4),
                })
            },
            { WmapTerrain.MidPlains, Tuple.Create(
                150, new []
                {
                    Tuple.Create("Fire Sprite", 0.1),
                    Tuple.Create("Ice Sprite", 0.1),
                    Tuple.Create("Magic Sprite", 0.1),
                    Tuple.Create("Pink Blob", 0.07),
                    Tuple.Create("Gray Blob", 0.07),
                    Tuple.Create("Earth Golem", 0.04),
                    Tuple.Create("Paper Golem", 0.04),
                    Tuple.Create("Big Green Slime", 0.08),
                    Tuple.Create("Swarm", 0.05),
                    Tuple.Create("Wasp Queen", 0.2),
                    Tuple.Create("Shambling Sludge", 0.03),
                    Tuple.Create("Orc King", 0.06),
                    Tuple.Create("Candy Gnome", 0.02)
                })
            },
            { WmapTerrain.MidForest, Tuple.Create(
                150, new []
                {
                    Tuple.Create("Dwarf King", 0.3),
                    Tuple.Create("Metal Golem", 0.05),
                    Tuple.Create("Clockwork Golem", 0.05),
                    Tuple.Create("Werelion", 0.1),
                    Tuple.Create("Horned Drake", 0.3),
                    Tuple.Create("Red Spider", 0.1),
                    Tuple.Create("Black Bat", 0.1)
                })
            },
            { WmapTerrain.MidSand, Tuple.Create(
                300, new []
                {
                    Tuple.Create("Desert Werewolf", 0.25),
                    Tuple.Create("Fire Golem", 0.1),
                    Tuple.Create("Darkness Golem", 0.1),
                    Tuple.Create("Sand Phantom", 0.2),
                    Tuple.Create("Nomadic Shaman", 0.25),
                    Tuple.Create("Great Lizard", 0.1),
                })
            },
            { WmapTerrain.HighPlains, Tuple.Create(
                300, new []
                {
                    Tuple.Create("Shield Orc Key", 0.2),
                    Tuple.Create("Urgle", 0.2),
                    Tuple.Create("Undead Dwarf God", 0.6)
                })
            },
            { WmapTerrain.HighForest, Tuple.Create(
                300, new []
                {
                    Tuple.Create("Ogre King", 0.4),
                    Tuple.Create("Dragon Egg", 0.1),
                    Tuple.Create("Lizard God", 0.5),
                    Tuple.Create("Beer God", 0.1)
                })
            },
            { WmapTerrain.HighSand, Tuple.Create(
                250, new []
                {
                    Tuple.Create("Minotaur", 0.4),
                    Tuple.Create("Flayer God", 0.4),
                    Tuple.Create("Flamer King", 0.2)
                })
            },
            { WmapTerrain.Mountains, Tuple.Create(
                100, new []
                {
                    Tuple.Create("White Demon", 0.1),
                    Tuple.Create("Sprite God", 0.11),
                    Tuple.Create("Medusa", 0.1),
                    Tuple.Create("Ent God", 0.1),
                    Tuple.Create("Beholder", 0.1),
                    Tuple.Create("Flying Brain", 0.1),
                    Tuple.Create("Slime God", 0.09),
                    Tuple.Create("Ghost God", 0.09),
                    Tuple.Create("Rock Bot", 0.05),
                    Tuple.Create("Djinn", 0.09),
                    Tuple.Create("Leviathan", 0.09),
                    Tuple.Create("Arena Headless Horseman", 0.04)
                })
            },
        };
        #endregion

        public Oryx(Realm world)
        {
            _world = world;
            Init();
        }

        private static double GetUniform(Random rand)
        {
            // 0 <= u < 2^32
            var u = (uint)(rand.NextDouble() * uint.MaxValue);
            // The magic number below is 1/(2^32 + 2).
            // The result is strictly between 0 and 1.
            return (u + 1.0) * 2.328306435454494e-10;
        }

        private static double GetNormal(Random rand)
        {
            // Use Box-Muller algorithm
            var u1 = GetUniform(rand);
            var u2 = GetUniform(rand);
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;
            return r * Math.Sin(theta);
        }

        private static double GetNormal(Random rand, double mean, double standardDeviation)
        {
            return mean + standardDeviation * GetNormal(rand);
        }

        private ushort GetRandomObjType(IEnumerable<Tuple<string, double>> dat)
        {
            double p = _rand.NextDouble();
            double n = 0;
            ushort objType = 0;
            foreach (var k in dat)
            {
                n += k.Item2;
                if (n > p)
                {
                    objType = _world.Manager.Resources.GameData.IdToObjectType[k.Item1];
                    break;
                }
            }
            return objType;
        }

        private int Spawn(ObjectDesc desc, WmapTerrain terrain, int w, int h)
        {
            Entity entity;

            var ret = 0;
            var pt = new IntPoint();

            if (desc.Spawn != null)
            {
                var num = (int) GetNormal(_rand, desc.Spawn.Mean, desc.Spawn.StdDev);
                
                if (num > desc.Spawn.Max)
                    num = desc.Spawn.Max;
                else if (num < desc.Spawn.Min) 
                    num = desc.Spawn.Min;

                do
                {
                    pt.X = _rand.Next(0, w);
                    pt.Y = _rand.Next(0, h);
                } while (_world.Map[pt.X, pt.Y].Terrain != terrain ||
                         !_world.IsPassable(pt.X, pt.Y) ||
                         _world.AnyPlayerNearby(pt.X, pt.Y));

                for (var k = 0; k < num; k++)
                {
                    entity = Entity.Resolve(_world.Manager, desc.ObjectType);
                    entity.Move(
                        pt.X + (float)(_rand.NextDouble() * 2 - 1) * 5,
                        pt.Y + (float)(_rand.NextDouble() * 2 - 1) * 5);
                    (entity as Enemy).Terrain = terrain;
                    _world.EnterWorld(entity);
                    ret++;
                }
                return ret;
            }

            do
            {
                pt.X = _rand.Next(0, w);
                pt.Y = _rand.Next(0, h);
            } while (_world.Map[pt.X, pt.Y].Terrain != terrain ||
                        !_world.IsPassable(pt.X, pt.Y) ||
                        _world.AnyPlayerNearby(pt.X, pt.Y));

            entity = Entity.Resolve(_world.Manager, desc.ObjectType);
            entity.Move(pt.X, pt.Y);
            (entity as Enemy).Terrain = terrain;
            _world.EnterWorld(entity);
            ret++;
            return ret;
        }

        public void Init()
        {
            Log.InfoFormat("Oryx is controlling world {0}({1})...", _world.Id, _world.Name);

            var w = _world.Map.Width;
            var h = _world.Map.Height;
            var stats = new int[12];
            
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var tile = _world.Map[x, y];
                    if (tile.Terrain != WmapTerrain.None)
                        stats[(int)tile.Terrain - 1]++;
                }

            Log.Info("Spawning minions...");
            foreach (var i in RegionMobs)
            {
                var terrain = i.Key;
                var idx = (int)terrain - 1;
                var enemyCount = stats[idx] / i.Value.Item1;
                _enemyMaxCounts[idx] = enemyCount;
                _enemyCounts[idx] = 0;
                
                for (var j = 0; j < enemyCount; j++)
                {
                    var objType = GetRandomObjType(i.Value.Item2);
                    
                    if (objType == 0) 
                        continue;

                    _enemyCounts[idx] += Spawn(_world.Manager.Resources.GameData.ObjectDescs[objType], terrain, w, h);
                    
                    if (_enemyCounts[idx] >= enemyCount)
                        break;
                }
            }

            Log.Info("Oryx is done.");
        }

        public void Tick(RealmTime time)
        {
            if (time.TotalElapsedMs - _prevTick <= 10000)
                return;

            if (_tenSecondTick % 2 == 0)
                HandleAnnouncements();

            if (_tenSecondTick % 6 == 0)
                EnsurePopulation();

            _tenSecondTick++;
            _prevTick = time.TotalElapsedMs;
        }

        private void EnsurePopulation()
        {
            Log.Info("Oryx is controlling population...");

            RecalculateEnemyCount();

            var state = new int[12];
            var diff = new int[12];
            var c = 0;

            for (var i = 0; i < state.Length; i++)
            {
                if (_enemyCounts[i] > _enemyMaxCounts[i] * 1.5) //Kill some
                {
                    state[i] = 1;
                    diff[i] = _enemyCounts[i] - _enemyMaxCounts[i];
                    c++;
                    continue;
                }
                
                if (_enemyCounts[i] < _enemyMaxCounts[i] * 0.75) //Add some
                {
                    state[i] = 2;
                    diff[i] = _enemyMaxCounts[i] - _enemyCounts[i];
                    continue;
                }

                state[i] = 0;
            }

            foreach (var i in _world.Enemies) //Kill
            {
                var idx = (int) i.Value.Terrain - 1;

                if (idx == -1 || state[idx] == 0 ||
                    i.Value.GetNearestEntity(10, true) != null ||
                    diff[idx] == 0)
                    continue;

                if (state[idx] == 1)
                {
                    _world.LeaveWorld(i.Value);
                    diff[idx]--;
                    if (diff[idx] == 0)
                        c--;
                }
                
                if (c == 0) 
                    break;
            }

            var w = _world.Map.Width;
            var h = _world.Map.Height;
            
            for (var i = 0; i < state.Length; i++) //Add
            {
                if (state[i] != 2) 
                    continue;

                var x = diff[i];
                var t = (WmapTerrain)(i + 1);
                for (var j = 0; j < x; )
                {
                    var objType = GetRandomObjType(RegionMobs[t].Item2);
                    
                    if (objType == 0)
                        continue;

                    j += Spawn(_world.Manager.Resources.GameData.ObjectDescs[objType], t, w, h);
                }
            }
            RecalculateEnemyCount();

            //GC.Collect();
            Log.Info("Oryx is back to sleep.");
        }

        private void RecalculateEnemyCount()
        {
            for (var i = 0; i < _enemyCounts.Length; i++)
                _enemyCounts[i] = 0;

            foreach (var i in _world.Enemies)
            {
                if (i.Value.Terrain == WmapTerrain.None) 
                    continue;

                _enemyCounts[(int)i.Value.Terrain - 1]++;
            }
        }

        private void HandleAnnouncements()
        {
            if (_world.Closed)
                return;

            var taunt = CriticalEnemies[_rand.Next(0, CriticalEnemies.Length)];
            var count = 0;
            foreach (var i in _world.Enemies)
            {
                var desc = i.Value.ObjectDesc;
                if (desc == null || desc.ObjectId != taunt.Item1)
                    continue;
                count++;
            }

            if (count == 0) 
                return;
            
            if ((count == 1 && taunt.Item2.Final != null) ||
                (taunt.Item2.Final != null && taunt.Item2.NumberOfEnemies == null))
            {
                var arr = taunt.Item2.Final;
                var msg = arr[_rand.Next(0, arr.Length)];
                BroadcastMsg(msg);
            }
            else
            {
                var arr = taunt.Item2.NumberOfEnemies;
                if (arr == null)
                    return;
                
                var msg = arr[_rand.Next(0, arr.Length)];
                msg = msg.Replace("{COUNT}", count.ToString());
                BroadcastMsg(msg);
            }
        }

        private void BroadcastMsg(string message)
        {
            _world.Manager.Chat.Oryx(_world, message);
        }

        public void OnPlayerEntered(Player player)
        {
            player.SendInfo("Welcome to Realm of the Mad God");
            player.SendEnemy("Oryx the Mad God", "You are food for my minions!");
            player.SendInfo("Use [WASDQE] to move; click to shoot!");
            player.SendInfo("Type \"/help\" for more help");
        }

        private void SpawnEvent(string name, ISetPiece setpiece)
        {
            var pt = new IntPoint();
            do
            {
                pt.X = _rand.Next(0, _world.Map.Width);
                pt.Y = _rand.Next(0, _world.Map.Height);
            } while ((_world.Map[pt.X, pt.Y].Terrain < WmapTerrain.Mountains ||
                      _world.Map[pt.X, pt.Y].Terrain > WmapTerrain.MidForest) ||
                      !_world.IsPassable(pt.X, pt.Y, true) ||
                      _world.AnyPlayerNearby(pt.X, pt.Y));

            pt.X -= (setpiece.Size - 1) / 2;
            pt.Y -= (setpiece.Size - 1) / 2;
            setpiece.RenderSetPiece(_world, pt);
            Log.InfoFormat("Oryx spawned {0} at ({1}, {2}).", name, pt.X, pt.Y);
        }

        public void OnEnemyKilled(Enemy enemy, Player killer)
        {
            // enemy is quest?
            if (enemy.ObjectDesc == null || !enemy.ObjectDesc.Quest) 
                return;
            
            // is a critical quest?
            TauntData? dat = null;
            foreach (var i in CriticalEnemies)
                if (enemy.ObjectDesc.ObjectId == i.Item1)
                {
                    dat = i.Item2;
                    break;
                }
            if (dat == null) 
                return;

            // has killed message?
            if (dat.Value.Killed != null)
            {
                var arr = dat.Value.Killed;
                if (killer == null)
                    arr = arr.Where(m => !m.Contains("{PLAYER}")).ToArray();

                if (arr.Length > 0)
                {
                    var msg = arr[_rand.Next(0, arr.Length)];
                    msg = msg.Replace("{PLAYER}", (killer != null) ? killer.Name : "");
                    BroadcastMsg(msg);
                }
            }

            // 25% for new event ???
            //if (_rand.NextDouble() > 0.25) 
            //    return;

            var events = _events;
            if (_rand.NextDouble() <= 0.01)
                events = _rareEvents;

            var evt = events[_rand.Next(0, events.Count)];
            var gameData = _world.Manager.Resources.GameData;
            if (gameData.ObjectDescs[gameData.IdToObjectType[evt.Item1]].PerRealmMax == 1)
                events.Remove(evt);
            SpawnEvent(evt.Item1, evt.Item2);

            // new event is critical?
            dat = null;
            foreach (var i in CriticalEnemies)
                if (evt.Item1 == i.Item1)
                {
                    dat = i.Item2;
                    break;
                }
            if (dat == null) 
                return;

            // has spawn message?
            if (dat.Value.Spawn != null)
            {
                var arr = dat.Value.Spawn;
                string msg = arr[_rand.Next(0, arr.Length)];
                BroadcastMsg(msg);
            }

            foreach (var player in _world.Players) {
                player.Value.HandleQuest(dummy_rt, true);
            }
        }

        public void InitCloseRealm()
        {
            Closing = true;
            _world.Manager.Chat.Announce("Realm closing in 1 minute.", true);
            _world.Timers.Add(new WorldTimer(60000, (w, t) => CloseRealm()));
        }

        private void CloseRealm()
        {
            _world.Closed = true;
            BroadcastMsg("I HAVE CLOSED THIS REALM!");
            BroadcastMsg("YOU WILL NOT LIVE TO SEE THE LIGHT OF DAY!");

            _world.Timers.Add(new WorldTimer(22000, (w, t) => SendToCastle()));
        }

        private void SendToCastle()
        {
            BroadcastMsg("MY MINIONS HAVE FAILED ME!");
            BroadcastMsg("BUT NOW YOU SHALL FEEL MY WRATH!");
            BroadcastMsg("COME MEET YOUR DOOM AT THE WALLS OF MY CASTLE!");

            if (_world.Players.Count <= 0)
                return;

            var castle = _world.Manager.AddWorld(
                new worlds.logic.Castle(_world.Manager.Resources.Worlds.Data["Castle"], playersEntering: _world.Players.Count));
            _world.QuakeToWorld(castle);
        }
    }
}
