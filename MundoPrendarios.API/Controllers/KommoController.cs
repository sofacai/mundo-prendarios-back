using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
            {
                return BadRequest(new { error = "Código no proporcionado" });
            }

            try
            {
                // Extraer el subdominio si se proporciona el dominio completo
                string subdomain = "api-c"; // Valor por defecto

                if (!string.IsNullOrEmpty(request.AccountDomain))
                {
                    if (request.AccountDomain.Contains(".kommo.com"))
                    {
                        subdomain = request.AccountDomain.Split('.')[0];
                    }
                    else
                    {
                        subdomain = request.AccountDomain;
                    }
                }

                // Construir la URL del endpoint de token
                string tokenUrl = "https://www.kommo.com/oauth2/access_token";

                // Si tenemos un subdominio específico, usarlo
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "www")
                {
                    tokenUrl = $"https://{subdomain}.kommo.com/oauth2/access_token";
                }

                // Construir los datos del formulario
                var formContent = new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "authorization_code",
                    ["code"] = request.Code,
                    ["redirect_uri"] = _redirectUri
                };

                // Registrar la URL y los datos para depuración
                Console.WriteLine($"Token URL: {tokenUrl}");
                Console.WriteLine($"Form Data: {JsonSerializer.Serialize(formContent)}");

                var formData = new FormUrlEncodedContent(formContent);
                var response = await _httpClient.PostAsync(tokenUrl, formData);

                // Registrar la respuesta completa para depuración
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                // Registrar la excepción para depuración
                Console.WriteLine($"Error in ExchangeCode: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token no proporcionado" });
            }

            try
            {
                // Preparar los parámetros exactamente como los espera Kommo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = request.RefreshToken
                });

                // Usar la URL correcta de OAuth 2.0
                var response = await _httpClient.PostAsync("https://kommo.com/oauth2/access_token", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Respuesta de refresh: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseBody);
                }

                return Ok(JsonSerializer.Deserialize<object>(responseBody));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("leads")]
        public async Task<IActionResult> CreateLead([FromBody] object leadsData)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "No autorizado" });
            }

            var token = authHeader.Split(' ')[1];

            try
            {
                // Crear la solicitud
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.kommo.com/api/v4/leads");

                // Agregar encabezados según la documentación
                request.Headers.Add("Authorization", $"Bearer {token}");

                // Serializar los datos del lead como un array
                var jsonData = JsonSerializer.Serialize(leadsData);
                request.Content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                // Enviar la solicitud
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}, Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }

                return Ok(JsonSerializer.Deserialize<object>(responseContent));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
}