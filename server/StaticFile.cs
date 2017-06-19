using System.Collections.Specialized;
using Anna.Request;

namespace server
{
    class StaticFile : RequestHandler
    {
        private readonly string _contentType;
        private readonly byte[] _data;

        public StaticFile(byte[] data, string contentType)
        {
            _contentType = contentType;
            _data = data;
        }

        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            Write(context.Response(_data), _contentType);
        }
    }
}
