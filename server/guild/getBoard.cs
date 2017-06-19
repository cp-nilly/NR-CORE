using System.Collections.Specialized;
using Anna.Request;
using common;

namespace server.guild
{
    class getBoard : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            DbAccount acc;
            var status = Database.Verify(query["guid"], query["password"], out acc);
            if (status == LoginStatus.OK)
            {
                if (acc.GuildId <= 0)
                {
                    Write(context, "<Error>Not in guild</Error>");
                    return;
                }

                var guild = Database.GetGuild(acc.GuildId);
                Write(context, guild.Board);
            }
            else
                Write(context, "<Error>" + status.GetInfo() + "</Error>");
        }
    }
}
