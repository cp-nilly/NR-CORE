using common;
using log4net;
using System.Threading;

namespace server
{
    public class ISControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ISControl));

        private readonly ISManager _manager;

        public ISControl(ISManager manager)
        {
            _manager = manager;

            // listen to control communications
            _manager.AddHandler<ControlMsg>(Channel.Control, HandleControl);
        }

        private void HandleControl(object sender, InterServerEventArgs<ControlMsg> e)
        {
            var c = e.Content;
            var serverInfo = _manager.GetServerInfo(e.InstanceId);
            switch (c.Type)
            {
                case ControlType.Reboot:
                    if (c.TargetInst.Equals(_manager.InstanceId))
                    {
                        Log.Info($"Server received control message to reboot from {c.Issuer} on {serverInfo?.name}.");
                        if (c.Delay > 0)
                        {
                            Thread.Sleep(c.Delay);
                        }
                        Program.Stop();
                    }
                    break;
            }
        }
    }
}
