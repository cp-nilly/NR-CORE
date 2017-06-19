using System.Collections.Specialized;
using Anna.Request;

namespace server.friends
{
    class getRequests : RequestHandler
    {

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            // TODO
            Write(context, "<Requests></Requests>");
        }
    }
}