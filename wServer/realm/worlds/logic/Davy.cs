using common.resources;
using wServer.networking;
using wServer.networking.packets.outgoing;
using wServer.realm.entities;

namespace wServer.realm.worlds.logic
{
    class Davy : World
    {
        private bool _greenFound;
        private bool _redFound;
        private bool _yellowFound;
        private bool _purpleFound;

        public Davy(ProtoWorld proto, Client client = null) : base(proto)
        {
        }

        public override int EnterWorld(Entity entity)
        {
            var player = entity as Player;
            if (player != null)
            {
                var client = player.Client;

                client.SendPacket(new GlobalNotification() { Text = "showKeyUI" });

                if (_purpleFound)
                    client.SendPacket(new GlobalNotification() { Text = "purple" });
                if (_greenFound)
                    client.SendPacket(new GlobalNotification() { Text = "green" });
                if (_redFound)
                    client.SendPacket(new GlobalNotification() { Text = "red" });
                if (_yellowFound)
                    client.SendPacket(new GlobalNotification() { Text = "yellow" });
            }
            
            return base.EnterWorld(entity);
        }

        public override void LeaveWorld(Entity entity)
        {
            base.LeaveWorld(entity);

            if (entity.ObjectDesc.ObjectId.Equals("Purple Key"))
            {
                _purpleFound = true;
                BroadcastPacket(new GlobalNotification() { Text = "purple" }, null);
                return;
            }

            if (entity.ObjectDesc.ObjectId.Equals("Green Key"))
            {
                _greenFound = true;
                BroadcastPacket(new GlobalNotification() { Text = "green" }, null);
                return;
            }

            if (entity.ObjectDesc.ObjectId.Equals("Red Key"))
            {
                _redFound = true;
                BroadcastPacket(new GlobalNotification() { Text = "red" }, null);
                return;
            }

            if (entity.ObjectDesc.ObjectId.Equals("Yellow Key"))
            {
                _yellowFound = true;
                BroadcastPacket(new GlobalNotification() { Text = "yellow" }, null);
                return;
            }
        }
    }
}
