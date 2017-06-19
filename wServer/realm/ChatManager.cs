using System;
using System.Linq;
using log4net;
using common;
using wServer.networking.packets.outgoing;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class ChatManager : IDisposable
    {
        private static readonly string[] exclusiveEmotes = { ":whitebag:", ":bluebag:", ":cyanbag:", ":rip:", ":pbag:" };

        static ILog log = LogManager.GetLogger(typeof(ChatManager));

        RealmManager manager;
        public ChatManager(RealmManager manager)
        {
            this.manager = manager;
            manager.InterServer.AddHandler<ChatMsg>(Channel.Chat, HandleChat);
            manager.InterServer.NewServer += AnnounceNewServer;
            manager.InterServer.ServerQuit += AnnounceServerQuit;
        }

        private void AnnounceNewServer(object sender, EventArgs e)
        {
            var networkMsg = (InterServerEventArgs<NetworkMsg>) e;
            if (networkMsg.Content.Info.type == ServerType.Account)
                return;
            Announce($"A new server has come online: {networkMsg.Content.Info.name}", true);
        }

        private void AnnounceServerQuit(object sender, EventArgs e)
        {
            var networkMsg = (InterServerEventArgs<NetworkMsg>)e;
            if (networkMsg.Content.Info.type == ServerType.Account)
                return;
            Announce($"Server, {networkMsg.Content.Info.name}, is no longer online.", true);
        }

        public void Dispose()
        {
            manager.InterServer.NewServer -= AnnounceNewServer;
            manager.InterServer.ServerQuit -= AnnounceServerQuit;
        }

        public void Say(Player src, string text)
        {
            foreach (var word in text.Split(' ')
                .Where(word => word.StartsWith(":") && word.EndsWith(":") && exclusiveEmotes.Contains(word))
                .Where(word => !src.Client.Account.Emotes.Contains(word)))
                text = text.Replace(word, string.Empty);

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (src.IsControlling)
            {
                Mob(src.SpectateTarget, text);
            }
            else
            {
                var tp = new Text()
                {
                    Name = (src.Client.Account.Admin ? "@" : "") + src.Name,
                    ObjectId = src.Id,
                    NumStars = src.Stars,
                    Admin = src.Admin,
                    BubbleTime = 5,
                    Recipient = "",
                    Txt = text,
                    CleanText = text,
                    NameColor = (src.Glow != 0) ? src.Glow : 0x123456,
                    TextColor = (src.Glow != 0) ? 0xFFFFFF : 0x123456
                };

                SendTextPacket(src, tp, p => !p.Client.Account.IgnoreList.Contains(src.AccountId));
            }
        }

        public bool Local(Player src, string text)
        {
            foreach (var word in text.Split(' ')
                .Where(word => word.StartsWith(":") && word.EndsWith(":") && exclusiveEmotes.Contains(word))
                .Where(word => !src.Client.Account.Emotes.Contains(word)))
                text = text.Replace(word, string.Empty);

            if (string.IsNullOrWhiteSpace(text))
                return true;

            var tp = new Text()
            {
                Name = (src.Client.Account.Admin ? "@" : "") + src.Name,
                ObjectId = src.Id,
                NumStars = src.Stars,
                Admin = src.Admin,
                BubbleTime = 5,
                Recipient = "",
                Txt = text,
                CleanText = text,
                NameColor = 0xAD85FF,
                TextColor = 0xAD85FF
            };

            SendTextPacket(src, tp,
                p => !p.Client.Account.IgnoreList.Contains(src.AccountId) &&
                     p.DistSqr(src) < Player.RadiusSqr);
            return true;
        }

        private void SendTextPacket(Player src, Text tp, Predicate<Player> conditional)
        {
            var filtered = manager.Resources.FilterList.Any(r => r.IsMatch(tp.Txt));

            if (filtered)
            {
                // message found in filter list, only send to clients with same ip as source
                src.Owner.BroadcastPacketConditional(tp,
                    p => conditional(p) && p.Client.Account.IP == src.Client.Account.IP);
            }
            else
            {
                src.Owner.BroadcastPacketConditional(tp, conditional);
            }

            log.Info($"[{src.Owner.Name}({src.Owner.Id}){(filtered ? " *filtered*" : "")}] <{src.Name}> {tp.Txt}");
        }

        public void Mob(Entity entity, string text)
        {
            if (string.IsNullOrWhiteSpace(text) || entity.Owner == null)
                return;

            var world = entity.Owner;
            var name = entity.ObjectDesc.DisplayId;

            world.BroadcastPacket(new Text()
            {
                ObjectId = entity.Id,
                BubbleTime = 5,
                NumStars = -1,
                Name = $"#{name}",
                Txt = text
            }, null, PacketPriority.Low);
            log.Info($"[{world.Name}({world.Id})] <{name}> {text}");
        }

        public void Announce(string text, bool local = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (local)
            {
                foreach (var i in manager.Clients.Keys
                .Where(x => x.Player != null)
                .Select(x => x.Player))
                {
                    i.AnnouncementReceived(text);
                }
                return;
            }

            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = ChatType.Announce,
                Inst = manager.InstanceId,
                Text = text
            });
        }

        public bool SendInfo(int target, string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return true;

            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = ChatType.Info,
                Inst = manager.InstanceId,
                To = target,
                Text = text
            });
            return true;
        }


        public void Oryx(World world, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            
            world.BroadcastPacket(new Text()
            {
                BubbleTime = 0,
                NumStars = -1,
                Name = "#Oryx the Mad God",
                Txt = text
            }, null, PacketPriority.Low);
            log.InfoFormat("[{0}({1})] <Oryx the Mad God> {2}", world.Name, world.Id, text);
        }
        
        public bool Tell(Player src, string target, string text)
        {
            foreach (var word in text.Split(' ')
                .Where(word => word.StartsWith(":") && word.EndsWith(":") && exclusiveEmotes.Contains(word))
                .Where(word => !src.Client.Account.Emotes.Contains(word)))
                text = text.Replace(word, string.Empty);

            if (String.IsNullOrWhiteSpace(text))
                return true;
            
            int id = manager.Database.ResolveId(target);
            if (id == 0) return false;

            if (!manager.Database.AccountLockExists(id))
                return false;

            var acc = manager.Database.GetAccount(id);
            if (acc == null || acc.Hidden && src.Admin == 0)
                return false;
            
            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = ChatType.Tell,
                Inst = manager.InstanceId,
                ObjId = src.Id,
                Stars = src.Stars,
                Admin = src.Admin,
                From = src.Client.Account.AccountId,
                To = id,
                Text = text,
                SrcIP = src.Client.IP
            });
            return true;
        }

        public bool Invite(Player src, string target, string dungeon, int wid)
        {
            int id = manager.Database.ResolveId(target);
            if (id == 0) return false;

            if (!manager.Database.AccountLockExists(id))
                return false;

            var acc = manager.Database.GetAccount(id);
            if (acc == null || acc.Hidden && src.Admin == 0)
                return false;
            
            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = ChatType.Invite,
                Inst = manager.InstanceId,
                ObjId = wid,
                From = src.Client.Account.AccountId,
                To = id,
                Text = dungeon
            });
            return true;
        }

        public bool Guild(Player src, string text, bool announce = false)
        {
            foreach (var word in text.Split(' ').Where(word => word.StartsWith(":") && word.EndsWith(":") && exclusiveEmotes.Contains(word)).Where(word => !src.Client.Account.Emotes.Contains(word)))
                text = text.Replace(word, String.Empty);

            if (String.IsNullOrWhiteSpace(text))
                return true;
            
            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = (announce) ? ChatType.GuildAnnounce : ChatType.Guild,
                Inst = manager.InstanceId,
                ObjId = src.Id,
                Stars = src.Stars,
                Admin = src.Admin,
                From = src.Client.Account.AccountId,
                To = src.Client.Account.GuildId,
                Text = text
            });
            return true;
        }

        public bool GuildAnnounce(DbAccount acc, string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return true;

            manager.InterServer.Publish(Channel.Chat, new ChatMsg()
            {
                Type = ChatType.GuildAnnounce,
                Inst = manager.InstanceId,
                From = acc.AccountId,
                To = acc.GuildId,
                Text = text,
                Hidden = acc.Hidden
            });
            return true;
        }
        
        void HandleChat(object sender, InterServerEventArgs<ChatMsg> e)
        {
            switch (e.Content.Type)
            {
                case ChatType.Invite:
                    {
                        string from = manager.Database.ResolveIgn(e.Content.From);
                        foreach (var i in manager.Clients.Keys
                            .Where(x => x.Player != null)
                            .Where(x => !x.Account.IgnoreList.Contains(e.Content.From))
                            .Where(x => x.Account.AccountId == e.Content.To)
                            .Select(x => x.Player))
                        {
                            i.Invited(e.Content.ObjId, from, e.Content.Text);
                        }
                    } break;
                case ChatType.Tell:
                    {
                        string from = manager.Database.ResolveIgn(e.Content.From);
                        string to = manager.Database.ResolveIgn(e.Content.To);
                        bool filtered = manager.Resources.FilterList.Any(r => r.IsMatch(e.Content.Text));
                        foreach (var i in manager.Clients.Keys
                            .Where(x => x.Player != null)
                            .Where(x => !x.Account.IgnoreList.Contains(e.Content.From))
                            .Where(x => x.Account.AccountId == e.Content.From ||
                                        x.Account.AccountId == e.Content.To && (!filtered || x.Account.IP == e.Content.SrcIP))
                            .Select(x => x.Player))
                        {
                            i.TellReceived(
                                e.Content.Inst == manager.InstanceId ? e.Content.ObjId : -1,
                                e.Content.Stars, e.Content.Admin, from, to, e.Content.Text);
                        }
                    } break;
                case ChatType.Guild:
                    {
                        string from = manager.Database.ResolveIgn(e.Content.From);
                        foreach (var i in manager.Clients.Keys
                            .Where(x => x.Player != null)
                            .Where(x => !x.Account.IgnoreList.Contains(e.Content.From))
                            .Where(x => x.Account.GuildId > 0)
                            .Where(x => x.Account.GuildId == e.Content.To)
                            .Select(x => x.Player))
                        {
                            i.GuildReceived(
                                e.Content.Inst == manager.InstanceId ? e.Content.ObjId : -1,
                                e.Content.Stars, e.Content.Admin, from, e.Content.Text);
                        }
                    } break;
                case ChatType.GuildAnnounce:
                    {
                        foreach (var i in manager.Clients.Keys
                            .Where(x => x.Player != null)
                            .Where(x => x.Account.GuildId > 0)
                            .Where(x => x.Account.GuildId == e.Content.To)
                            .Where(x => !e.Content.Hidden || x.Account.Admin)
                            .Select(x => x.Player))
                        {
                            i.GuildReceived(-1, -1, 0, "", e.Content.Text);
                        }
                    } break;
                case ChatType.Announce:
                    {
                        foreach (var i in manager.Clients.Keys
                            .Where(x => x.Player != null)
                            .Select(x => x.Player))
                        {
                            i.AnnouncementReceived(e.Content.Text);
                        }
                    } break;
                case ChatType.Info:
                    {
                        var player = manager.Clients.Keys.Where(c => c.Account.AccountId == e.Content.To).FirstOrDefault();
                        player?.Player.SendInfo(e.Content.Text);
                    }
                    break;
            }
        }
    }
}
