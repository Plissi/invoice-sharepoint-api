using DechargeAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DechargeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [Route("GetFactureADecharger")]
        [HttpGet]
        public async Task<String> GetFactureADecharger()
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                
                var response = await client.GetAsync(sp.factureADecharger);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").ToString(); ;
            }
        }

        [Route("GetFactureDechargee")]
        [HttpGet]
        public async Task<String> GetFactureDechargee()
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.GetAsync(sp.factureDechargee);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").ToString();
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

        [HttpGet("GetById/{id}")]
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

                return root.GetProperty("d");
            }
        }

        [HttpGet("GetByCodeClient/{code}")]
        public async Task<JsonNode> GetAsync(string code)
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
                var doc = JsonArray.Parse(json);

                return doc["d"];
            }
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
