using common.resources;
using wServer.realm;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class EnemyHitHandler : PacketHandlerBase<EnemyHit>
    {
        public override PacketId ID => PacketId.ENEMYHIT;

        private static readonly RealmTime DummyTime = new RealmTime();

        protected override void HandlePacket(Client client, EnemyHit packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, t, packet));
            //Handle(client.Player, DummyTime, packet); 
                // time currently isn't used by any hit detection (actually there is no hit detection right now)
        }

        void Handle(Player player, RealmTime time, EnemyHit pkt)
        {
            var entity = player?.Owner?.GetEntity(pkt.TargetId);
            if (entity?.Owner == null)
                return;

            if (player.Client.IsLagging || player.HasConditionEffect(ConditionEffects.Hidden))
                return;

            var prj = (player as IProjectileOwner).Projectiles[pkt.BulletId];
            if (prj == null)
                Log.Debug("prj is dead...");
            prj?.ForceHit(entity, time);

            if (pkt.Killed)
                player.ClientKilledEntity.Enqueue(entity);
        }
    }
}
