using System.Collections.Specialized;
using Anna.Request;
using common;
using log4net;

namespace server.account
{
    class changePassword : RequestHandler
    {
        private static readonly ILog PassLog = LogManager.GetLogger("PassLog");

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status == LoginStatus.OK)
            {
                Database.ChangePassword(query["guid"], query["newPassword"]);
                Write(context, "<Success />");
                PassLog.Info($"Password changed. IP: {context.Request.ClientIP()}, Account: {acc.Name} ({acc.AccountId})");
            }
            else
                Write(context, "<Error>" + status.GetInfo() + "</Error>");
        }
    }
}
