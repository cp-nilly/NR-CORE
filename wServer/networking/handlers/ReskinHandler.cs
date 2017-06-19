using System.Linq;
using common.resources;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class ReskinHandler : PacketHandlerBase<Reskin>
    {
        public override PacketId ID => PacketId.RESKIN;

        protected override void HandlePacket(Client client, Reskin packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, (ushort)packet.SkinId));
            Handle(client, (ushort)packet.SkinId);
        }

        void Handle(Client client, ushort skin)
        {
            if (client.Player == null)
                return;

            var gameData = client.Manager.Resources.GameData;

            var ownedSkins = client.Account.Skins;
            var currentClass = client.Player.ObjectType;

            var skinData = gameData.Skins;
            var skinSize = 100;

            if (skin != 0)
            {
                SkinDesc skinDesc;
                skinData.TryGetValue(skin, out skinDesc);

                if (skinDesc == null)
                {
                    client.Player.SendError("Unknown skin type.");
                    return;
                }

                if (!ownedSkins.Contains(skin))
                {
                    client.Player.SendError("Skin not owned.");
                    return;
                }

                if (skinDesc.PlayerClassType != currentClass)
                {
                    client.Player.SendError("Skin is for different class.");
                    return;
                }

                if (skinDesc.NoSkinSelect)
                {
                    client.Player.SendError("Cannot select this skin.");
                    return;
                }

                skinSize = skinDesc.Size;
            }

            // set skin
            client.Player.SetDefaultSkin(skin);
            client.Player.SetDefaultSize(skinSize);
        }
    }
}
