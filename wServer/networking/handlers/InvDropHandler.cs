using System;
using System.Linq;
using wServer.realm;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;

namespace wServer.networking.handlers
{
    class InvDropHandler : PacketHandlerBase<InvDrop>
    {
        static readonly Random InvRand = new Random();

        public override PacketId ID => PacketId.INVDROP;

        protected override void HandlePacket(Client client, InvDrop packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet.SlotObject.SlotId));
            Handle(client.Player, packet.SlotObject);
        }

        private void Handle(Player player, ObjectSlot slot)
        {
            if (player?.Owner == null || player.tradeTarget != null)
                return;

            const ushort normBag = 0x0500;
            const ushort soulBag = 0x0503;

            IContainer con;

            // container isn't always the player's inventory, it's given by the SlotObject's ObjectId
            if (slot.ObjectId != player.Id)
            {
                if (player.Owner.GetEntity(slot.ObjectId) is Player)
                {
                    player.Client.SendPacket(new InvResult() { Result = 1 });
                    return;
                }
                con = player.Owner.GetEntity(slot.ObjectId) as IContainer;
            } 
            else
            {
                con = player as IContainer;
            }


            if (slot.ObjectId == player.Id && player.Stacks.Any(stack => stack.Slot == slot.SlotId))
            {
                player.Client.SendPacket(new InvResult() { Result = 1 });
                return; // don't allow dropping of stacked items
            }
            
            if (con?.Inventory[slot.SlotId] == null)
            {
                //give proper error
                player.Client.SendPacket(new InvResult() { Result = 1 });
                return;
            }
            
            var item = con.Inventory[slot.SlotId];

            // if item is from gift chest, remove from user's gift chest list
            if (con is GiftChest)
            {
                var trans = player.Manager.Database.Conn.CreateTransaction();
                player.Manager.Database.RemoveGift(player.Client.Account, item.ObjectType, trans);
                if (!trans.Execute())
                {
                    player.Client.SendPacket(new InvResult() { Result = 1 });
                    return;
                }
            }

            con.Inventory[slot.SlotId] = null;

            // create new container for item to be placed in
            Container container;
            if (item.Soulbound || player.Client.Account.Admin)
            {
                container = new Container(player.Manager, soulBag, 1000 * 60, true);
                container.BagOwners = new int[] { player.AccountId };
            }
            else
            {
                container = new Container(player.Manager, normBag, 1000 * 60, true);
            }
                
            // init container
            container.Inventory[0] = item;
            container.Move(player.X + (float)((InvRand.NextDouble() * 2 - 1) * 0.5),
                            player.Y + (float)((InvRand.NextDouble() * 2 - 1) * 0.5));
            container.SetDefaultSize(75);
            player.Owner.EnterWorld(container);

            // send success
            player.Client.SendPacket(new InvResult() { Result = 0 });
        }
    }
}
