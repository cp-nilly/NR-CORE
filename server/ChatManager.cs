using System.Linq;
using common;
using log4net;

namespace server
{
    public class ChatManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatManager));
        
        private readonly InterServerChannel _interServer;

        public ChatManager(InterServerChannel interServer)
        {
            _interServer = interServer;

            // listen to chat communications
            _interServer.AddHandler<ChatMsg>(Channel.Chat, HandleChat);
        }

        private void HandleChat(object sender, InterServerEventArgs<ChatMsg> e)
        {
            switch (e.Content.Type)
            {
                case ChatType.Tell:
                    {
                        var from = _interServer.Database.ResolveIgn(e.Content.From);
                        var to = _interServer.Database.ResolveIgn(e.Content.To);
                        bool filtered = Program.Resources.FilterList.Any(r => r.IsMatch(e.Content.Text));
                        Log.InfoFormat("<{0} -> {1}{2}> {3}", from, to, filtered ? " *filtered*" : "", e.Content.Text);
                        break;
                    }
                case ChatType.Guild:
                    {
                        var from = _interServer.Database.ResolveIgn(e.Content.From);
                        Log.InfoFormat("<{0} -> Guild> {1}", from, e.Content.Text);
                        break;
                    }
                case ChatType.Announce:
                    Log.InfoFormat("<Announcement> {0}", e.Content.Text);
                    break;
            }
        }
    }
}
