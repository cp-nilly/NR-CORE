using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace wServer.realm
{
    public class FLLogicTicker
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FLLogicTicker));

        private readonly RealmManager _manager;
        private readonly ConcurrentQueue<Action<RealmTime>>[] _pendings;
        
        public readonly int TPS;
        public readonly int MsPT;

        private readonly ManualResetEvent _mre;
        private Task _worldTask;
        private RealmTime _worldTime;

        public FLLogicTicker(RealmManager manager)
        {
            _manager = manager;
            MsPT = 1000 / manager.TPS;
            _mre = new ManualResetEvent(false);
            _worldTime = new RealmTime();

            _pendings = new ConcurrentQueue<Action<RealmTime>>[5];
            for (int i = 0; i < 5; i++)
                _pendings[i] = new ConcurrentQueue<Action<RealmTime>>();
        }

        public void TickLoop()
        {
            Log.Info("Logic loop started.");

            var loopTime = 0;
            var t = new RealmTime();
            var watch = Stopwatch.StartNew();
            do
            {
                t.TotalElapsedMs = watch.ElapsedMilliseconds;
                t.TickDelta = loopTime / MsPT;
                t.TickCount += t.TickDelta;
                t.ElaspedMsDelta = t.TickDelta * MsPT;

                if (t.TickDelta > 3)
                    Log.Warn("LAGGED! | ticks:" + t.TickDelta +
                                      " ms: " + loopTime +
                                      " tps: " + t.TickCount / (t.TotalElapsedMs / 1000.0));

                if (_manager.Terminating)
                    break;

                DoLogic(t);

                var logicTime = (int)(watch.ElapsedMilliseconds - t.TotalElapsedMs);
                _mre.WaitOne(Math.Max(0, MsPT - logicTime));
                loopTime += (int)(watch.ElapsedMilliseconds - t.TotalElapsedMs) - t.ElaspedMsDelta;
            } while (true);
            Log.Info("Logic loop stopped.");
        }

        private void DoLogic(RealmTime t)
        {
            var clients = _manager.Clients.Keys;
            
            foreach (var i in _pendings)
            {
                Action<RealmTime> callback;
                while (i.TryDequeue(out callback))
                    try
                    {
                        callback(t);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
            }

            _manager.ConMan.Tick(t);
            _manager.Monitor.Tick(t);
            _manager.InterServer.Tick(t.ElaspedMsDelta);

            TickWorlds1(t);

            foreach (var client in clients)
                if (client.Player != null && client.Player.Owner != null)
                    client.Player.Flush();
        }

        void TickWorlds1(RealmTime t)    //Continous simulation
        {
            _worldTime.TickDelta += t.TickDelta;
            
            // tick essentials
            try
            {
                foreach (var w in _manager.Worlds.Values.Distinct())
                    w.TickLogic(t);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            // tick world every 200 ms
            if (_worldTask == null || _worldTask.IsCompleted)
            {
                t.TickDelta = _worldTime.TickDelta;
                t.ElaspedMsDelta = t.TickDelta * MsPT;

                if (t.ElaspedMsDelta < 200) 
                    return;

                _worldTime.TickDelta = 0;
                _worldTask = Task.Factory.StartNew(() =>
                {
                    foreach (var i in _manager.Worlds.Values.Distinct())
                        i.Tick(t);
                }).ContinueWith(e =>
                    Log.Error(e.Exception.InnerException.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public void AddPendingAction(Action<RealmTime> callback,
            PendingPriority priority = PendingPriority.Normal)
        {
            _pendings[(int)priority].Enqueue(callback);
        }
    }
}
