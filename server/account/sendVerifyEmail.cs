using System.Collections.Specialized;
using Anna.Request;

namespace server.account
{
    class sendVerifyEmail : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            Write(context, "<Error>Nope.</Error>");
        }
    }
}
