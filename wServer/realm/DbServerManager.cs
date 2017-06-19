using System;
using System.Linq;
using log4net;
using common;
using wServer.networking.packets.outgoing;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class DbServerManager
    {
        public const string FORCE_PRIVATE_MESSAGE_REFRESH = "forcePrivateMessageRefresh";

        private struct Message
        {
            public string Type;
            public string Inst;

            public string TargetPlayer;
        }

        private readonly RealmManager manager;

        public DbServerManager(RealmManager manager)
        {
            this.manager = manager;
            manager.InterServer.AddHandler<Message>(Channel.Control, HandleControl);
        }

        private void HandleControl(object sender, InterServerEventArgs<Message> e)
        {
            switch (e.Content.Type)
            {
                case FORCE_PRIVATE_MESSAGE_REFRESH:
                    foreach (var i in manager.Clients.Keys
                        .Where(x => (x.Player?.Owner?.IsNotCombatMapArea ?? false) && String.Equals(x.Account.Name, e.Content.TargetPlayer, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        i.SendPacket(new GlobalNotification
                        {
                            Text = "forcePrivateMessageRefresh"
                        });
                    }
                    break;
            }
        }
    }
}
