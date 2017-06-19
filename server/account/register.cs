using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.account
{
    class register : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            if (!Utils.IsValidEmail(query["newGUID"]))
                Write(context, "<Error>Invalid email</Error>");
            else
            {
                string key = Database.REG_LOCK;
                string lockToken = null;
                try
                {
                    while ((lockToken = Database.AcquireLock(key)) == null) ;

                    DbAccount acc;
                    var status = Database.Verify(query["guid"], "", out acc);
                    if (status == LoginStatus.OK)
                    {
                        //what? can register in game? kill the account lock
                        if (!Database.RenameUUID(acc, query["newGUID"], lockToken))
                        {
                            Write(context, "<Error>Duplicate Email</Error>");
                            return;
                        }
                        Database.ChangePassword(acc.UUID, query["newPassword"]);
                        Database.Guest(acc, false);
                        Write(context, "<Success />");
                    }
                    else
                    {
                        var s = Database.Register(query["newGUID"], query["newPassword"], false, out acc);
                        if (s == RegisterStatus.OK)
                            Write(context, "<Success />");
                        else
                            Write(context, "<Error>" + s.GetInfo() + "</Error>");
                    }
                }
                finally
                {
                    if (lockToken != null)
                        Database.ReleaseLock(key, lockToken);
                }
            }
        }
    }
}
