using System.Collections.Concurrent;
using System.Linq;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.realm.worlds.logic;

namespace wServer.realm.entities
{
    public partial class Player
    {
        private const int PingPeriod = 3000;
        public const int DcThresold = 12000;

        private long _pingTime = -1;
        private long _pongTime = -1;
        
        private int _cnt;

        private long _sum;
        public long TimeMap { get; private set; }
        
        private long _latSum;
        public int Latency { get; private set; }

        public int LastClientTime = -1;
        public long LastServerTime = -1;

        private readonly ConcurrentQueue<long> _shootAckTimeout = 
            new ConcurrentQueue<long>();
        private readonly ConcurrentQueue<long> _updateAckTimeout =
            new ConcurrentQueue<long>(); 
        private readonly ConcurrentQueue<long> _gotoAckTimeout =
            new ConcurrentQueue<long>();
        private readonly ConcurrentQueue<int> _move =
            new ConcurrentQueue<int>();
        private readonly ConcurrentQueue<int> _clientTimeLog =
            new ConcurrentQueue<int>();
        private readonly ConcurrentQueue<int> _serverTimeLog =
            new ConcurrentQueue<int>();
        
        bool KeepAlive(RealmTime time)
        {
            if (_pingTime == -1)
            {
                _pingTime = time.TotalElapsedMs - PingPeriod;
                _pongTime = time.TotalElapsedMs;
            }

            // check for disconnect timeout
            if (time.TotalElapsedMs - _pongTime > DcThresold)
            {
                _client.Disconnect("Connection timeout. (KeepAlive)");
                return false;
            }

            long timeout;

            // check for shootack timeout
            if (_shootAckTimeout.TryPeek(out timeout))
            {
                if (time.TotalElapsedMs > timeout)
                {
                    _client.Disconnect("Connection timeout. (ShootAck)");
                    return false;
                }
            }

            // check for updateack timeout
            if (_updateAckTimeout.TryPeek(out timeout))
            {
                if (time.TotalElapsedMs > timeout)
                {
                    _client.Disconnect("Connection timeout. (UpdateAck)");
                    return false;
                }
            }

            // check for gotoack timeout
            if (_gotoAckTimeout.TryPeek(out timeout))
            {
                if (time.TotalElapsedMs > timeout)
                {
                    _client.Disconnect("Connection timeout. (GotoAck)");
                    return false;
                }
            }

            if (time.TotalElapsedMs - _pingTime < PingPeriod)
                return true;

            // send ping
            _pingTime = time.TotalElapsedMs;
            _client.SendPacket(new Ping()
            {
                Serial = (int)time.TotalElapsedMs
            });
            return UpdateOnPing();
        }

        public void Pong(RealmTime time, Pong pongPkt)
        {
            _cnt++;

            _sum += time.TotalElapsedMs - pongPkt.Time;
            TimeMap = _sum / _cnt;
            
            _latSum += (time.TotalElapsedMs - pongPkt.Serial) / 2;
            Latency = (int) _latSum / _cnt;

            _pongTime = time.TotalElapsedMs;
        }

        private bool UpdateOnPing()
        {
            // renew account lock
            try
            {
                if (!Manager.Database.RenewLock(_client.Account))
                    _client.Disconnect("RenewLock failed. (Pong)");
            }
            catch
            {
                _client.Disconnect("RenewLock failed. (Timeout)");
                return false;
            }

            // save character
            if (!(Owner is Test))
            {
                SaveToCharacter();
                _client.Character.FlushAsync();
            }
            return true;
        }

        public long C2STime(int clientTime)
        {
            return clientTime + TimeMap;
        }

        public long S2CTime(int serverTime)
        {
            return serverTime - TimeMap;
        }

        public void AwaitShootAck(long serverTime)
        {
            _shootAckTimeout.Enqueue(serverTime + DcThresold);
        }

        public void ShootAckReceived()
        {
            long ignored;
            if (!_shootAckTimeout.TryDequeue(out ignored))
            {
                _client.Disconnect("One too many ShootAcks");
            }
        }

        public void AwaitUpdateAck(long serverTime)
        {
            _updateAckTimeout.Enqueue(serverTime + DcThresold);
        }

        public void UpdateAckReceived()
        {
            long ignored;
            if (!_updateAckTimeout.TryDequeue(out ignored))
            {
                _client.Disconnect("One too many UpdateAcks");
            }
        }

        public void AwaitGotoAck(long serverTime)
        {
            _gotoAckTimeout.Enqueue(serverTime + DcThresold);
        }

        public void GotoAckReceived()
        {
            long ignored;
            if (!_gotoAckTimeout.TryDequeue(out ignored))
            {
                _client.Disconnect("One too many GotoAcks");
            }
        }

        public void AwaitMove(int tickId)
        {
            _move.Enqueue(tickId);
        }

        public void MoveReceived(RealmTime time, Move pkt)
        {
            int tickId;
            if (!_move.TryDequeue(out tickId))
            {
                _client.Disconnect("One too many MovePackets");
                return;
            }

            if (tickId != pkt.TickId)
            {
                _client.Disconnect("[NewTick -> Move] TickIds don't match");
                return;
            }

            if (pkt.TickId > TickId)
            {
                _client.Disconnect("[NewTick -> Move] Invalid tickId");
                return;
            }

            var lastClientTime = LastClientTime;
            var lastServerTime = LastServerTime;
            LastClientTime = pkt.Time;
            LastServerTime = time.TotalElapsedMs;

            if (lastClientTime == -1)
                return;
            
            _clientTimeLog.Enqueue(pkt.Time - lastClientTime);
            _serverTimeLog.Enqueue((int)(time.TotalElapsedMs - lastServerTime));

            if (_clientTimeLog.Count < 30)
                return;

            if (_clientTimeLog.Count > 30)
            {
                int ignore;
                _clientTimeLog.TryDequeue(out ignore);
                _serverTimeLog.TryDequeue(out ignore);
            }

            // calculate average
            var clientDeltaAvg = _clientTimeLog.Sum() / _clientTimeLog.Count;
            var serverDeltaAvg = _serverTimeLog.Sum() / _serverTimeLog.Count;
            var dx = clientDeltaAvg > serverDeltaAvg 
                ? clientDeltaAvg - serverDeltaAvg
                : serverDeltaAvg - clientDeltaAvg;
            if (dx > 15)
            {
                Log.Debug($"TickId: {tickId}, Client Delta: {_clientTimeLog.Sum() / _clientTimeLog.Count}, Server Delta: {_serverTimeLog.Sum() / _serverTimeLog.Count}");
            }
        }
    }
}
