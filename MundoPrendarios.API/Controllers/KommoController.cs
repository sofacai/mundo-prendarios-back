using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
        private readonly string _tokenUrl = "https://api-c.kommo.com/oauth2/access_token";
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

            try
            {
                var content = JsonContent.Create(new
                {
                    client_id = _clientId,
                    client_secret = _clientSecret,
                    grant_type = "authorization_code",
                    code = request.Code,
                    redirect_uri = _redirectUri
                });

                var response = await _httpClient.PostAsync(_tokenUrl, content);

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

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token no proporcionado" });
            }

            try
            {
                var content = JsonContent.Create(new
                {
                    client_id = _clientId,
                    client_secret = _clientSecret,
                    grant_type = "refresh_token",
                    refresh_token = request.RefreshToken
                });

                var response = await _httpClient.PostAsync(_tokenUrl, content);

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

        [HttpGet("leads")]
        public async Task<IActionResult> GetLeads()
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

                var response = await _httpClient.GetAsync($"{_apiBase}/leads");

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
    }

    public class AuthCodeRequest
    {
        public string Code { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}