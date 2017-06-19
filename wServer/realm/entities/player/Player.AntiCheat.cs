using System;
using common.resources;
using log4net;

namespace wServer.realm.entities
{
    partial class Player
    {
        private static readonly ILog CheatLog = LogManager.GetLogger("CheatLog");

        private const double Tolerance = 0.75;
        private readonly int[] _lastShotTime = new int[2];
        private readonly long[] _lastShotServerTime = new long[2];
        private readonly int[] _shotsLeft = new int[2];
        private int _infractions = 0;

        public int? ValidatePlayerShootPacket(Item item, int time, RealmTime realmTime)
        {
            int slot;
            var nextShot = CalcNextShotTime(item, out slot);

            ObjectDesc desc;
            Manager.Resources.GameData.ObjectDescs.TryGetValue(ObjectType, out desc);
            var playerType = (desc != null) ? desc.ObjectId : "";

            if (slot == -1)
            {
                //Log.InfoFormat("[Cheating?] ({0}, {1}, {2}) - Weapon not equipped.",
                //    Name, playerType, item.ObjectId);
                return _infractions; // weapon not equipped (can be caused by weapon switching so don't increase infraction count)
            }
            

            if (time >= nextShot)
            {


                long cDelta = time - _lastShotTime[slot];
                long timeAccordingClient = _lastShotServerTime[slot] + cDelta;

                _lastShotTime[slot] = time;
                _lastShotServerTime[slot] = realmTime.TotalElapsedMs;

                int shootDelta = (int) (GetShootDelta(item) * 0.6);

                if (timeAccordingClient >= realmTime.TotalElapsedMs + shootDelta)
                {
                    //CheatLog.Info($"{Name} is shooting {timeAccordingClient - realmTime.TotalElapsedMs}ms into the future.");
                    return _infractions;
                }

                _infractions = 0;
                _shotsLeft[slot] = 1;
                return null;
            }

            if (time == _lastShotTime[slot])
            {
                _shotsLeft[slot]++;
                if (_shotsLeft[slot] <= item.NumProjectiles)
                {
                    _infractions = 0;
                    return null;
                }
            }

            //CheatLog.Info($"{Name}, {playerType}, {item.ObjectId}) -Firing faster than player should. ({time - _lastShotTime[slot]}elapsed, should be > {nextShot - _lastShotTime[slot]}) ");
            return _infractions++;
        }

        private int GetShootDelta(Item item)
        {
            var shootDelta = 500.0;

            // ability shoot items (like quivers)
            if (item == Inventory[1])
            {
                if (Math.Abs(item.Cooldown) > 0)
                    shootDelta = (int)(item.Cooldown * 1000);
            }

            // main weapon
            if (item == Inventory[0])
            {
                shootDelta = 1 / DexRateOfFire() * 1 / item.RateOfFire;
            }

            return (int)shootDelta;
        }

        private int CalcNextShotTime(Item item, out int slot)
        {
            var shootDelta = 500.0;
            var nextShot = 0;
            slot = -1;

            // ability shoot items (like quivers)
            if (item == Inventory[1])
            {
                if (Math.Abs(item.Cooldown) > 0)
                    shootDelta = (int) (item.Cooldown * 1000);
                nextShot = (int) (_lastShotTime[1] + shootDelta * Tolerance);
                slot = 1;
            }

            // main weapon
            if (item == Inventory[0])
            {
                shootDelta = 1 / DexRateOfFire() * 1 / item.RateOfFire;
                nextShot = (int) (_lastShotTime[0] + shootDelta * Tolerance);
                slot = 0;
            }

            return nextShot;
        }

        private double DexRateOfFire()
        {
            if (HasConditionEffect(ConditionEffects.Dazed))
                return 0.0015;

            var rof = 0.0015 + (Stats[5] / 75.0) * (0.008 - 0.0015);

            if (HasConditionEffect(ConditionEffects.Berserk))
                rof = rof * 1.5;

            return rof;
        }

        public bool IsNoClipping()
        {
            if (Owner == null || !TileOccupied(RealX, RealY) && !TileFullOccupied(RealX, RealY))
                return false;

            CheatLog.Info($"{Name} is walking on an occupied tile.");
            return true;
        }
    }
}
