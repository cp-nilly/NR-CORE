using System;
using System.Threading;
using common.resources;
using common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using wServer.networking;
using System.Threading.Tasks;
using System.Linq;
using wServer.logic;
using log4net;
using wServer.realm.commands;
using wServer.realm.entities.vendors;
using wServer.realm.worlds;
using wServer.realm.worlds.logic;

namespace wServer.realm
{
    public struct RealmTime
    {
        public long TickCount;
        public long TotalElapsedMs;
        public int TickDelta;
        public int ElaspedMsDelta;
    }

    public enum PendingPriority
    {
        Emergent,
        Destruction,
        Normal,
        Creation,
    }

    public enum PacketPriority
    {
        High,
        Normal,
        Low // no guarantees that packets of low priority will be sent
    }

    public class RealmManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealmManager));

        private readonly bool _initialized;
        public string InstanceId { get; private set; }
        public bool Terminating { get; private set; }

        public Resources Resources { get; private set; }
        public Database Database { get; private set; }
        public ServerConfig Config { get; private set; }
        public int TPS { get; private set; }
        
        public ConnectManager ConMan { get; private set; }
        public BehaviorDb Behaviors { get; private set; }
        public ISManager InterServer { get; private set; }
        public ISControl ISControl { get; private set; }
        public ChatManager Chat { get; private set; }
        public DbServerManager DbServerController { get; private set; }
        public CommandManager Commands { get; private set; }
        public Market Market { get; private set; }
        public DbTinker Tinker { get; private set; }
        public PortalMonitor Monitor { get; private set; }
        public DbEvents DbEvents { get; private set; }

        private Thread _network;
        private Thread _logic;
        public NetworkTicker Network { get; private set; }
        public FLLogicTicker Logic { get; private set; }

        public readonly ConcurrentDictionary<int, World> Worlds = new ConcurrentDictionary<int, World>();
        public readonly ConcurrentDictionary<Client, PlayerInfo> Clients = new ConcurrentDictionary<Client, PlayerInfo>();

        private int _nextWorldId = 0;
        private int _nextClientId = 0;

        public RealmManager(Resources resources, Database db, ServerConfig config)
        {
            Log.Info("Initializing Realm Manager...");

            InstanceId = Guid.NewGuid().ToString();
            Database = db;
            Resources = resources;
            Config = config;
            Config.serverInfo.instanceId = InstanceId;
            TPS = config.serverSettings.tps;

            // all these deal with db pub/sub... probably should put more thought into their structure... 
            InterServer = new ISManager(Database, config);
            ISControl = new ISControl(this);
            Chat = new ChatManager(this);
            DbServerController = new DbServerManager(this); // probably could integrate this with ChatManager and rename...
            DbEvents = new DbEvents(this);

            // basic server necessities
            ConMan = new ConnectManager(this, 
                config.serverSettings.maxPlayers,
                config.serverSettings.maxPlayersWithPriority);
            Behaviors = new BehaviorDb(this);
            Commands = new CommandManager(this);
            
            // some necessities that shouldn't be (will work this out later)
            MerchantLists.Init(this);
            Tinker = new DbTinker(db.Conn);
            if (Config.serverSettings.enableMarket)
                Market = new Market(this);

            var serverMode = config.serverSettings.mode;
            switch (serverMode)
            {
                case ServerMode.Single:
                    InitializeNexusHub();
                    AddWorld("Realm");
                    break;
                case ServerMode.Nexus:
                    InitializeNexusHub();
                    break;
                case ServerMode.Realm:
                    AddWorld("Realm");
                    break;
                case ServerMode.Marketplace:
                    AddWorld("Marketplace", true);
                    AddWorld("Vault");
                    AddWorld("ClothBazaar");
                    break;
            }
            
            // add portal monitor to nexus and initialize worlds
            if (Worlds.ContainsKey(World.Nexus))
                Monitor = new PortalMonitor(this, Worlds[World.Nexus]);
            foreach (var world in Worlds.Values)
                OnWorldAdded(world);

            _initialized = true;

            Log.Info("Realm Manager initialized.");
        }

        private void InitializeNexusHub()
        {
            // load world data
            foreach (var wData in Resources.Worlds.Data.Values)
                if (wData.id < 0)
                    AddWorld(wData);
        }
        
        public void Run()
        {
            Log.Info("Starting Realm Manager...");

            // start server logic management
            Logic = new FLLogicTicker(this);
            var logic = new Task(() => Logic.TickLoop(), TaskCreationOptions.LongRunning);
            logic.ContinueWith(Program.Stop, TaskContinuationOptions.OnlyOnFaulted);
            logic.Start();

            // start received packet processor
            Network = new NetworkTicker(this);
            var network = new Task(() => Network.TickLoop(), TaskCreationOptions.LongRunning);
            network.ContinueWith(Program.Stop, TaskContinuationOptions.OnlyOnFaulted);
            network.Start();

            Log.Info("Realm Manager started.");
        }

        public void Stop()
        {
            Log.Info("Stopping Realm Manager...");

            Terminating = true;
            InterServer.Dispose();
            Resources.Dispose();
            Network.Shutdown();

            Log.Info("Realm Manager stopped.");
        }

        public bool TryConnect(Client client)
        {
            if (client?.Account == null)
                return false;

            client.Id = Interlocked.Increment(ref _nextClientId);
            var plrInfo = new PlayerInfo()
            {
                AccountId = client.Account.AccountId,
                GuildId = client.Account.GuildId,
                Name = client.Account.Name,
                WorldInstance = -1
            };
            Clients[client] = plrInfo;

            // recalculate usage statistics
            Config.serverInfo.players = ConMan.GetPlayerCount();
            Config.serverInfo.maxPlayers = Config.serverSettings.maxPlayers;
            Config.serverInfo.queueLength = ConMan.QueueLength();
            Config.serverInfo.playerList.Add(plrInfo);
            return true;
        }

        public void Disconnect(Client client)
        {
            var player = client.Player;
            player?.Owner?.LeaveWorld(player);
            
            PlayerInfo plrInfo;
            Clients.TryRemove(client, out plrInfo);

            // recalculate usage statistics
            Config.serverInfo.players = ConMan.GetPlayerCount();
            Config.serverInfo.maxPlayers = Config.serverSettings.maxPlayers;
            Config.serverInfo.queueLength = ConMan.QueueLength();
            Config.serverInfo.playerList.Remove(plrInfo);
        }

        private void AddWorld(string name, bool actAsNexus = false)
        {
            AddWorld(Resources.Worlds.Data[name], actAsNexus);
        }

        private void AddWorld(ProtoWorld proto, bool actAsNexus = false)
        {
            int id;
            if (actAsNexus)
            {
                id = World.Nexus;
            }
            else
            {
                id = (proto.id < 0)
                    ? proto.id
                    : Interlocked.Increment(ref _nextWorldId);
            }

            World world;
            DynamicWorld.TryGetWorld(proto, null, out world);
            if (world != null)
            {
                if (world is Marketplace && !Config.serverSettings.enableMarket)
                    return;

                AddWorld(id, world);
                return;
            }

            AddWorld(id, new World(proto));
        }

        private void AddWorld(int id, World world)
        {
            if (world.Manager != null)
                throw new InvalidOperationException("World already added.");
            world.Id = id;
            Worlds[id] = world;
            if (_initialized)
                OnWorldAdded(world);
        }

        public World AddWorld(World world)
        {
            if (world.Manager != null)
                throw new InvalidOperationException("World already added.");
            world.Id = Interlocked.Increment(ref _nextWorldId);
            Worlds[world.Id] = world;
            if (_initialized)
                OnWorldAdded(world);
            return world;
        }

        public World GetWorld(int id)
        {
            World ret;
            if (!Worlds.TryGetValue(id, out ret)) return null;
            if (ret.Id == 0) return null;
            return ret;
        }

        public bool RemoveWorld(World world)
        {
            if (world.Manager == null)
                throw new InvalidOperationException("World is not added.");
            if (Worlds.TryRemove(world.Id, out world))
            {
                OnWorldRemoved(world);
                return true;
            }
            else
                return false;
        }

        void OnWorldAdded(World world)
        {
            world.Manager = this;
            Log.InfoFormat("World {0}({1}) added. {2} Worlds existing.", world.Id, world.Name, Worlds.Count);
        }

        void OnWorldRemoved(World world)
        {
            //world.Manager = null;
            Monitor.RemovePortal(world.Id);
            Log.InfoFormat("World {0}({1}) removed.", world.Id, world.Name);
        }

        public World GetRandomGameWorld()
        {
            var realms = Worlds.Values
                .OfType<Realm>()
                .Where(w => !w.Closed)
                .ToArray();

            return realms.Length == 0 ?
                Worlds[World.Nexus] :
                realms[Environment.TickCount % realms.Length];
        }
    }
}
