using System;
using System.Collections.Specialized;
using Anna.Request;

namespace server.picture
{
    class get : RequestHandler
    {
        public override void HandleRequest(RequestContext context, NameValueCollection query)
        {
            // format id...
            string id;
            try
            {
                var texId = query["id"].Split(':');
                
                if (texId.Length == 1)
                    id = texId[0];
                else if (texId.Length == 2)
                    id = texId[1];
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                context.Respond("<Error>Invalid input</Error>");
                return;
            }

            // check to see if texture is already loaded first...
            var textures = Program.Resources.Textures;
            if (textures.ContainsKey(id))
            {
                WriteImg(context, textures[id]);
                return;
            }

            context.Respond(404);

            // try to fetch texture from the internet 
            // *for testing only. should be disable for production.*
            /*byte[] img;
            try
            {
                var wLoc = "http://realmofthemadgod.appspot.com/picture/get?id=" + id;
                img = new WebClient().DownloadData(wLoc);
            }
            catch (Exception e)
            {
                try
                {
                    var wLoc = "http://rotmgtesting.appspot.com/picture/get?id=" + id;
                    img = new WebClient().DownloadData(wLoc);
                }
                catch (Exception ee)
                {
                    //Write(context, "<Error>Image not found</Error>");
                    context.Respond(404);
                    return;
                }
            }
            string fLoc = "textures/_" + id + ".png";
            File.WriteAllBytes(fLoc, img);
            Program.Resources.AddTexture(id, img);
            WriteImg(context, img);*/
        }
    }
}
