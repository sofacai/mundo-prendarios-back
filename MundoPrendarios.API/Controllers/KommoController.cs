using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MundoPrendarios.Controllers
{
    [Route("api/kommo")]
    [ApiController]
    public class KommoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _kommoToken;

        public KommoController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _kommoToken = configuration["Kommo:Token"];
        }

        [HttpPost("leads")]
        public async Task<IActionResult> CreateLead([FromBody] JsonElement leadsData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://mundoprendario.kommo.com/api/v4/leads")
            {
                Headers = { { "Authorization", $"Bearer {_kommoToken}" } },
                Content = new StringContent(leadsData.GetRawText(), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseContent);

            return Ok(JsonSerializer.Deserialize<object>(responseContent));
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> CreateContact([FromBody] JsonElement contactoPayload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://mundoprendario.kommo.com/api/v4/contacts")
            {
                Headers = { { "Authorization", $"Bearer {_kommoToken}" } },
                Content = new StringContent(contactoPayload.GetRawText(), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseContent);

            return Ok(JsonSerializer.Deserialize<object>(responseContent));
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] JsonElement companyData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://mundoprendario.kommo.com/api/v4/companies")
            {
                Headers = { { "Authorization", $"Bearer {_kommoToken}" } },
                Content = new StringContent(companyData.GetRawText(), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseContent);

            return Ok(JsonSerializer.Deserialize<object>(responseContent));
        }

        [HttpPost("contacts/{contactId}/link")]
        public async Task<IActionResult> LinkContactToLead(int contactId, [FromBody] LinkRequest requestData)
        {
            var apiUrl = $"https://mundoprendario.kommo.com/api/v4/contacts/{contactId}/link";
            var requestBody = JsonSerializer.Serialize(requestData);

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Headers = { { "Authorization", $"Bearer {_kommoToken}" } },
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseContent);

            return Ok(JsonSerializer.Deserialize<object>(responseContent));
        }

        // DTOs internos
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
