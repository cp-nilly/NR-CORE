using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.@char
{
    class purchaseClassUnlock : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status == LoginStatus.OK)
            {
                var cType = (ushort)Utils.FromString(query["classType"]);
                var playerDesc = Program.Resources.GameData.Classes[cType];
                    
                if (playerDesc.Unlock == null ||
                    playerDesc.Unlock.Cost == null)
                {
                    Write(context, "<Error>Bad input to character unlock</Error>");
                    return;
                }

                if (acc.Credits < playerDesc.Unlock.Cost)
                {
                    Write(context, "<Error>Not enough gold</Error>");
                    return;
                }

                int cost = (int) playerDesc.Unlock.Cost;
                Program.Database.UpdateCredit(acc, -cost);
                Program.Database.UnlockClass(acc, cType);
                Write(context, "<Success />");
            }
            else
                Write(context, "<Error>" + status.GetInfo() + "</Error>");
        }
    }
}
