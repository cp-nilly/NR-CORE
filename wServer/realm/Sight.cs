using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm
{
    public class Sight
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Sight));

        private const int Radius = Player.Radius;
        private const int RadiusSqr = Player.RadiusSqr;
        private const int AppoxAreaOfSight = (int)(Math.PI * Radius * Radius + 1);
        private const int MaxNumRegions = 2048;

        // blocked line of sight vars
        private const float StartAngle = 0;
        private const float EndAngle = (float)(2 * Math.PI);
        private const float RayStepSize = .1f;
        private const float AngleStepSize = 2.30f / Radius;

        private static readonly IntPoint[] SurroundingPoints = new IntPoint[]
        {
            new IntPoint(1, 0),
            new IntPoint(1, 1),
            new IntPoint(0, 1),
            new IntPoint(-1, 1),
            new IntPoint(-1, 0),
            new IntPoint(-1, -1),
            new IntPoint(0, -1),
            new IntPoint(1, -1)
        };

        private readonly Player _player;
        
        private IntPoint _ip;
        private readonly HashSet<IntPoint> _sCircle; 
        
        public int LastX { get; private set; }
        public int LastY { get; private set; }
        public int UpdateCount { get; set; }

        private static List<IntPoint> _unblockedView;
        private static Dictionary<float, IntPoint[]> _sightRays;
        private static readonly List<int> Primes; 
        private static readonly List<IntPoint> VisibleTilesList = new List<IntPoint>(1024 * 1024);
        private static readonly HashSet<IntPoint> VisibleTilesSet = new HashSet<IntPoint>();
        
        private IntPoint _sTile;
        private readonly List<IntPoint> _visibleTilesList;
        
        static Sight()
        {
            InitUnblockedView();
            InitSightRays();

            Primes = MathsUtils.GeneratePrimes(MaxNumRegions);
        }

        private static void InitUnblockedView()
        {
            var x = 0;
            var y = 0;
            var i = 1;
            var j = 2;

            _unblockedView = new List<IntPoint> { new IntPoint(0, 0) };
            do
            {
                UnblockedViewTryAdd(x = x + 1, y);
                for (var k = 0; k < i; k++)
                    UnblockedViewTryAdd(x, y = y - 1);
                for (var k = 0; k < j; k++)
                    UnblockedViewTryAdd(x = x - 1, y);
                for (var k = 0; k < j; k++)
                    UnblockedViewTryAdd(x, y = y + 1);
                for (var k = 0; k < j; k++)
                    UnblockedViewTryAdd(x = x + 1, y);

                i += 2;
                j += 2;
            } while (j <= 2 * Radius);
        }

        private static void UnblockedViewTryAdd(int x, int y)
        {
            if (x * x + y * y <= RadiusSqr)
                _unblockedView.Add(new IntPoint(x, y));
        }

        private static void InitSightRays()
        {
            _sightRays = new Dictionary<float, IntPoint[]>();

            var currentAngle = StartAngle;
            while (currentAngle < EndAngle)
            {
                var ray = new List<IntPoint>(Radius);
                var dist = RayStepSize;
                while (dist < Radius)
                {
                    var point = new IntPoint(
                        (int)(dist * Math.Cos(currentAngle)),
                        (int)(dist * Math.Sin(currentAngle)));

                    if (!ray.Contains(point))
                        ray.Add(point);

                    dist += RayStepSize;
                }
                _sightRays[currentAngle] = ray.ToArray();

                currentAngle += AngleStepSize;
            }
        }

        public Sight(Player player)
        {
            _player = player;
            _ip = new IntPoint(0, 0);
            _sCircle = new HashSet<IntPoint>();
            _visibleTilesList = new List<IntPoint>(AppoxAreaOfSight);
            UpdateCount++;
        }
        
        public HashSet<IntPoint> GetSightCircle(int blocked = 0)
        {
            //var time = Stopwatch.StartNew();
            var specPlayer = _player.SpectateTarget as Player;
            if (specPlayer != null)
                return specPlayer.Sight._sCircle;

            if (UpdateCount <= 0)
                return _sCircle;

            UpdateCount = 0;
            LastX = (int)_player.X;
            LastY = (int)_player.Y;

            if (_player.Owner == null)
            {
                _sCircle.Clear();
                return _sCircle;
            }

            var map = _player.Owner.Map;
            //for (var i = 0; i < 150; i++)
            //{
            switch (blocked)
            {
                case 1:
                    CalcBlockedRoomSight(map);
                    break;
                case 2:
                    CalcBlockedLineOfSight(map);
                    break;
                case 3:
                    CalcRegionBlockSight(map);
                    break;
                default:
                    CalcUnblockedSight(map);
                    break;
            }
            //}
            //Log.InfoFormat(time.ElapsedMilliseconds.ToString());
            return _sCircle;
        }

        private void CalcUnblockedSight(Wmap map)
        {
            _sCircle.Clear();
            foreach (var p in _unblockedView)
            {
                _ip.X = LastX + p.X;
                _ip.Y = LastY + p.Y;

                if (!map.Contains(_ip))
                    continue;

                _sCircle.Add(_ip);
            }
        }

        private void CalcRegionBlockSight(Wmap map)
        {
            var sRegion = map[LastX, LastY].SightRegion;

            _sCircle.Clear();
            foreach (var p in _unblockedView)
            {
                _ip.X = LastX + p.X;
                _ip.Y = LastY + p.Y;

                if (!map.Contains(_ip))
                    continue;

                var t = map[_ip.X, _ip.Y];
                if (t.SightRegion % sRegion == 0)
                    _sCircle.Add(_ip);
            }
        }

        private void CalcBlockedRoomSight(Wmap map)
        {
            var height = map.Height;
            var width = map.Width;

            _sCircle.Clear();
            _visibleTilesList.Clear();

            _sTile = new IntPoint(LastX, LastY);
            _visibleTilesList.Add(_sTile);

            for (var i = 0; i < _visibleTilesList.Count; i++)
            {
                var tile = _visibleTilesList[i];

                if (tile.Generation > Radius)
                    continue;

                foreach (var sPoint in SurroundingPoints)
                {
                    var x = (_sTile.X = tile.X + sPoint.X);
                    var y = (_sTile.Y = tile.Y + sPoint.Y);

                    var dx = LastX - x;
                    var dy = LastY - y;

                    if (_sCircle.Contains(_sTile) ||
                        x < 0 || x >= width ||
                        y < 0 || y >= height ||
                        dx * dx + dy * dy > RadiusSqr)
                        continue;

                    _sTile.Generation = tile.Generation + 1;
                    _sCircle.Add(_sTile);

                    var t = map[x, y];
                    if (IsBlocking(t))
                        continue;

                    _visibleTilesList.Add(_sTile);
                }
            }
        }

        private void CalcBlockedLineOfSight(Wmap map)
        {
            _sCircle.Clear();
            foreach (var ray in _sightRays)
                for (var i = 0; i < ray.Value.Length; i++)
                {
                    _ip.X = LastX + ray.Value[i].X;
                    _ip.Y = LastY + ray.Value[i].Y;

                    if (!map.Contains(_ip))
                        continue;

                    _sCircle.Add(_ip);

                    var t = map[_ip.X, _ip.Y];
                    if (t.ObjType != 0 && t.ObjDesc != null && t.ObjDesc.BlocksSight)
                        break;
                    foreach (var sPoint in SurroundingPoints)
                    {
                        IntPoint _intPoint = new IntPoint(sPoint.X + _ip.X, sPoint.Y + _ip.Y);
                        if (map.Contains(_intPoint))
                            _sCircle.Add(_intPoint);
                    }
                }
        }

        public static void CalcRegionBlocks(Wmap map)
        {
            var i = 0;
            for (var x = 0; x < map.Width; x++)
                for (var y = 0; y < map.Height; y++)
                {
                    var t = map[x, y];
                    if (t.SightRegion != 1 || IsBlocking(t))
                        continue;

                    CalcRegion(map, i++, x, y);
                }
        }

        private static void CalcRegion(Wmap map, int pIndex, int sx, int sy)
        {
            VisibleTilesList.Clear();
            VisibleTilesSet.Clear();

            var p = new IntPoint(sx, sy);
            var prime = Primes[pIndex];
            map[p.X, p.Y].SightRegion = prime;

            VisibleTilesList.Add(p);

            for (var i = 0; i < VisibleTilesList.Count; i++)
            {
                var op = VisibleTilesList[i];
                foreach (var sp in SurroundingPoints)
                {
                    p.X = op.X + sp.X;
                    p.Y = op.Y + sp.Y;

                    if (!map.Contains(p) || VisibleTilesSet.Contains(p))
                        continue;
                    
                    VisibleTilesSet.Add(p);

                    var t = map[p.X, p.Y];

                    if (IsBlocking(t))
                    {
                        checked { t.SightRegion *= prime; }
                        continue;
                    }
                    
                    t.SightRegion = prime;
                    VisibleTilesList.Add(p);
                }
            }
        }
        
        public static void UpdateRegion(Wmap map, int ox, int oy)
        {
            var op = new IntPoint(ox, oy);
            var p = new IntPoint();
            var connectRegions = new List<long>(4);
            
            // get non blocked sight regions
            foreach (var sp in SurroundingPoints)
            {
                p.X = op.X + sp.X;
                p.Y = op.Y + sp.Y;

                var t = map[p.X, p.Y];
                if (!IsBlocking(t))
                    connectRegions.Add(t.SightRegion);
            }

            // shouldn't happen but just in case...
            if (connectRegions.Count == 0)
            {
                map[op.X, op.Y].SightRegion = 1;
                return;
            }

            // pick new binding region
            var nr = connectRegions.Min();
            map[op.X, op.Y].SightRegion = nr;

            // make uninitialized blocked sight regions visible
            foreach (var sp in SurroundingPoints)
            {
                p.X = op.X + sp.X;
                p.Y = op.Y + sp.Y;

                var t = map[p.X, p.Y];
                if (IsBlocking(t))
                {
                    foreach (var r in connectRegions)
                        if (t.SightRegion % r == 0)
                            t.SightRegion /= r;
                    checked { t.SightRegion *= nr; }
                }
            }

            // connect sight regions
            for (var i = 0; i < connectRegions.Count; i++)
            {
                var wr = connectRegions[i];
                if (wr == nr)
                    continue;

                for (var x = 0; x < map.Width; x++)
                    for (var y = 0; y < map.Height; y++)
                    {
                        var t = map[x, y];
                        if (t.SightRegion % wr != 0)
                            continue;

                        t.SightRegion /= wr;
                        checked { t.SightRegion *= nr; }
                    }
            }
        }

        private static bool IsBlocking(WmapTile tile)
        {
            return tile.ObjType != 0 &&
                   tile.ObjDesc != null &&
                   tile.ObjDesc.BlocksSight;
        }
    }
}