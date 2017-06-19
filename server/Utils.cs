using System.Linq;
using Anna.Request;

namespace server
{
    public static class AnnaExtentions
    {
        public static string ClientIP(this Request r)
        {
            if (r.Headers.ContainsKey("X-Forwarded-For"))
            {
                return r.Headers["X-Forwarded-For"].Last();
            }

            if (r.Headers.ContainsKey("remote_addr"))
            {
                return r.Headers["remote_addr"].Last();
            }

            return r.ListenerRequest.RemoteEndPoint?.Address.ToString();
        }
    }
}
