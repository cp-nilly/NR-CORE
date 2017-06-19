using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common.resources;
using StackExchange.Redis;
using wServer.logic.loot;
using wServer.networking;
using wServer.networking.packets.outgoing;
using wServer.networking.packets.outgoing.arena;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm.worlds.logic
{
    class DeathArena : World
    {
        public enum ArenaState
        {
            NotStarted,
            CountDown,
            Start,
            Rest,
            Spawn,
            Fight,
            Awaiting,
            Ended
        }

        private enum CountDownState
        {
            Notify60,
            Notify30,
            StartGame,
            Done
        }

        // random enemies used for all levels
        private readonly string[] _randomEnemies =
        {
            "Djinn", "Beholder", "White Demon of the Abyss", "Flying Brain", "Slime God",
            "Native Sprite God", "Ent God", "Medusa", "Ghost God", "Leviathan", "Mini Bot"
        };

        // _bossLevel defines the wave at which the random bosses change
        // _randomBosses defines the set of bosses that are used for a particular boss level
        private int _bossLevel = 0;
        private readonly int[] _changeBossLevel = new int[] { 0, 4, 10, 16, 23, 30 /*0, 1, 6, 11, 16, 21, 36*/ };

        private readonly string[][] _randomBosses = new string[][]
        {
            new string[]
            {
                "Dreadstump the Pirate King", "Red Demon", "Phoenix Lord", "Henchman of Oryx", "Mama Megamoth"
            },
            new string[]
            {
                "Stheno the Snake Queen", "Archdemon Malphas", "Septavius the Ghost God",
                "Limon the Sprite God", "Dr Terrible", "Gigacorn"
            },
            new string[]
            {
                "Thessal the Mermaid Goddess", "Crystal Prisoner", "Lord of the Lost Lands",
                "Septavius the Ghost God", "Limon the Sprite God", "Dr Terrible", "Gigacorn",
                "Archdemon Malphas", "Epic Larva"
            },
            new string[]
            {
                "Tomb Support", "Tomb Defender", "Tomb Attacker", "Oryx the Mad God 2", 
                "Grand Sphinx", "Queen of Hearts", "Thessal the Mermaid Goddess", "Gigacorn",
                "Crystal Prisoner", "Lord of the Lost Lands", "Epic Larva", "TestChicken 2"
            },
            new string[]
            {
                "Thessal the Mermaid Goddess", "Tomb Support", "Tomb Defender", "Tomb Attacker", 
                "Queen of Hearts", "Grand Sphinx", "Oryx the Mad God 2", "Cube God", "Skull Shrine",
                "TestChicken 2"
            },
            new string[]
            {
                "Oryx the Mad God 3", "Phoenix Wright", "Boshy", "Cube God", "Skull Shrine", "PacMan"
            }
        };

        private readonly new Dictionary<int, string[]> _waveRewards = new Dictionary<int, string[]>
        {
            {  5, new string[] {"Loot Drop Potion 5 min", "Yellow Gumball", "Red Gumball", "Purple Gumball", "Blue Gumball", "Green Gumball"} },
            { 10, new string[] {"Tincture of Fear", "Tincture of Courage", "Holy Water"} },
            { 15, new string[] {"Tincture of Dexterity", "Tincture of Life", "Tincture of Mana", "Tincture of Defense"} },
            { 20, new string[] {"Potion of Life", "Potion of Mana"} },
            { 25, new string[] {"Effusion of Dexterity", "Effusion of Life", "Effusion of Mana", "Effusion of Defense"} },
            { 30, new string[] {"Jeebs' Arena Key", "PacMan Key", "Loot Drop Potion 5 min"} },
            { 35, new string[] {"Backpack", "Cyanbag Emote", "Bluebag Emote", "Whitebag Emote" } }
        };

        private ArenaState _arenaState;
        private CountDownState _countDown;

        private int _wave;
        private long _restTime;
        private long _time;
        private int _startingPlayers;
        private int _difficulty;

        private List<IntPoint> _outerSpawn;
        private List<IntPoint> _centralSpawn;

        public static DeathArena Instance { get; private set; }
        public ArenaState CurrentState => _arenaState;

        public DeathArena(ProtoWorld proto, Client client = null)
            : base(proto)
        {
            Instance = this;
            WorldLoot.Add(new ItemLoot("Oryx's Arena Key", 0));
            _arenaState = ArenaState.NotStarted;
            _wave = 1;
        }

        protected override void Init()
        {
            base.Init();

            if (IsLimbo) return;

            InitArena();
        }

        private void InitArena()
        {
            // setup spawn regions
            _outerSpawn = new List<IntPoint>();
            _centralSpawn = new List<IntPoint>();
            var w = Map.Width;
            var h = Map.Height;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (Map[x, y].Region == TileRegion.Arena_Central_Spawn)
                        _centralSpawn.Add(new IntPoint(x, y));

                    if (Map[x, y].Region == TileRegion.Arena_Edge_Spawn)
                        _outerSpawn.Add(new IntPoint(x, y));
                }
        }

        public override bool AllowedAccess(Client client)
        {
            var isOpen = Manager.Monitor.PortalIsOpen(World.DeathArena);
            return base.AllowedAccess(client) && 
                (isOpen || (client.Account.Admin && Players.Count > 0));
        }

        public override World GetInstance(Client client)
        {
            var manager = client.Manager;

            // join existing open arena if possible
            foreach (var world in manager.Worlds.Values)
            {
                var da = world as DeathArena;
                if (world.IsLimbo || da == null || da._arenaState == ArenaState.Ended)
                    continue;

                return world;
            }

            var arena = Manager.AddWorld(
                new DeathArena(manager.Resources.Worlds[Name], client) { IsLimbo = false });
            Manager.Monitor.UpdateWorldInstance(World.DeathArena, arena);
            return arena;
        }

        private void SpawnEnemies()
        {
            var enemies = new List<string>();
            var r = new Random();

            for (int i = 0; i < Math.Ceiling((_wave + _difficulty) / 2f); i++)
                enemies.Add(_randomEnemies[r.Next(0, _randomEnemies.Length)]);

            foreach (string i in enemies)
            {
                var id = Manager.Resources.GameData.IdToObjectType[i];

                var pos = _outerSpawn[r.Next(0, _outerSpawn.Count)];
                var xloc = pos.X + 0.5f;
                var yloc = pos.Y + 0.5f;

                var enemy = Entity.Resolve(Manager, id);
                enemy.Move(xloc, yloc);
                EnterWorld(enemy);
            }
        }

        private void SpawnBosses()
        {
            var bosses = new List<string>();
            var r = new Random();

            for (int i = 0; i < 1; i++)
                bosses.Add(_randomBosses[_bossLevel][r.Next(0, _randomBosses[_bossLevel].Length)]);

            foreach (string i in bosses)
            {
                ushort id = Manager.Resources.GameData.IdToObjectType[i];

                var pos = _centralSpawn[r.Next(0, _centralSpawn.Count)];
                var xloc = pos.X + 0.5f;
                var yloc = pos.Y + 0.5f;

                var enemy = Entity.Resolve(Manager, id);
                enemy.Move(xloc, yloc);
                EnterWorld(enemy);
            }
        }

        public override void Tick(RealmTime time)
        {
            base.Tick(time);

            if (IsLimbo)
                return;
            if (Deleted)
            {
                LockPortal();
                return;
            }

            _time += time.ElaspedMsDelta;

            switch (_arenaState)
            {
                case ArenaState.NotStarted:
                    _arenaState = ArenaState.CountDown;
                    break;
                case ArenaState.CountDown:
                    Countdown(time);
                    break;
                case ArenaState.Start:
                    Start(time);
                    break;
                case ArenaState.Rest:
                    Rest(time);
                    break;
                case ArenaState.Spawn:
                    Spawn(time);
                    break;
                case ArenaState.Fight:
                    Fight(time);
                    break;
                case ArenaState.Awaiting:
                    WaitForPlayersToLeave(time);
                    break;
                case ArenaState.Ended:
                    break;
                default:
                    _arenaState = ArenaState.Start;
                    break;
            }
        }

        private void Countdown(RealmTime time)
        {
            if (_countDown == CountDownState.Notify60)
            {
                _countDown = CountDownState.Notify30;

                foreach (var w in Manager.Worlds.Values)
                    foreach (var p in w.Players.Values)
                        if (p.Owner == this)
                            p.SendInfo("Game starting in 60 seconds.");
                        else
                        {
                            p.SendError("<ANNOUNCEMENT> Oryx's Arena closing in 1 min. Type /oa to join.");
                            if (p.Owner is Nexus || p.Owner is Vault)
                            {
                                p.Client.SendPacket(new GlobalNotification
                                {
                                    Type = GlobalNotification.ADD_ARENA,
                                    Text = "{\"name\":\"Oryx Arena\",\"open\":true}"
                                });
                            }
                        }
            }

            if (_countDown == CountDownState.Notify30 && _time > 30000)
            {
                _countDown = CountDownState.StartGame;

                foreach (var w in Manager.Worlds.Values)
                    foreach (var p in w.Players.Values)
                        if (p.Owner == this)
                            p.SendInfo("Game starting in 30 seconds.");
                        else
                            p.SendError("<ANNOUNCEMENT> Oryx's Arena closing in 30 seconds. Type /oa to join.");
            }

            if (_countDown == CountDownState.StartGame && _time > 60000)
            {
                _countDown = CountDownState.Done;
                _arenaState = ArenaState.Start;
                _time = 0;
                _startingPlayers = Players.Count;
                _difficulty = Players.Count(p => p.Value.Level >= 15);

                Manager.Monitor.ClosePortal(World.DeathArena);

                foreach (var p in Manager.Worlds.Values.SelectMany(w => w.Players.Values).Where(p => p.Owner is Nexus || p.Owner is Vault))
                    p.Client.SendPacket(new GlobalNotification
                    {
                        Type = GlobalNotification.ADD_ARENA,
                        Text = "{\"name\":\"Oryx Arena\",\"open\":false}"
                    });
            }
        }

        private void WaitForPlayersToLeave(RealmTime time)
        {
            if (Players.Count == 0)
            {
                _arenaState = ArenaState.Ended;
                LockPortal();
            }
        }

        private void Start(RealmTime time)
        {
            _arenaState = ArenaState.Rest;
            Rest(time, true);
        }

        private void Rest(RealmTime time, bool recover = false)
        {
            if (recover)
            {
                foreach (var plr in Players.Values)
                {
                    plr.ApplyConditionEffect(ConditionEffectIndex.Healing, 5000);
                    if (plr.HasConditionEffect(ConditionEffects.Hexed))
                    {
                        plr.ApplyConditionEffect(new ConditionEffect()
                        {
                            Effect = ConditionEffectIndex.Speedy,
                            DurationMS = 0
                        });
                    }
                    plr.ApplyConditionEffect(Player.NegativeEffs);
                }

                BroadcastPacket(new ImminentArenaWave()
                {
                    CurrentRuntime = (int)_time,
                    Wave = _wave
                }, null);

                HandleWaveRewards();

                return;
            }

            if (_time - _restTime < 5000)
                return;

            _arenaState = ArenaState.Spawn;
        }

        private void Spawn(RealmTime time)
        {
            SpawnEnemies();
            SpawnBosses();
            _arenaState = ArenaState.Fight;
        }

        private void Fight(RealmTime time)
        {
            if (Players.Count(p => !p.Value.Client.Account.Admin) <= 1)
            {
                var plr = Players.Values.SingleOrDefault(p => !p.Client.Account.Admin);
                if (plr != null)
                    Manager.Chat.Announce(
                        "Death eludes " + plr.Name + 
                        ". Congratulations. (Wave: " + _wave + 
                        ", Starting Players: " + _startingPlayers + ")");

                foreach (var p in Manager.Worlds.Values.SelectMany(w => w.Players.Values).Where(p => p.Owner is Nexus))
                    p.Client.SendPacket(new GlobalNotification
                    {
                        Type = GlobalNotification.DELETE_ARENA,
                        Text = "Oryx Arena"
                    });

                _arenaState = ArenaState.Awaiting;
                return;
            }

            if (!Enemies.Any(e => e.Value.ObjectDesc.Enemy && !e.Value.Spawned))
            {
                _wave++;
                _restTime = _time;
                _arenaState = ArenaState.Rest;

                if (_bossLevel + 1 < _changeBossLevel.Length &&
                    _changeBossLevel[_bossLevel + 1] <= _wave)
                    _bossLevel++;

                Rest(time, true);
            }
        }

        private void HandleWaveRewards()
        {
            if (!_waveRewards.ContainsKey(_wave))
                return;

            // initialize reward items
            var gameData = Manager.Resources.GameData;
            var items = new List<Item>();
            foreach (var reward in _waveRewards[_wave])
            {
                ushort itemType;
                Item item = null;

                if (!gameData.IdToObjectType.TryGetValue(reward, out itemType))
                    continue;

                if (!gameData.Items.TryGetValue(itemType, out item))
                    continue;

                items.Add(item);
            }

            if (items.Count <= 0)
                return;

            // hand out rewards
            var r = new Random();
            foreach (var player in Players.Values.Where(p => !p.HasConditionEffect(ConditionEffects.Hidden)))  // no rewards for lurkers
            {
                var item = items[r.Next(0, items.Count)];
                var changes = player.Inventory.CreateTransaction();
                var slot = changes.GetAvailableInventorySlot(item);
                if (slot != -1)
                {
                    changes[slot] = item;
                    Inventory.Execute(changes);
                }
                else
                {
                    player.SendError("[Oryx's Arena] We were unable to give you a reward, your inventory is full :(");
                }
            }
        }

        private void LockPortal()
        {
            Manager.Monitor.RemovePortal(World.DeathArena);
            var portal = new Portal(Manager, 0x023D, null)
            {
                Name = "Oryx's Arena",
                WorldInstance = Manager.Worlds[World.DeathArena]
            };
            var nexus = Manager.GetWorld(World.Nexus) as Nexus;
            var pos = nexus.GetRegionPosition(TileRegion.Arena_Edge_Spawn);
            Manager.Monitor.AddPortal(World.DeathArena, portal, pos, false);
        }

        public override int EnterWorld(Entity entity)
        {
            var ret = base.EnterWorld(entity);

            var p = entity as Player;
            if (p != null)
                p.SendInfo("Welcome to Oryx's Arena. Take caution, you can die here.");
            
            return ret;
        }
    }
}