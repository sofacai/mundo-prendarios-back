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
    [Route("api/[controller]")]
    [ApiController]
    public class KommoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _tokenUrl = "https://www.kommo.com/oauth2/access_token";
        private readonly string _apiBase = "https://api-c.kommo.com/api/v4";

        public KommoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Obtener configuración desde appsettings.json
            // o usar los valores directamente como hemos especificado
            _clientId = "c472bc29-e83d-4fe5-9550-29c7c844b060";
            _clientSecret = "qgQpgDtzYEbXng5mqEL9DPjHOjUqmZtPLJMMpht1djAm5uve2opRQsWkdhS0A3i3";
            _redirectUri = "http://localhost:8100/callback"; // Cambiar a tu URL real
        }

        [HttpPost("auth")]
        public async Task<IActionResult> ExchangeCode([FromBody] AuthCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { error = "Código no proporcionado" });
            }

            if (string.IsNullOrEmpty(request.AccountDomain))
            {
                return BadRequest(new { error = "Account domain no proporcionado" });
            }

            try
            {
                // Extraer el subdominio si se proporciona el dominio completo
                string subdomain = request.AccountDomain;
                if (subdomain.Contains(".kommo.com"))
                {
                    subdomain = subdomain.Split('.')[0];
                }

                // Usar el subdominio específico para la URL del token
                string tokenUrl = $"https://{subdomain}.kommo.com/oauth2/access_token";

                // Build form data
                var formContent = new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "authorization_code",
                    ["code"] = request.Code,
                    ["redirect_uri"] = _redirectUri
                };

                var formData = new FormUrlEncodedContent(formContent);
                var response = await _httpClient.PostAsync(tokenUrl, formData);

                // Log the full response for debugging
                var responseContent = await response.Content.ReadAsStringAsync();

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
                var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = request.RefreshToken
                });

                var response = await _httpClient.PostAsync(_tokenUrl, formData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("leads")]
        public async Task<IActionResult> CreateLead([FromBody] KommoLead lead)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "No autorizado" });
            }

            var token = authHeader.Split(' ')[1];

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Obtener el subdominio del usuario autenticado
                var accountInfoResponse = await _httpClient.GetAsync($"{_apiBase}/account");

                if (!accountInfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await accountInfoResponse.Content.ReadAsStringAsync();
                    return StatusCode((int)accountInfoResponse.StatusCode, errorContent);
                }

                var accountInfo = await accountInfoResponse.Content.ReadFromJsonAsync<AccountInfoResponse>();
                string subdomain = accountInfo.Subdomain;

                // Usar el subdominio específico para la API
                string leadsApiUrl = $"https://{subdomain}.kommo.com/api/v4/leads";

                var response = await _httpClient.PostAsJsonAsync(leadsApiUrl, lead);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement data)
        {
            try
            {
                // Guardar en logs la información recibida
                string jsonData = data.ToString();
                Console.WriteLine("Webhook recibido: " + jsonData);

                // Validar que sea un webhook de Kommo (opcional - añadir encabezados de verificación)
                // var signature = Request.Headers["X-Kommo-Signature"].ToString();
                // if (string.IsNullOrEmpty(signature)) return Unauthorized();

                // Aquí puedes procesar la información según el tipo de evento
                // Por ejemplo, actualizar el estado de la operación si un lead cambia de estado

                return Ok(new { success = true, message = "Webhook recibido correctamente" });
            }
            catch (Exception ex)
            {
                // Registrar error pero devolver éxito para que Kommo no reintente
                Console.WriteLine("Error procesando webhook: " + ex.Message);
                return Ok(new { success = false, message = "Error procesando webhook" });
            }
        }

        [HttpGet("webhook/setup")]
        public async Task<IActionResult> SetupWebhook()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Unauthorized(new { error = "Autorización requerida" });
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "Token no válido" });
            }

            var token = authHeader.Split(' ')[1];

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Obtener el subdominio
                var accountInfoResponse = await _httpClient.GetAsync($"{_apiBase}/account");
                if (!accountInfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await accountInfoResponse.Content.ReadAsStringAsync();
                    return StatusCode((int)accountInfoResponse.StatusCode, errorContent);
                }

                var accountInfo = await accountInfoResponse.Content.ReadFromJsonAsync<AccountInfoResponse>();
                string subdomain = accountInfo.Subdomain;

                // URL del webhook (debe ser accesible públicamente)
                string webhookUrl = _configuration["Kommo:WebhookUrl"] ??
                                   "https://tu-aplicacion.com/api/kommo/webhook";

                // Crear o actualizar la suscripción al webhook
                var webhookData = new
                {
                    destination = webhookUrl,
                    settings = new
                    {
                        leads_statuses = new[] { true },
                        contacts_relations = new[] { true },
                        leads_pipelines = new[] { true }
                    },
                    sort = 1,
                    is_active = true
                };

                string webhookApiUrl = $"https://{subdomain}.kommo.com/api/v4/webhooks";
                var response = await _httpClient.PostAsJsonAsync(webhookApiUrl, webhookData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(new { success = true, message = "Webhook configurado correctamente", data = content });
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

    public class KommoLead
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Status_id { get; set; }
        public int Pipeline_id { get; set; }
        public List<CustomFieldValue> Custom_fields_values { get; set; }
        public EmbeddedEntities _Embedded { get; set; }
    }

    public class CustomFieldValue
    {
        public int Field_id { get; set; }
        public List<ValueContainer> Values { get; set; }
    }

    public class ValueContainer
    {
        public object Value { get; set; }
    }

    public class EmbeddedEntities
    {
        public List<KommoContact> Contacts { get; set; }
    }

    public class KommoContact
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public List<CustomFieldValue> Custom_fields_values { get; set; }
    }

    public class AccountInfoResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
    }
}