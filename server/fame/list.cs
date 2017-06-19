using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.fame
{
    class list : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbChar character = null;
            if (query["accountId"] != null)
            {
                character = Database.LoadCharacter(
                    int.Parse(query["accountId"]),
                    int.Parse(query["charId"])
                );
            }
            var list = FameList.FromDb(Database, query["timespan"], character);
            Write(context, list.ToXml().ToString());
        }
    }
}
