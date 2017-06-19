using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common.resources;
using wServer.networking;
using wServer.networking.packets.outgoing;
using wServer.networking.packets.outgoing.arena;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm.worlds.logic
{
    class Arena : World
    {
        public enum ArenaState
        {
            NotStarted,
            CountDown,
            Start,
            Rest,
            Spawn,
            Fight
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
            "Native Sprite God", "Ent God", "Medusa", "Ghost God", "Leviathan", "Chaos Guardians",
            "Mini Bot"
        };

        // _bossLevel defines the wave at which the random bosses change
        // _randomBosses defines the set of bosses that are used for a particular boss level
        private int _bossLevel = 0;
        private readonly int[] _changeBossLevel = new int[] {0, 6, 11, 16, 21, 36};

        private readonly string[][] _randomBosses = new string[][]
        {
            new string[] {"Red Demon", "Phoenix Lord", "Henchman of Oryx", "Oryx Brute"},
            new string[] {"Red Demon", "Phoenix Lord", "Henchman of Oryx", "Stheno the Snake Queen"},
            new string[]
            {
                "Stheno the Snake Queen", "Archdemon Malphas", "Septavius the Ghost God",
                "Lord of the Lost Lands", "Dr Terrible"
            },
            new string[]
            {
                "Archdemon Malphas", "Septavius the Ghost God", "Limon the Sprite God",
                "Thessal the Mermaid Goddess", "Crystal Prisoner", "Gigacorn"
            },
            new string[]
            {
                "Thessal the Mermaid Goddess", "Crystal Prisoner", "Tomb Support",
                "Tomb Defender", "Tomb Attacker", "Oryx the Mad God 2", "Grand Sphinx",
                "Queen of Hearts", "TestChicken 2"
            },
            new string[]
            {
                "Thessal the Mermaid Goddess", "Crystal Prisoner", "Tomb Support",
                "Tomb Defender", "Tomb Attacker", "Oryx the Mad God 2",
                "Phoenix Wright", "TestChicken 2"
            }
        };

        private readonly new Dictionary<int, string[]> _waveRewards = new Dictionary<int, string[]>
        {
            { 5, new string[] {"Spider Den Key", "Pirate Cave Key", "Forest Maze Key"} },
            { 10, new string[] {"Snake Pit Key", "Sprite World Key", "Undead Lair Key", "Abyss of Demons Key", "Lab Key", "Beachzone Key"} },
            { 20, new string[] {"Bella's Key", "Tomb of the Ancients Key", "Shaitan's Key", "The Crawling Depths Key", "Candy Key"} },
            { 25, new string[] {"Loot Drop Potion" } },
            { 30, new string[] {"Jeebs' Arena Key", "Shatters Key", "Asylum Key"}},
            { 50, new string[] {"Backpack"} }
        };

        private bool _solo;
        private CountDownState _countDown;

        private int _wave;
        private long _restTime;
        private long _time;
        private int _startingPlayers;

        private List<IntPoint> _outerSpawn;
        private List<IntPoint> _centralSpawn;

        public ArenaState CurrentState { get; private set; }

        public static Arena Instance { get; private set; }

        public Arena(ProtoWorld proto, Client client = null) : base(proto)
        {
            CurrentState = ArenaState.NotStarted;
            _wave = 1;
            Instance = this;
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
            return base.AllowedAccess(client) &&
                   ((_countDown != CountDownState.Done && _solo == false) || client.Account.Admin);
        }

        public override World GetInstance(Client client)
        {
            var manager = client.Manager;

            // join existing open arena if possible
            foreach (var world in manager.Worlds.Values)
            {
                if (!(world is Arena) || world.IsLimbo || (world as Arena)._solo)
                    continue;

                if (world.Players.Count > 0)
                    return world;

                world.Delete();
                break; // if empty, reset by making new one
            }

            var arena = new Arena(manager.Resources.Worlds[Name], client) {IsLimbo = false};
            Manager.Monitor.UpdateWorldInstance(World.Arena, arena);
            return Manager.AddWorld(arena);
        }

        private void SpawnEnemies()
        {
            var enemies = new List<string>();
            var r = new Random();

            for (int i = 0; i < Math.Ceiling((double) (_wave + Players.Count)/2); i++)
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

            if (IsLimbo || Deleted || TotalConnects < 1)
            {
                if (!Deleted || _solo || Manager == null) 
                    return;

                Manager.Monitor.OpenPortal(World.Arena);
                Manager.Monitor.UpdateWorldInstance(World.Arena, Manager.Worlds[World.Arena]);
                return;
            }

            _time += time.ElaspedMsDelta;

            switch (CurrentState)
            {
                case ArenaState.NotStarted:
                    CurrentState = ArenaState.CountDown;
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
                default:
                    CurrentState = ArenaState.Start;
                    break;
            }
        }

        private void Countdown(RealmTime time)
        {
            if (_countDown == CountDownState.Notify60)
            {
                _countDown = CountDownState.Notify30;

                Manager.Chat.Announce("A public arena game is starting. Closing in 1 min. Type /arena to join.", true);
                foreach (var plr in Players.Values)
                {
                    if(plr.Owner?.IsNotCombatMapArea ?? false)
                        plr.Client.SendPacket(new GlobalNotification
                        {
                            Type = GlobalNotification.ADD_ARENA,
                            Text = "{\"name\":\"Public Arena\",\"open\":true}"
                        });
                    plr.SendInfo("Game starting in 60 seconds.");
                }
            }

            if (_countDown == CountDownState.Notify30 && _time > 30000)
            {
                _countDown = CountDownState.StartGame;

                foreach (var plr in Players.Values)
                    plr.SendInfo("Game starting in 30 seconds.");
            }

            if (_countDown == CountDownState.StartGame && _time > 60000)
            {
                _countDown = CountDownState.Done;
                CurrentState = ArenaState.Start;
                _time = 0;
                _startingPlayers = Players.Count(p => p.Value.SpectateTarget == null);

                Manager.Monitor.ClosePortal(World.Arena);

                foreach (var plr in Players.Values.Where(_ => _.Owner?.IsNotCombatMapArea ?? false))
                    plr.Client.SendPacket(new GlobalNotification
                    {
                        Type = GlobalNotification.ADD_ARENA,
                        Text = "{\"name\":\"Public Arena\",\"open\":false}"
                    });
            }
        }

        private void Start(RealmTime time)
        {
            CurrentState = ArenaState.Rest;
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
                    CurrentRuntime = (int) _time,
                    Wave = _wave
                }, null);

                HandleWaveRewards();

                return;
            }

            if (_time - _restTime < 5000)
                return;

            CurrentState = ArenaState.Spawn;
        }

        private void Spawn(RealmTime time)
        {
            SpawnEnemies();
            SpawnBosses();
            CurrentState = ArenaState.Fight;
        }

        private void Fight(RealmTime time)
        {
            if (!_solo && Players.Count(p => !p.Value.Client.Account.Admin || p.Value.SpectateTarget != null) <= 1)
            {
                _solo = true;

                Manager.Monitor.OpenPortal(World.Arena);
                Manager.Monitor.UpdateWorldInstance(World.Arena, Manager.Worlds[World.Arena]);
                
                var plr = Players.FirstOrDefault(p => !p.Value.Client.Account.Admin).Value;
                if (plr != null && _startingPlayers > 1)
                {
                    Manager.Chat.Announce(
                        "Congrats to " + plr.Name +
                        " for being the sole survivor of the public arena. (Wave: " + _wave + ", Starting Players: " +
                        _startingPlayers + ")", true);

                    foreach (var client in Manager.Clients.Keys)
                        client.SendPacket(new GlobalNotification
                        {
                            Type = GlobalNotification.DELETE_ARENA,
                            Text = "Public Arena"
                        });
                }
            }

            if (!Enemies.Any(e => e.Value.ObjectDesc.Enemy))
            {
                _wave++;
                _restTime = _time;
                CurrentState = ArenaState.Rest;

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
            foreach (var player in Players.Values.Where(p => !p.HasConditionEffect(ConditionEffects.Hidden))) // no rewards for lurkers
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
                    player.SendError("[Public Arena] We were unable to give you a reward, your inventory is full :(");
                }
            }
        }

        public override void LeaveWorld(Entity entity)
        {
            base.LeaveWorld(entity);

            if (!(entity is Player)) return;
            if (Players.Count == 0)
                CurrentState = ArenaState.NotStarted;
        }
    }
}