using System.IO;

namespace common.resources
{
    public class ChangePassword
    {
        private readonly string _requestEmail;
        private readonly string _resetEmail;
        private readonly string _resetHtml;
        private readonly string _resetErrorHtml;

        private const string ResetLink = "{RESETLINK}";
        private const string Password = "{PASSWORD}";

        public ChangePassword(string dir)
        {
            var basePath = Path.Combine(Utils.GetAssemblyDirectory(), dir);

            _requestEmail = File.ReadAllText(Path.Combine(basePath, "request.txt"));
            _resetEmail = File.ReadAllText(Path.Combine(basePath, "reset.txt"));
            _resetHtml = File.ReadAllText(Path.Combine(basePath, "reset.html"));
            _resetErrorHtml = File.ReadAllText(Path.Combine(basePath, "resetError.html"));
        }

        public string GetRequestEmail(string link)
        {
            return _requestEmail.Replace(ResetLink, link);
        }

        public string GetResetEmail(string pass)
        {
            return _resetEmail.Replace(Password, pass);
        }

        public string GetResetHtml(string pass)
        {
            return _resetHtml.Replace(Password, pass);
        }

        public string GetResetErrorHtml()
        {
            return _resetErrorHtml;
        }
    }
}
