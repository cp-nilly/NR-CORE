using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using log4net;

namespace wServer.realm
{
    public class LogicTicker
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogicTicker));

        private readonly RealmManager _manager;
        private readonly ConcurrentQueue<Action<RealmTime>>[] _pendings;
        public readonly int TPS;
        public readonly int MsPT;

        public LogicTicker(RealmManager manager)
        {
            _manager = manager;

            _pendings = new ConcurrentQueue<Action<RealmTime>>[5];
            for (int i = 0; i < 5; i++)
                _pendings[i] = new ConcurrentQueue<Action<RealmTime>>();

            TPS = manager.TPS;
            MsPT = 1000 / TPS;
        }

        public void AddPendingAction(Action<RealmTime> callback)
        {
            AddPendingAction(callback, PendingPriority.Normal);
        }

        public void AddPendingAction(Action<RealmTime> callback, PendingPriority priority)
        {
            _pendings[(int)priority].Enqueue(callback);
        }
        
        public void TickLoop()
        {
            Log.Info("Logic loop started.");
            var watch = new Stopwatch();
            long dt = 0;
            long count = 0;

            watch.Start();
            var t = new RealmTime();
            do
            {
                if (_manager.Terminating) break;

                long times = dt / MsPT;
                dt -= times * MsPT;
                times++;

                long b = watch.ElapsedMilliseconds;

                count += times;
                if (times > 3)
                    Log.Warn("LAGGED!| time:" + times + " dt:" + dt + " count:" + count + " time:" + b + " tps:" + count / (b / 1000.0));

                t.TotalElapsedMs = b;
                t.TickCount = count;
                t.TickDelta = (int)times;
                t.ElaspedMsDelta = (int)(times * MsPT);

                long c = watch.ElapsedMilliseconds;

                foreach (var client in _manager.Clients.Keys)
                {
                    if (client.Player != null &&
                        client.Player.Owner != null)
                    {
                        client.Player.Flush();
                    }

                    //client.SendTick();
                }

                foreach (var i in _pendings)
                {
                    Action<RealmTime> callback;
                    while (i.TryDequeue(out callback))
                    {
                        try
                        {
                            callback(t);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                long dc = watch.ElapsedMilliseconds - c;
                
                TickWorlds1(t);
                _manager.InterServer.Tick(t.ElaspedMsDelta);

                Thread.Sleep(Math.Max(0, MsPT - (int)dc));
                dt += Math.Max(0, watch.ElapsedMilliseconds - b - MsPT);
            } while (true);
            Log.Info("Logic loop stopped.");
        }

        void TickWorlds1(RealmTime t)    //Continous simulation
        {
            foreach (var i in _manager.Worlds.Values.Distinct())
                i.Tick(t);
        }

        void TickWorlds2(RealmTime t)    //Discrete simulation
        {
            long counter = t.ElaspedMsDelta;
            long c = t.TickCount - t.TickDelta;
            long x = t.TotalElapsedMs - t.ElaspedMsDelta;
            while (counter >= MsPT)
            {
                c++; x += MsPT;
                TickWorlds1(new RealmTime()
                {
                    TickDelta = 1,
                    ElaspedMsDelta = MsPT,
                    TickCount = c,
                    TotalElapsedMs = x
                });
                counter -= MsPT;
            }
        }
    }
}
