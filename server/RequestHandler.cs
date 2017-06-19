using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Anna.Request;
using Anna.Responses;
using common;
using common.resources;
using server.account;

namespace server
{
    abstract class RequestHandler
    {
        public abstract void HandleRequest(RequestContext context, NameValueCollection query);

        public virtual void InitHandler(Resources resources) { }

        protected Database Database => Program.Database;
        
        internal void Write(RequestContext req, string val, bool zip = true)
        {
            if (zip)
            {
                var zipped = Utils.Deflate(Encoding.UTF8.GetBytes(val));
                Write(req, zipped, true);
                return;
            }

            Write(req.Response(val), "text/plain");
        }

        internal void Write(RequestContext req, byte[] val, bool zipped = false)
        {
            Write(req.Response(val), "text/plain", zipped);
        }

        internal void WriteXml(RequestContext req, string val, bool zip = true)
        {
            if (zip)
            {
                var zippedXml = Utils.Deflate(Encoding.UTF8.GetBytes(val));
                WriteXml(req, zippedXml, true);
                return;
            }

            Write(req.Response(val), "application/xml");
        }

        internal void WriteXml(RequestContext req, byte[] val, bool zipped)
        {
            Write(req.Response(val), "application/xml", zipped);
        }

        internal void WriteImg(RequestContext req, byte[] val)
        {
            Write(req.Response(val), "image/png");
        }

        internal void WriteSnd(RequestContext req, byte[] val)
        {
            Write(req.Response(val), "*/*");
        }

        internal void Write(Response r, string type, bool zipped = false)
        {
            if (zipped)
                r.Headers["Content-Encoding"] = "deflate";
            
            r.Headers["Content-Type"] = type;
            r.Send();
        }
    }

    internal static class RequestHandlers
    {
        public static void Initialize(Resources resources)
        {
            foreach (var h in Get)
                h.Value.InitHandler(resources);
            foreach (var h in Post)
                h.Value.InitHandler(resources);

            InitWebFiles(resources);
        }

        private static void InitWebFiles(Resources resources)
        {
            if (Get.ContainsKey("/"))
                throw new InvalidOperationException("Get handlers have already been initialized.");
            
            Get["/"] = new StaticFile(resources.WebFiles["/index.html"], "text/html");

            foreach (var f in resources.WebFiles)
                Get[f.Key] = new StaticFile(f.Value, MimeMapping.GetMimeMapping(f.Key));
        }

        public static readonly Dictionary<string, RequestHandler> Get = new Dictionary<string, RequestHandler>
        {
            {"/account/rp", new account.resetPassword() }
        };
         
        public static readonly Dictionary<string, RequestHandler> Post = new Dictionary<string, RequestHandler>
        {
            {"/char/list", new @char.list()},
            {"/char/delete", new @char.delete()},
            {"/char/fame", new @char.fame()},
            {"/account/register", new account.register()},
            {"/account/verify", new account.verify()},
            {"/account/forgotPassword", new account.forgotPassword()},
            {"/account/rp", new account.resetPassword() },
            {"/account/sendVerifyEmail", new account.sendVerifyEmail()},
            {"/account/changePassword", new account.changePassword()},
            {"/account/purchaseCharSlot", new account.purchaseCharSlot()},
            {"/account/setName", new account.setName()},
            {"/account/purchaseMysteryBox", new account.purchaseMysteryBox()},
            {"/account/purchasePackage", new account.purchasePackage()},
            {"/credits/getoffers", new credits.getoffers()},
            {"/credits/add", new credits.add()},
            {"/fame/list", new fame.list()},
            {"/picture/get", new picture.get()},
            {"/app/getLanguageStrings", new app.getLanguageStrings()},
            {"/app/init", new app.init()},
            {"/package/getPackages", new package.getPackages()},
            {"/app/globalNews", new app.globalNews()},
            {"/mysterybox/getBoxes", new mysterybox.getBoxes()},
            {"/account/verifyage", new account.verifyage()},
            {"/app/getServerXmls", new app.getServerXmls()},
            {"/char/purchaseClassUnlock", new @char.purchaseClassUnlock()},
            {"/account/purchaseSkin", new account.purchaseSkin()},
            {"/app/getTextures", new app.getTextures()},
            {"/guild/listMembers", new guild.listMembers()},
            {"/guild/getBoard", new guild.getBoard()},
            {"/guild/setBoard", new guild.setBoard()},
            {"/privateMessage/send", new privateMessage.send()},
            {"/privateMessage/list", new privateMessage.list()},
            {"/privateMessage/delete", new privateMessage.delete()},
            {"/account/rank", new account.rank()},
            {"/account/registerDiscord", new account.registerDiscord()},
            {"/account/unregisterDiscord", new account.unregisterDiscord()},
            {"/dailyLogin/fetchCalendar", new dailyLogin.fetchCalendar()},
            {"/inGameNews/getNews", new inGameNews.getNews()},
            {"/friends/getList", new friends.getList()},
            {"/friends/getRequests", new friends.getRequests()}
        };
    }
}
