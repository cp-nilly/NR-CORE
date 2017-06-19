using System;
using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.networking;
using wServer.networking.packets.outgoing;
using wServer.networking.packets.outgoing.arena;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm.worlds.logic
{
    class ArenaSolo : World
    {
        private enum ArenaState
        {
            CountDown,
            Start,
            Rest,
            Spawn,
            Fight
        }

        private enum CountDownState
        {
            Notify15,
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
        private readonly int[] _changeBossLevel = new int[] { 0, 6, 11, 16, 21, 36 };
        private readonly string[][] _randomBosses = new string[][]
        {
            new string[] { "Red Demon", "Phoenix Lord", "Henchman of Oryx", "Oryx Brute" }, 
            new string[] { "Red Demon", "Phoenix Lord", "Henchman of Oryx", "Stheno the Snake Queen" }, 
            new string[] { "Stheno the Snake Queen", "Archdemon Malphas", "Septavius the Ghost God", 
                           "Lord of the Lost Lands" }, 
            new string[] { "Archdemon Malphas", "Septavius the Ghost God", "Limon the Sprite God",
                           "Thessal the Mermaid Goddess", "Crystal Prisoner", "Gigacorn" }, 
            new string[] { "Thessal the Mermaid Goddess", "Crystal Prisoner", "Tomb Support", 
                           "Tomb Defender", "Tomb Attacker", "Oryx the Mad God 2", "Grand Sphinx" }, 
            new string[] { "Thessal the Mermaid Goddess", "Crystal Prisoner", "Tomb Support", 
                           "Tomb Defender", "Tomb Attacker", "Oryx the Mad God 2", 
                           "Phoenix Wright" }, 
            //new string[] { "Red Demon", "Phoenix Lord", "Henchman of Oryx", "Wishing Troll", "Jackal Lord" }
        };

        private ArenaState _arenaState;
        private CountDownState _countDown;

        private int _wave;
        private long _restTime;
        private long _time;

        private List<IntPoint> _outerSpawn;
        private List<IntPoint> _centralSpawn;

        public ArenaSolo(ProtoWorld proto, Client client = null)
            : base(proto)
        {
            _arenaState = ArenaState.CountDown;
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
            Name = "Arena"; // needed for client gui elements

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
            return base.AllowedAccess(client) && (TotalConnects < 1 || client.Account.Admin);
        }

        private void SpawnEnemies()
        {
            var enemies = new List<string>();
            var r = new Random();

            for (int i = 0; i < Math.Ceiling((double)(_wave + Players.Count) / 2); i++)
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
                return;
            
            _time += time.ElaspedMsDelta;

            switch (_arenaState)
            {
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
                    _arenaState = ArenaState.Start;
                    break;
            }
        }

        private void Countdown(RealmTime time)
        {
            if (_countDown == CountDownState.Notify15)
            {
                _countDown = CountDownState.StartGame;
                foreach (var plr in Players.Values)
                    plr.SendInfo("Game starting in 15 seconds.");
            }

            if (_countDown == CountDownState.StartGame && _time > 15000)
            {
                _countDown = CountDownState.Done;
                _arenaState = ArenaState.Start;
                _time = 0;
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
            if (!Enemies.Any(e => e.Value.ObjectDesc.Enemy))
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
    }
}
