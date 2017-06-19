using System;
using System.Collections;
using System.Linq;
using common;
using wServer.networking;
using wServer.networking.packets.incoming;

namespace wServer.realm
{
    public class ConInfo : IEquatable<ConInfo>
    {
        public readonly Client Client;
        public readonly DbAccount Account;
        public readonly string GUID;
        public readonly int GameId;
        public readonly byte[] Key;
        public readonly bool Reconnecting;
        public readonly string MapInfo;
        public readonly long Time;
        

        public ConInfo(Client client, Hello pkt)
        {
            Client = client;
            Account = client.Account;
            GUID = pkt.GUID;
            GameId = pkt.GameId;
            Key = pkt.Key;
            Reconnecting = !Key.SequenceEqual(Empty<byte>.Array);
            MapInfo = pkt.MapJSON;
            Time = DateTime.UtcNow.ToUnixTimestamp();
        }

        public bool Equals(ConInfo other)
        {
            return GUID.Equals(other.GUID);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is ConInfo)
            {
                var p = (ConInfo)obj;
                return Equals(p);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return GUID.GetHashCode();
        }
    }

    internal class ByRank : IComparer
    {
        public int Compare(object x, object y)
        {
            var a = (ConInfo) x;
            var b = (ConInfo) y;

            // guid check
            if (a.GUID.Equals(b.GUID))
                return 0; // will force an exception which will be used to prevent duplicates

            // by rank with adjustment for reconnecting players
            var xSortVal = a.Account.Rank + (a.Reconnecting ? 101 : 0);
            var ySortVal = b.Account.Rank + (b.Reconnecting ? 101 : 0);
            var result = -1 * xSortVal.CompareTo(ySortVal);

            // by time added
            if (result == 0)
                result = a.Time.CompareTo(b.Time);

            // if the same time, push later
            if (result == 0)
                result = 1;

            return result;
        }
    }

    class ConnectionQueue
    {
        private readonly SortedList _queue;
        public int Count => _queue.Count;

        public ConnectionQueue()
        {
            _queue = new SortedList(new ByRank());
        }

        public bool Add(ConInfo conInfo)
        {
            try
            {
                _queue.Add(conInfo, null);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public ConInfo Remove()
        {
            var ret = (ConInfo) _queue.GetKey(0);
            _queue.RemoveAt(0);
            return ret;
        }

        public void KeepAlive(RealmTime time)
        {
            for (var i = _queue.Count - 1; i >= 0; i--)
            {
                var conInfo = (ConInfo) _queue.GetKey(i);
                if (conInfo.Client.KeepAlive(time, i + 1, Count))
                    continue;
                
                _queue.RemoveAt(i);
            }
        }

        public int Position(ConInfo conInfo)
        {
            return _queue.IndexOfKey(conInfo) + 1;
        }
    }
}
