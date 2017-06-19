using System;
using System.Collections.Specialized;
using Anna.Request;
using log4net;
using SendGrid.Helpers.Mail;

namespace server.account
{
    class resetPassword : RequestHandler
    {
        private static readonly ILog PassLog = LogManager.GetLogger("PassLog");

        private static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = "";
            var rnd = new Random();
            while (0 < length--)
                res += valid[rnd.Next(valid.Length)];
            return res;
        }

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            var acc = Database.GetAccount(Convert.ToInt32(query["a"]));
            var passToken = query["b"];
                
            // verify
            if (String.IsNullOrEmpty(passToken) || !acc.PassResetToken.Equals(passToken))
            {
                context.Respond(Program.Resources.ChangePass.GetResetErrorHtml());
                return;
            }

            // change password
            var password = CreatePassword(new Random().Next(8, 12));
            Database.ChangePassword(acc.UUID, password);

            // reset token
            acc.PassResetToken = "";
            acc.FlushAsync();

            // send email with pass
            var apikey = Program.Config.serverSettings.sendGridApiKey;
            var sg = new SendGrid.SendGridAPIClient(apikey);
            var from = new Email("noreply@nillysrealm.com", "Nilly's Realm");
            var subject = "New Password on Nilly's Realm";
            var to = new Email(acc.UUID);
            var content = new Content("text/plain", Program.Resources.ChangePass.GetResetEmail(password));
            var mail = new Mail(from, subject, to, content);
            sg.client.mail.send.post(requestBody: mail.Get());

            // return html page
            context.Respond(Program.Resources.ChangePass.GetResetHtml(password));
            PassLog.Info($"Password reset. IP: {context.Request.ClientIP()}, Account: {acc.Name} ({acc.AccountId})");
        }
    }
}

