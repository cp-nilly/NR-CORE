using System.Collections.Specialized;
using System.IO;
using Anna.Request;
using common;
using common.resources;

namespace server.mysterybox
{
    class getBoxes : RequestHandler
    {
        private static byte[] _data;

        public override void InitHandler(Resources resources)
        {
            _data = Utils.Deflate(File.ReadAllBytes($"{resources.ResourcePath}/data/mysteryBoxes.xml"));
        }

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            Write(context.Response(_data), "application/xml", true);
        }
    }
}
