using DechargeAPI.Classes;
using DechargeAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DechargeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController
    {
        private readonly IConfiguration _config;

        private HttpClientHandler handler;
        private SharePoint sp;
        private byte[] salt;

        public AuthenticateController(IConfiguration configuration)
        {
            _config = configuration;
            DotNetEnv.Env.Load();

            salt = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("salt"));

            sp = new SharePoint();
            handler = sp.handlerAuth;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserModel model)
        {
            JsonNode user = await GetAsync(model.Username);

            if (!user.AsArray().IsNullOrEmpty() && VerifyPassword(model.Password, (string)user[0]["Password"], salt))
            {
                Console.WriteLine("OK");
                 var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _config["JWT:ValidIssuer"],
                    audience: _config["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return new OkObjectResult(new
                {
                    status = 200,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
                 
            }
            Console.WriteLine("NOTOK");
            return new UnauthorizedResult();

        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<UserModel>> Register([FromBody] UserModel model, string digest)
        {
            model.Password = HashPasword(model.Password, salt);
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);

                var response = await client.PostAsync(sp.users, data);
                response.EnsureSuccessStatusCode();

                return new CreatedResult("", "Utilisateur " + model.Username + " créé");
            }
        }

        [HttpPost]
        [Route("digest")]
        public async Task<string> GetDigest()
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.PostAsync(sp.context2, null);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").GetProperty("GetContextWebInformation").GetProperty("FormDigestValue").ToString();
            }
        }

        [HttpGet("{username}")]
        public async Task<JsonNode> GetAsync(string username)
        {
            var json = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var response = await client.GetAsync(sp.users + "?$filter=Username eq '" + username + "'");

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonArray.Parse(json);
                
                return doc["d"]["results"];
            }
        }

        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        string HashPasword(string password, byte[] salt)
        {
            //salt = RandomNumberGenerator.GetBytes(keySize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                hashAlgorithm,
                keySize);

            return Convert.ToHexString(hash);
        }

        bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);

            return hashToCompare.SequenceEqual(Convert.FromHexString(hash));
        }

        [HttpPost("changepass/{id}")]
        public async Task<IActionResult> ChangePass([FromBody] UserModel user, string digest, int id)
        {
            user.Password = HashPasword(user.Password, salt);
            var json = JsonConvert.SerializeObject(user);;
            var uri = sp.users + "(" + id + ")";
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);
                client.DefaultRequestHeaders.Add("X-HTTP-Method", "MERGE");
                client.DefaultRequestHeaders.Add("If-Match", "*");

                var response = await client.PatchAsync(uri, data);

                response.EnsureSuccessStatusCode();

                return new CreatedResult("", "Modification(s) effectuée(s)");
            }
        }
    }
}
