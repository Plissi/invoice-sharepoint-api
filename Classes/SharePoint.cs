using Microsoft.SharePoint.Client;
using System.Security;

namespace DechargeAPI.Classes
{
    public class SharePoint
    {
        public string siteUrl, factureADecharger,factureDechargee, context, listItems, users, context2;
        public string? site2;
        public SharePointOnlineCredentials credentials;
        public HttpClientHandler handler, handlerAuth;
        public Uri uri;

        public SharePoint()
        {
            DotNetEnv.Env.Load();

            siteUrl = Environment.GetEnvironmentVariable("siteUrl");
            factureADecharger = siteUrl + Environment.GetEnvironmentVariable("endpointFactureADecharger");
            factureDechargee = siteUrl + Environment.GetEnvironmentVariable("endpointFactureDechargee");
            context = siteUrl + Environment.GetEnvironmentVariable("contextInfo");
            listItems = siteUrl + Environment.GetEnvironmentVariable("listItems");

            site2 = Environment.GetEnvironmentVariable("site2");
            users = site2 + Environment.GetEnvironmentVariable("users");
            context2 = site2 + Environment.GetEnvironmentVariable("contextInfo");

            var username = Environment.GetEnvironmentVariable("username");
            var password = Environment.GetEnvironmentVariable("password");

            var securePassword = new SecureString();
            password.ToCharArray().ToList().ForEach(c => securePassword.AppendChar(c));
            credentials = new SharePointOnlineCredentials(username, securePassword);

            handler = new HttpClientHandler();
            handler.Credentials = credentials;

            uri = new Uri(siteUrl);
            handler.CookieContainer.SetCookies(uri, credentials.GetAuthenticationCookie(uri));

            handlerAuth = new HttpClientHandler();
            handlerAuth.Credentials = credentials;

            uri = new Uri(site2);
            handlerAuth.CookieContainer.SetCookies(uri, credentials.GetAuthenticationCookie(uri));
        }
    }
}
