using System.Collections.Specialized;
using Anna.Request;

namespace server.app
{
    class getServerXmls : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            Write(context, Program.Resources.GameData.ZippedXmls, true);
        }
    }
}
