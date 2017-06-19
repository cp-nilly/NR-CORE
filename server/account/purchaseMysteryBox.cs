using System.Collections.Specialized;
using System.Threading.Tasks;
using Anna.Request;
using common;
using common.resources;
using StackExchange.Redis;

namespace server.account
{
    class purchaseMysteryBox : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status != LoginStatus.OK)
            {
                Write(context, $"<Error>{status.GetInfo()}</Error>");
                return;
            }

            // get box
            var box = Program.Resources.MysteryBoxes[query["boxId"].ToInt32()];
            if (box == null)
            {
                Write(context, "<Error>Invalid box</Error>");
                return;
            }

            // purchase box
            ITransaction tran;
            var pResult = box.Purchase(Database, acc, out tran);
            if (pResult.Result != MBoxPurchaseResult.Success)
            {
                if (pResult.Result == MBoxPurchaseResult.NotEnoughGold)
                    Write(context, "<Error>Not Enough Gold</Error>");
                else if (pResult.Result == MBoxPurchaseResult.NotEnoughFame)
                    Write(context, "<Error>Not Enough Fame</Error>");
                else if (pResult.Result == MBoxPurchaseResult.InvalidCurrency)
                    Write(context, "<Error>Invalid Currency</Error>");
                else
                    Write(context, "<Error>Unknown</Error>");
                return;
            }

            // save gifts
            Database.AddGifts(acc, pResult.Awards, tran);
            tran.ExecuteAsync().ContinueWith(t =>
            {
                var success = !t.IsCanceled && t.Result;
                if (!success)
                {
                    Write(context, "<Error>Transaction Failed</Error>");
                    return;
                }

                Write(context, pResult.Xml);
            }).ContinueWith(e =>
                Program.Log.Error(e.Exception.InnerException.ToString()),
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
