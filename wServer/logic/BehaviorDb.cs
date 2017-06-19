using System;
using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.realm;
using wServer.realm.entities;
using wServer.logic.loot;
using System.Threading;
using System.Reflection;
using log4net;

namespace wServer.logic
{
    public partial class BehaviorDb
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(BehaviorDb));

        public RealmManager Manager { get; private set; }

        static int _initializing;
        internal static BehaviorDb InitDb;
        internal static XmlData InitGameData => InitDb.Manager.Resources.GameData;

        public BehaviorDb(RealmManager manager)
        {
            Log.Info("Initializing Behavior Database...");

            Manager = manager;
            MobDrops.Init(manager);

            Definitions = new Dictionary<ushort, Tuple<State, Loot>>();

            if (Interlocked.Exchange(ref _initializing, 1) == 1)
            {
                Log.Error("Attempted to initialize multiple BehaviorDb at the same time.");
                throw new InvalidOperationException("Attempted to initialize multiple BehaviorDb at the same time.");
            }
            InitDb = this;

            var fields = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.FieldType == typeof(_))
                .ToArray();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                Log.InfoFormat("Loading behavior for '{0}'({1}/{2})...", field.Name, i + 1, fields.Length);
                ((_)field.GetValue(this))();
                field.SetValue(this, null);
            }

            InitDb = null;
            _initializing = 0;

            Log.Info("Behavior Database initialized...");
        }

        public void ResolveBehavior(Entity entity)
        {
            Tuple<State, Loot> def;
            if (Definitions.TryGetValue(entity.ObjectType, out def))
                entity.SwitchTo(def.Item1);
        }

        delegate ctor _();
        struct ctor
        {
            public ctor Init(string id, State rootState, params MobDrops[] defs)
            {
                var d = new Dictionary<string, State>();
                rootState.Resolve(d);
                rootState.ResolveChildren(d);
                var dat = InitDb.Manager.Resources.GameData;

                if (!dat.IdToObjectType.ContainsKey(id))
                {
                    Log.Error($"Failed to add behavior: {id}. Xml data not found.");
                    return this;
                }

                if (defs.Length > 0)
                {
                    var loot = new Loot(defs);
                    rootState.Death += (sender, e) => loot.Handle((Enemy)e.Host, e.Time);
                    InitDb.Definitions.Add(dat.IdToObjectType[id], new Tuple<State, Loot>(rootState, loot));
                }
                else
                    InitDb.Definitions.Add(dat.IdToObjectType[id], new Tuple<State, Loot>(rootState, null));
                return this;
            }
        }
        static ctor Behav()
        {
            return new ctor();
        }

        public Dictionary<ushort, Tuple<State, Loot>> Definitions { get; private set; }

    }
}
