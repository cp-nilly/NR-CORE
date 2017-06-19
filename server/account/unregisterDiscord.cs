using System.Collections.Specialized;
using Anna.Request;
using common;
using log4net;

namespace server.account
{
    class unregisterDiscord : RequestHandler
    {
        private static readonly ILog RankManagerLog = LogManager.GetLogger("RankManagerLog");

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status == LoginStatus.OK)
            {
                if (!acc.RankManager)
                {
                    Write(context, "<Error>No permission</Error>");
                    return;
                }

                var accId = Database.ResolveId(query["ign"]);
                if (accId == 0)
                {
                    Write(context, "<Error>Account does not exist</Error>");
                    return;
                }

                var dId = query["dId"];
                if (string.IsNullOrEmpty(dId))
                {
                    Write(context, "<Error>Invalid discord id</Error>");
                    return;
                }

                if (!Database.UnregisterDiscord(dId, accId))
                {
                    Write(context, "<Error>Account not linked to discord id</Error>");
                    return;
                }

                Write(context, "<Success/>");
                RankManagerLog.Info($"[{acc.Name}] Unregistered discord from account ({dId}:{accId})");
            }
            else
                Write(context, "<Error>" + status.GetInfo() + "</Error>");
        }
    }
}
