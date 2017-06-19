using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.account
{
    class verifyage : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status == LoginStatus.OK)
            {
                if (query["isAgeVerified"].Equals("1"))
                    Database.ChangeAgeVerified(acc, true);
                else
                    Database.ChangeAgeVerified(acc, false);

                Write(context, "<Success />"); 
            }
            else
                Write(context, "<Error>" + status.GetInfo() + "</Error>");
        }
    }
}
