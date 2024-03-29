﻿using DechargeAPI.Classes;
using DechargeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DechargeAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthenticateController
    {
        private readonly IConfiguration _config;

        private HttpClientHandler testHandler, mainHandler;
        private SharePoint sp;
        private byte[] salt;

        public AuthenticateController(IConfiguration configuration)
        {
            _config = configuration;
            DotNetEnv.Env.Load();

            salt = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("salt"));

            sp = new SharePoint();
            testHandler = sp.handlerAuth;
            mainHandler = sp.handler;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            JsonNode user = await GetUser(model.Username);

            var password = user[0]["MotDePasse"];

            if (!user.AsArray().IsNullOrEmpty() && VerifyPassword(model.Password, (string)password, salt))
            {
                //Console.WriteLine("OK");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"]);
                var authSigningKey = new SymmetricSecurityKey(key);
                var claims = new ClaimsIdentity(new Claim[]{
                        new Claim(JwtRegisteredClaimNames.Sub, model.Username),
                        new Claim(JwtRegisteredClaimNames.Aud, _config["Jwt:ValidAudience"]),
                        new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:ValidIssuer"])
                    });

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Audience = _config["JWT:ValidAudience"],
                    Issuer = _config["JWT:ValidIssuer"],
                    Expires = DateTime.UtcNow.AddHours(3),
                    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return new OkObjectResult(new
                {
                    status = 200,
                    token = tokenHandler.WriteToken(token),
                    expiration = token.ValidTo
                });

            }
            //Console.WriteLine("NOTOK");
            return new UnauthorizedResult();

        }
       
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<UserModel>> Register([FromBody] UserModel model, string digest)
        {
            model.MotDePasse = HashPasword(model.MotDePasse, salt);

            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient(mainHandler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);

                Console.WriteLine(sp.users);

                var response = await client.PostAsync(sp.users, data);
                response.EnsureSuccessStatusCode();

                return new CreatedResult("", "Utilisateur " + model.Login + " créé");
            }
        }
        
        [HttpGet("{username}")]
        public async Task<JsonNode> GetUser(string username)
        {
            var json = string.Empty;
            using (var client = new HttpClient(mainHandler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var url = sp.users + "?$filter=Login eq '" + username + "'";

                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonArray.Parse(json);
                
                return doc["d"]["results"];
            }
        }

        [HttpPost]
        [Route("testLogin")]
        public async Task<IActionResult> TestLogin([FromBody] LoginModel model)
        {
            JsonNode user = await TestGetUser(model.Username);

            var password = user[0]["Password"];

            if (!user.AsArray().IsNullOrEmpty() && VerifyPassword(model.Password, (string)password, salt))
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
        [Route("testRegister")]
        public async Task<ActionResult<TestUserModel>> TestRegister([FromBody] TestUserModel model, string digest)
        {
            model.Password = HashPasword(model.Password, salt);

            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient(testHandler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("X-RequestDigest", digest);

                Console.WriteLine(sp.testUsers);

                var response = await client.PostAsync(sp.testUsers, data);
                response.EnsureSuccessStatusCode();

                return new CreatedResult("", "Utilisateur " + model.Username + " créé");
            }
        }

        [HttpPost]
        [Route("testDigest")]
        public async Task<JsonElement> GetTestDigest()
        {
            var json = string.Empty;
            using (var client = new HttpClient(testHandler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                var response = await client.PostAsync(sp.testContext, null);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return root.GetProperty("d").GetProperty("GetContextWebInformation").GetProperty("FormDigestValue");
            }
        }
        

        [HttpGet("test/{username}")]
        public async Task<JsonNode> TestGetUser(string username)
        {
            var json = string.Empty;
            using (var client = new HttpClient(testHandler))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var url = sp.testUsers + "?$filter=Username eq '" + username + "'";

                var response = await client.GetAsync(url);

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
        public async Task<IActionResult> ChangePass([FromBody] TestUserModel user, string digest, int id)
        {
            user.Password = HashPasword(user.Password, salt);
            var json = JsonConvert.SerializeObject(user);;
            var uri = sp.users + "(" + id + ")";
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient(mainHandler))
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
