using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;

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

        /// <summary>
        /// Endpoint que recibe los webhooks de Kommo (formato x-www-form-urlencoded).
        /// </summary>
        /// <returns>Resultado de la actualización</returns>
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> RecibirWebhook()
        {
            var form = await Request.ReadFormAsync();
            var result = await _webhookService.ProcesarDesdeFormAsync(form);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult VerificarWebhook()
        {
            return Ok("Webhook Kommo OK");
        }
    }
}
