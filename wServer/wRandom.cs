using System;

namespace wServer
{
    public class wRandom : Random
    {
        private uint _seed;

        public wRandom() : this((uint)Environment.TickCount) { }
        
        public wRandom(uint seed)
        {
            this._seed = seed;
        }

        public uint CurrentSeed { get { return _seed; } set { _seed = value; } }

        public override int Next(int min, int max)
        {
            return (int)(min == max ? min : (min + (SampleNext() % (max - min))));
        }

        private uint SampleNext()
        {
            uint lb = (16807 * (_seed & 0xFFFF));
            uint hb = (16807 * (_seed >> 16));
            lb = lb + ((hb & 32767) << 16);
            lb = lb + (hb >> 15);
            if (lb > 2147483647)
            {
                lb = (lb - 2147483647);
            }
            return _seed = lb;
        }
    }
}
