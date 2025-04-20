using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TuProyecto.Controllers
{
    [Route("api/kommo")]
    [ApiController]
    public class KommoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public KommoController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("KommoApi");
            _configuration = configuration;

            _clientId = configuration["Kommo:ClientId"] ?? "c472bc29-e83d-4fe5-9550-29c7c844b060";
            _clientSecret = configuration["Kommo:ClientSecret"] ?? "qgQpgDtzYEbXng5mqEL9DPjHOjUqmZtPLJMMpht1djAm5uve2opRQsWkdhS0A3i3";
            _redirectUri = configuration["Kommo:RedirectUri"] ?? "http://localhost:8100/callback";
        }

        [HttpPost("auth")]
        public async Task<IActionResult> ExchangeCode([FromBody] AuthCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                return BadRequest(new { error = "Código no proporcionado" });

            try
            {
                string subdomain = "api-c";
                if (!string.IsNullOrEmpty(request.AccountDomain))
                {
                    if (request.AccountDomain.Contains(".kommo.com"))
                        subdomain = request.AccountDomain.Split('.')[0];
                    else
                        subdomain = request.AccountDomain;
                }

                string tokenUrl = $"https://{subdomain}.kommo.com/oauth2/access_token";

                var formContent = new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "authorization_code",
                    ["code"] = request.Code,
                    ["redirect_uri"] = _redirectUri
                };

                var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(formContent));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseContent);

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { error = "Refresh token no proporcionado" });

            try
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = request.RefreshToken
                });

                var response = await _httpClient.PostAsync("https://kommo.com/oauth2/access_token", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseBody);

                return Ok(JsonSerializer.Deserialize<object>(responseBody));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("leads")]
        public async Task<IActionResult> CreateLead([FromBody] JsonElement leadsData)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { error = "No autorizado" });

            var token = authHeader.Split(' ')[1];

            try
            {
                string rawData = leadsData.GetRawText();
                string apiUrl = "https://mundoprendario.kommo.com/api/v4/leads";

                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Headers = { { "Authorization", $"Bearer {token}" } },
                    Content = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseContent);

                return Ok(JsonSerializer.Deserialize<object>(responseContent));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> CreateContact([FromBody] JsonElement contactoPayload)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { error = "No autorizado" });

            var token = authHeader.Split(' ')[1];

            try
            {
                string rawJson = contactoPayload.GetRawText();

                var request = new HttpRequestMessage(HttpMethod.Post, "https://mundoprendario.kommo.com/api/v4/contacts")
                {
                    Headers = { { "Authorization", $"Bearer {token}" } },
                    Content = new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseContent);

                return Ok(JsonSerializer.Deserialize<object>(responseContent));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] JsonElement companyData)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { error = "No autorizado" });

            var token = authHeader.Split(' ')[1];

            try
            {
                string rawJson = companyData.GetRawText();
                string apiUrl = "https://mundoprendario.kommo.com/api/v4/companies";

                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Headers = { { "Authorization", $"Bearer {token}" } },
                    Content = new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseContent);

                return Ok(JsonSerializer.Deserialize<object>(responseContent));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost("contacts/{contactId}/link")]
        public async Task<IActionResult> LinkContactToLead(int contactId, [FromBody] LinkRequest request)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { error = "No autorizado" });

            var token = authHeader.Split(' ')[1];

            try
            {
                var apiUrl = $"https://mundoprendario.kommo.com/api/v4/contacts/{contactId}/link";
                var jsonContent = JsonSerializer.Serialize(request);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Headers = { { "Authorization", $"Bearer {token}" } },
                    Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(httpRequest);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, responseBody);

                return Ok(JsonSerializer.Deserialize<object>(responseBody));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class AuthCodeRequest
        {
            public string Code { get; set; }
            public string AccountDomain { get; set; }
            public string ClientId { get; set; }
            public string State { get; set; }
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }

        public class LinkRequest
        {
            [JsonPropertyName("to")]
            public List<LinkToEntity> To { get; set; }
        }

        public class LinkToEntity
        {
            [JsonPropertyName("to_entity_id")]
            public int ToEntityId { get; set; }

            [JsonPropertyName("to_entity_type")]
            public string ToEntityType { get; set; } = "leads";
        }
    }
}
