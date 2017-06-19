using System.IO;
using System.Linq;
using System.Threading.Tasks;
using common.resources;
using log4net;
using wServer.networking;
using wServer.realm.entities;
using wServer.realm.setpieces;

namespace wServer.realm.worlds.logic
{
    public class Realm : World
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Realm));

        private Oryx _overseer;

        private readonly bool _oryxPresent;
        private readonly int _mapId;
        private Task _overseerTask;

        public Realm(ProtoWorld proto, Client client = null) : base(proto)
        {
            _oryxPresent = true;
            _mapId = 1;
        }

        public override bool AllowedAccess(Client client)
        {
            // since map gets reset, admins not allowed to join when closed. Possible to crash server otherwise.
            return !Closed && base.AllowedAccess(client);
        }

        protected override void Init()
        {
            Log.InfoFormat("Initializing Game World {0}({1}) from map {2}...", Id, Name, _mapId);

            FromWorldMap(new MemoryStream(Manager.Resources.Worlds["Realm"].wmap[_mapId - 1]));
            SetPieces.ApplySetPieces(this);
            
            if (_oryxPresent)
            {
                _overseer = new Oryx(this);
                _overseer.Init();
            }

            Log.Info("Game World initalized.");
        }

        public override void Tick(RealmTime time)
        {
            if (Closed)
                Manager.Monitor.ClosePortal(Id);
            else
                Manager.Monitor.OpenPortal(Id);

            base.Tick(time);

            if (IsLimbo || Deleted) 
                return;

            if (_overseerTask == null || _overseerTask.IsCompleted)
            {
                _overseerTask = Task.Factory.StartNew(() =>
                {
                    var secondsElapsed = time.TotalElapsedMs/1000;
                    if (secondsElapsed > 10 && secondsElapsed%1800 < 10 && !IsClosing())
                        CloseRealm();

                    if (Closed && Players.Count == 0 && _overseer != null)
                    {
                        Init(); // will reset everything back to the way it was when made
                        Closed = false;
                    }

                    _overseer?.Tick(time);
                }).ContinueWith(e =>
                    Log.Error(e.Exception.InnerException.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public void EnemyKilled(Enemy enemy, Player killer)
        {
            if (_overseer != null && !enemy.Spawned)
                _overseer.OnEnemyKilled(enemy, killer);
        }

        public override int EnterWorld(Entity entity)
        {
            var ret = base.EnterWorld(entity);
            var player = entity as Player;
            if (player != null)
                _overseer?.OnPlayerEntered(player);
            return ret;
        }

        public bool CloseRealm()
        {
            if (_overseer == null)
                return false;

            _overseer.InitCloseRealm();
            return true;
        }

        public bool IsClosing()
        {
            if (_overseer == null)
                return false;

            return _overseer.Closing;
        }
    }
}