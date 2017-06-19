using System;
using System.Collections.Specialized;
using System.Linq;
using Anna.Request;
using common;

namespace server.account
{
    class setName : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            string name = query["name"];
            if (name.Length < 3 || name.Length > 15 || !name.All(char.IsLetter) ||
                Database.GuestNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                Write(context, "<Error>Invalid name</Error>");
            else
            {
                string key = Database.NAME_LOCK;
                string lockToken = null;
                try
                {
                    while ((lockToken = Database.AcquireLock(key)) == null) ;

                    if (Database.Conn.HashExists("names", name.ToUpperInvariant()))
                    {
                        Write(context, "<Error>Duplicated name</Error>");
                        return;
                    }

                    DbAccount acc;
                    var status = Database.Verify(query["guid"], query["password"], out acc);
                    if (status == LoginStatus.OK)
                    {
                        using (var l = Database.Lock(acc))
                            if (Database.LockOk(l))
                            {
                                if (acc.NameChosen && acc.Credits < 1000)
                                    Write(context, "<Error>Not enough credits</Error>");
                                else
                                {
                                    if (acc.NameChosen)
                                        Database.UpdateCredit(acc, -1000);
                                    while (!Database.RenameIGN(acc, name, lockToken)) ;
                                    Write(context, "<Success />");
                                }
                            }
                            else
                                Write(context, "<Error>Account in use</Error>");
                    }
                    else
                        Write(context, "<Error>" + status.GetInfo() + "</Error>");
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
