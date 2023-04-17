using Microsoft.SharePoint.Client;
using System.Security;

namespace DechargeAPI.Classes
{
    public class SharePoint
    {
        public string siteUrl, factureADecharger,factureDechargee, context, listItems, users, testUsers, testContext, attachmentPath;
        public string? testUrl;
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
            users = siteUrl + Environment.GetEnvironmentVariable("usersEndpoint");

            testUrl = Environment.GetEnvironmentVariable("testUrl");
            attachmentPath = testUrl + Environment.GetEnvironmentVariable("userAttachment");
            testContext = testUrl + Environment.GetEnvironmentVariable("contextInfo");
            testUsers = testUrl + Environment.GetEnvironmentVariable("testUsersEndpoint");

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

            uri = new Uri(testUrl);
            handlerAuth.CookieContainer.SetCookies(uri, credentials.GetAuthenticationCookie(uri));
        }
    }
}
