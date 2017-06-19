using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.privateMessage
{
    class delete : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            LoginStatus s;
            if ((s = Database.Verify(query["guid"], query["password"], out acc)) == LoginStatus.OK)
                acc.PrivateMessages
                    .DelteMessage(Database, int.Parse(query["time"]))
                    .ContinueWith(t => Write(context, "<Success />"));
            else
                Write(context, $"<Error>{s.GetInfo()}</Error>");
        }
    }
}
