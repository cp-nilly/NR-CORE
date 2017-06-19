using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using Anna.Request;
using common;

namespace server.privateMessage
{
    class send : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            var recipient = HttpUtility.UrlDecode(query["recipient"]);
            var subject = HttpUtility.UrlDecode(query["subject"]);
            var message = HttpUtility.UrlDecode(query["message"]);

            DbAccount acc;
            LoginStatus s;
            if ((s = Database.Verify(query["guid"], query["password"], out acc)) == LoginStatus.OK)
            {
                if (string.Equals(acc.Name, recipient, StringComparison.InvariantCultureIgnoreCase))
                {
                    Write(context, "<Error>Stop sending yourself messages.</Error>");
                    return;
                }

                var targetAccount = Database.GetAccount(Database.ResolveId(recipient));
                if (targetAccount == null)
                {
                    Write(context, "<Error>Recipient not found. This account does not exist or its an unnamed account, make sure you wrote the name correctly.</Error>");
                    return;
                }

                targetAccount.AddPrivateMessage(acc.AccountId, subject, message)
                    .ContinueWith(t =>
                    {
                        Program.ISManager.Publish(Channel.Control, new ControlMsg
                        {
                            Type = ControlType.PrivateMessageRefresh,
                            Payload = recipient
                        });
                        Write(context, "Your message has been sent successfully.");
                    })
                    .ContinueWith(e =>
                    {
                        Program.Log.Error(e.Exception.InnerExceptions);
                        Write(context, "<Error>Internal server error</Error>");
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else
                Write(context, $"<Error>{s.GetInfo()}</Error>");
        }
    }
}
