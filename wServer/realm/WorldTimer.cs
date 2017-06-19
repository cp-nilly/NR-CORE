using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class WorldTimer
    {
        private readonly Action<World, RealmTime> _cb;
        private readonly Func<World, RealmTime, bool> _rcb; 
        private readonly int _total;
        private int _remain;

        public WorldTimer(int tickMs, Action<World, RealmTime> callback)
        {
            _remain = _total = tickMs;
            _cb = callback;
        }

        public WorldTimer(int tickMs, Func<World, RealmTime, bool> callback)
        {
            _remain = _total = tickMs;
            _rcb = callback;
        }

        public void Reset()
        {
            _remain = _total;
        }

        public bool Tick(World world, RealmTime time)
        {
            _remain -= time.ElaspedMsDelta;
            
            if (_remain >= 0) 
                return false;

            if (_cb != null)
            {
                _cb(world, time);
                return true;
            }

            return _rcb(world, time);
        }
    }
}
