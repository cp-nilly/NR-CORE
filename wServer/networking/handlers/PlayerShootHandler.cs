using common.resources;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.realm;
using log4net;

namespace wServer.networking.handlers
{
    class PlayerShootHandler : PacketHandlerBase<PlayerShoot>
    {
        public override PacketId ID => PacketId.PLAYERSHOOT;
        private static readonly ILog CheatLog = LogManager.GetLogger("CheatLog");

        protected override void HandlePacket(Client client, PlayerShoot packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet, t));
            //Handle(client.Player, packet);
        }
        
        void Handle(Player player, PlayerShoot packet, RealmTime time)
        {
            if (player?.Owner == null) 
                return;
            
            Item item;
            if (!player.Manager.Resources.GameData.Items.TryGetValue(packet.ContainerType, out item))
                return;

            // validate
            int? infCount;
            if ((infCount = player.ValidatePlayerShootPacket(item, packet.Time, time)) != null)
            { // anti cheat measure
                if (infCount > 50)
                {
                    CheatLog.Info($"{player.Name} kicked for messing with PlayerShoot");
                    player.Client.Disconnect();
                }
                return;
            }

            // if not shooting main weapon do nothing (ability shoot is handled with useItem)
            if (player.Inventory[0] != item)
                return;

            // create projectile and show other players
            var prjDesc = item.Projectiles[0]; //Assume only one
            Projectile prj = player.PlayerShootProjectile(
                packet.BulletId, prjDesc, item.ObjectType,
                packet.Time, packet.StartingPos, packet.Angle);
            player.Owner.EnterWorld(prj);
            player.Owner.BroadcastPacketNearby(new AllyShoot()
            {
                OwnerId = player.Id,
                Angle = packet.Angle,
                ContainerType = packet.ContainerType,
                BulletId = packet.BulletId
            }, player, player, PacketPriority.Low);
            player.FameCounter.Shoot(prj);
        }
    }
}
