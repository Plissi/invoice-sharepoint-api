using DechargeAPI.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DechargeAPI.Controllers
{
    [Route("api")]
    [ApiController]
    //[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
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
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.GetAsync(url);

                Console.WriteLine(sp.factureADecharger);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
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

        /*
         [Route("PageSuivante")]
        [HttpGet]
        public async Task<IActionResult> PageSuivante(string? nextUrl)
        {
            if (nextUrl == null)
            {
                nextUrl = sp.factureADecharger;
            }
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.GetAsync(nextUrl);
                var content = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return Ok(content);
            }
        }
         */

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
            var uri = sp.users + "(" + id + ")/AttachmentFiles/ add(FileName='" + imageFile.FileName + "')";

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);
                Console.WriteLine(imageFile.Name);

                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    var fileStream = new StreamContent(imageFile.OpenReadStream());
                    //Add the file
                    multipartFormContent.Add(fileStream, name: "DEPOT SCAN", fileName: imageFile.FileName);

                    //Send it
                    var response = await client.PostAsync(uri, multipartFormContent);
                    response.EnsureSuccessStatusCode();
                    return new CreatedResult("", "Décharge ajoutée");
                }
            }
        }

        /*
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
        */
    }
}
