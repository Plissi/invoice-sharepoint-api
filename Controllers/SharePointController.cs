using DechargeAPI.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DechargeAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class SharePointController : ControllerBase
    {
        private HttpClientHandler handler;
        private SharePoint sp;

        public SharePointController()
        {
            sp = new SharePoint();
            handler = sp.handler;
        }

        // GET: api/<SharePointController>
        [Route("FacturesADecharger")]
        [HttpGet]
        public async Task<JsonElement> FacturesADecharger(string? url)
        {
            if (url == null)
            {
                url = sp.factureADecharger;
            }
            Console.WriteLine(url);
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.GetAsync(url);

                Console.WriteLine(sp.factureADecharger);
                response.EnsureSuccessStatusCode();

                string? json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root;
            }
        }

        [Route("FacturesDechargee")]
        [HttpGet]
        public async Task<JsonElement> FacturesDechargee(string? url)
        {
            if (url == null)
            {
                url = sp.factureDechargee;
            }
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root;
            }
        }

        [Route("digest")]
        [HttpPost]
        public async Task<JsonElement> GetDigest()
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.PostAsync(sp.context, null);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").GetProperty("GetContextWebInformation").GetProperty("FormDigestValue");
            }
        }

        [HttpGet("FactureParId/{id}")]
        public async Task<JsonElement> GetAsync(int id)
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var response = await client.GetAsync(sp.listItems + "(" + id + ")");

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root;
            }
        }

        [HttpGet("FactureParCodeClient/{code}")]
        public async Task<JsonElement> GetAsync(string code)
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                Console.WriteLine(sp.listItems + "?$filter=startswith(CodeClient,'" + code + "')");
                var response = await client.GetAsync(sp.listItems + "?$filter=startswith(CodeClient,'" + code + "')&$orderby=Created%20desc");

                //response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root;
            }
        }
        [HttpPost("UploadImage/{id}")]
        public async Task<IActionResult> UploadImage(string digest, int id, IFormFile imageFile)
        {
            var uri = sp.listItems + "(" + id + ")/AttachmentFiles/add(FileName='" + imageFile.FileName + "')";

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);

                var fileStream = new StreamContent(imageFile.OpenReadStream());

                //Add the file
                fileStream.Headers.ContentType = MediaTypeHeaderValue.Parse(imageFile.ContentType);

                //Send it
                var response = await client.PostAsync(uri, fileStream);
                response.EnsureSuccessStatusCode();
                return new CreatedResult("", "Décharge ajoutée");
            }
        }
    }
}
