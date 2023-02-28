using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client;
using System.Net;
using System.Security;
using System;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DechargeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharePointController : ControllerBase
    {
        private string siteUrl;
        private string spItems;
        private SharePointOnlineCredentials credentials;
        private HttpClientHandler handler;
        private Uri uri;

        public SharePointController()
        {
            DotNetEnv.Env.Load();

            siteUrl = Environment.GetEnvironmentVariable("siteUrl");
            spItems = Environment.GetEnvironmentVariable("endpoint");
            var username = Environment.GetEnvironmentVariable("username");
            var password = Environment.GetEnvironmentVariable("password");
            
            var securePassword = new SecureString();
            password.ToCharArray().ToList().ForEach(c => securePassword.AppendChar(c));
            credentials = new SharePointOnlineCredentials(username, securePassword);

            handler = new HttpClientHandler();
            handler.Credentials = credentials;

            uri = new Uri(siteUrl);
            handler.CookieContainer.SetCookies(uri, credentials.GetAuthenticationCookie(uri));
        }

        // GET: api/<SharePointController>
        [HttpGet]
        public async Task<String> Get()
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var response = await client.GetAsync(spItems);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").GetProperty("results").ToString();
            }
        }

        // GET api/<SharePointController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SharePointController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SharePointController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SharePointController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
