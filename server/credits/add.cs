using System.Collections.Specialized;
using Anna.Request;

namespace server.credits
{
    class add : RequestHandler
    {
        //place holder
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            /*var query = getQuery(context);

            if (query["guid"] != null || query["jwt"] != null)
            {
                string ret;
                DbAccount acc = Database.GetAccount(query["guid"]);
                if (acc != null)
                {
                    int amount = int.Parse(query["jwt"]);
                    Database.UpdateCredit(acc, amount);
                    ret = "Done ya!";
                }
                else
                    ret = "Account not exists!";

                context.Respond(@"<html>" +
                                @"<head>" +
                                @"<title>Ya...</title>" +
                                @"</head>" +
                                @"<body style='background: #333333'>" +
                                @"<h1 style='color: #EEEEEE; text-align: center'>" + 
                                @"" + ret + @"" +
                                @"</h1>" +
                                @"</body>" +
                                @"</html>");
                return;
            }*/
            Write(context, "<Error>Nope</Error>");
        }
    }
}