using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using wServer.networking.packets;

namespace wServer.realm.entities
{
    partial class Player
    {
        private readonly ConcurrentQueue<Tuple<Packet, Predicate<Player>>> _pendingPackets = 
            new ConcurrentQueue<Tuple<Packet, Predicate<Player>>>();

        internal void Flush()
        {
            var players = Owner.Players.Values;

            Tuple<Packet, Predicate<Player>> pac;
            while (_pendingPackets.TryDequeue(out pac))
                foreach (var plr in players)
                {
                    if (plr == null || pac.Item1 == null || !pac.Item2(plr))
                        continue;

                    plr._client.SendPacket(pac.Item1, PacketPriority.Low);
                }
        }

        public void BroadcastSync(Packet packet)
        {
            BroadcastSync(packet, _ => true);
        }
        public void BroadcastSync(Packet packet, Predicate<Player> cond)
        {
            _pendingPackets.Enqueue(Tuple.Create(packet, cond));
        }
        public void BroadcastSync(IEnumerable<Packet> packets)
        {
            foreach (var i in packets)
                BroadcastSync(i, _ => true);
        }
        public void BroadcastSync(IEnumerable<Packet> packets, Predicate<Player> cond)
        {
            foreach (var i in packets)
                BroadcastSync(i, cond);
        }
    }
}
