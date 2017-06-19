using System.Timers;
using common;

namespace server
{
    public class LegendSweeper
    {
        private readonly Timer _tmr = new Timer(60000);
        private readonly Database _db;

        public LegendSweeper(Database db)
        {
            _db = db;
        }

        public void Run()
        {
            _tmr.Elapsed += (sender, e) => _db.CleanLegends();
            _tmr.Start();
        }
    }
}

