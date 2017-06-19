using System.Collections.Specialized;
using Anna.Request;
using common;
using common.resources;

namespace server.account
{
    class purchasePackage : RequestHandler
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

            // get pkg
            var pkg = Program.Resources.Packages[query["packageId"].ToInt32()];
            if (pkg == null)
            {
                Write(context, "<Error>Invalid PackageId</Error>");
                return;
            }

            var result = pkg.Purchase(Database, acc).Result;
            switch (result)
            {
                case PkgPurchaseResult.MaxedPurchase:
                    Write(context, "<Error>Purchase failed. Purchase cap reached.</Error>");
                    return;
                case PkgPurchaseResult.QuantityZero:
                    Write(context, "<Error>Purchase failed. Package sold out.</Error>");
                    return;
                case PkgPurchaseResult.Error:
                    Write(context, "<Error>Purchase failed. An unknown error occured.</Error>");
                    return;
                case PkgPurchaseResult.TransactionFailed:
                    Write(context, "<Error>Transaction failed.</Error>");
                    return;
                case PkgPurchaseResult.InvalidCurrency:
                    Write(context, "<Error>Invalid package. Unsupported currency.</Error>");
                    return;
                case PkgPurchaseResult.NotEnoughFame:
                    Write(context, "<Error>Not enough fame.</Error>");
                    return;
                case PkgPurchaseResult.NotEnoughGold:
                    Write(context, "<Error>Not enough gold.</Error>");
                    return;
            }

            Write(context, "<Success/>");
        }
    }
}
