using System.Collections.Specialized;
using Anna.Request;

namespace server.@char
{
    class fame : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            var character = Database.LoadCharacter(int.Parse(query["accountId"]), int.Parse(query["charId"]));
            if (character == null)
            {
                Write(context, "<Error>Invalid character</Error>");
                return;
            }

            var fame = Fame.FromDb(character);
            if (fame == null)
            {
                Write(context, "<Error>Character not dead</Error>");
                return;
            }
            Write(context, fame.ToXml().ToString());
        }
    }
}
