using System;
using common;
using log4net;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class ISControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ISControl));

        private readonly RealmManager _manager;
        private bool _rebooting;

        public ISControl(RealmManager manager)
        {
            _manager = manager;

            // listen to control communications
            _manager.InterServer.AddHandler<ControlMsg>(Channel.Control, HandleControl);
        }

        private void HandleControl(object sender, InterServerEventArgs<ControlMsg> e)
        {
            var c = e.Content;
            var serverInfo = _manager.InterServer.GetServerInfo(e.InstanceId);
            switch (c.Type)
            {
                case ControlType.Reboot:
                    if (c.TargetInst.Equals(_manager.InstanceId))
                    {
                        Log.Info($"Server received control message to reboot from {c.Issuer} on {serverInfo?.name}.");
                        Reboot();
                    }
                    break;
            }
        }

        private void Reboot()
        {
            if (_rebooting)
                return;

            _rebooting = true;

            WorldTimer tmr = null;
            var s = 30;
            Func<World, RealmTime, bool> rebootTick = (w, t) =>
            {
                s -= 1;

                if (s == 15)
                    _manager.Chat.Announce("Server rebooting in 15 seconds...", true);
                else if (s == 5)
                    _manager.Chat.Announce("Server rebooting in 5 seconds...", true);
                else if (s == 0)
                {
                    // this could help avoid unfinished transactions when rebooting
                    foreach (var world in _manager.Worlds.Values)
                    {
                        world.Closed = true;
                        foreach (var p in world.Players.Values)
                            p.Client?.Disconnect();     
                    }
                    Program.Stop();
                    return true;
                }


                tmr.Reset();
                return false;
            };

            tmr = new WorldTimer(1000, rebootTick);
            _manager.Chat.Announce("Server rebooting in 30 seconds...", true);
            _manager.GetWorld(World.Nexus).Timers.Add(tmr);
        }
    }
}
