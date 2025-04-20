using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Controllers
{
    [ApiController]
    [Route("webhook/kommo")]
    public class KommoWebhookController : ControllerBase
    {
        private readonly IKommoWebhookService _webhookService;

        public KommoWebhookController(IKommoWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> RecibirWebhook()
        {
            var form = await Request.ReadFormAsync();
            var result = await _webhookService.ProcesarDesdeFormAsync(form);
            return Ok(result);
        }
    }
}
