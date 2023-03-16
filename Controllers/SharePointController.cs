using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client;
using System.Net;
using System.Security;
using System;
using System.Text.Json;
using DechargeAPI.Classes;
using System.Web.Helpers;

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
        [HttpGet]
        public async Task<String> Get()
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

        // GET: api/SharePointController2
        [Route("/api/[controller]/2")]
        [HttpGet]
        public async Task<String> Get2()
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

        [Route("/api/digest")]
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

        // GET api/<SharePointController>/5
        [HttpGet("{id}")]
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
