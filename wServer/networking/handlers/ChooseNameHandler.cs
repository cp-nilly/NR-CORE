using System;
using System.Linq;
using common;
using log4net;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;

namespace wServer.networking.handlers
{
    class ChooseNameHandler : PacketHandlerBase<ChooseName>
    {
        private static readonly ILog NameChangeLog = LogManager.GetLogger("NameChangeLog");

        public override PacketId ID => PacketId.CHOOSENAME;

        protected override void HandlePacket(Client client, ChooseName packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet));
            Handle(client, packet);
        }

        private void Handle(Client client, ChooseName packet)
        {
            if (client.Player == null || IsTest(client))
                return;

            client.Manager.Database.ReloadAccount(client.Account);

            string name = packet.Name;
            if (name.Length < 3 || name.Length > 15 || !name.All(char.IsLetter) ||
                Database.GuestNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                client.SendPacket(new NameResult()
                {
                    Success = false,
                    ErrorText = "Invalid name"
                });
            else
            {
                string key = Database.NAME_LOCK;
                string lockToken = null;
                try
                {
                    while ((lockToken = client.Manager.Database.AcquireLock(key)) == null) ;

                    if (client.Manager.Database.Conn.HashExists("names", name.ToUpperInvariant()))
                    {
                        client.SendPacket(new NameResult()
                        {
                            Success = false,
                            ErrorText = "Duplicated name"
                        });
                        return;
                    }

                    if (client.Account.NameChosen && client.Account.Fame < 5000)
                        client.SendPacket(new NameResult()
                        {
                            Success = false,
                            ErrorText = "Not enough fame"
                        });
                    else
                    {
                        // remove fame is purchasing name change
                        if (client.Account.NameChosen)
                            client.Manager.Database.UpdateFame(client.Account, -5000);

                        // rename
                        var oldName = client.Account.Name;
                        while (!client.Manager.Database.RenameIGN(client.Account, name, lockToken)) ;
                        NameChangeLog.Info($"{oldName} changed their name to {name}");

                        // update player
                        UpdatePlayer(client.Player);
                        client.SendPacket(new NameResult()
                        {
                            Success = true,
                            ErrorText = ""
                        });
                    }
                }
                finally
                {
                    if (lockToken != null)
                        client.Manager.Database.ReleaseLock(key, lockToken);
                }
            }
        }
        
        private void UpdatePlayer(Player player)
        {
            player.Credits = player.Client.Account.Credits;
            player.CurrentFame = player.Client.Account.Fame;
            player.Name = player.Client.Account.Name;
            player.NameChosen = true;
        }
    }
}
