using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.realm;

namespace wServer.networking
{
    public partial class Client
    {
        private long _pingTime = -1;
        private long _pongTime = -1;
        private int _serial;

        const int PingPeriod = 3000;
        const int DcThresold = 15000;

        public bool KeepAlive(RealmTime time, int position, int count)
        {
            if (_pingTime == -1)
            {
                _pingTime = time.TotalElapsedMs - PingPeriod;
                _pongTime = time.TotalElapsedMs;
            }

            // check for disconnect timeout
            if (time.TotalElapsedMs - _pongTime > DcThresold)
            {
                Disconnect("Queue connection timeout. (KeepAlive)");
                return false;
            }

            if (time.TotalElapsedMs - _pingTime < PingPeriod)
            {
                return true;
            }
            
            // send ping
            _pingTime = time.TotalElapsedMs;
            _serial = (int) _pingTime;
            SendPacket(new QueuePing()
            {
                Serial = _serial,
                Position = position,
                Count = count
            });
            return true;
        }

        public void Pong(RealmTime time, QueuePong pongPkt)
        {
            if (pongPkt.Serial == _serial)
                _pongTime = time.TotalElapsedMs;
        }
    }
}
