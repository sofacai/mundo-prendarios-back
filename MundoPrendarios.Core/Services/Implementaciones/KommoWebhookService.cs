using Microsoft.AspNetCore.Http;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class WebhookKommoService : IKommoWebhookService
    {
        private readonly IOperacionRepository _operacionRepository;

        public WebhookKommoService(IOperacionRepository operacionRepository)
        {
            _operacionRepository = operacionRepository;
        }

        public async Task<object> ProcesarDesdeFormAsync(IFormCollection form)
        {
            try
            {
                // Buscar lead[update][0][custom_fields][]
                string Get(string key) => form.FirstOrDefault(x => x.Key == key).Value.ToString();

                var operacionIdStr = Get("leads[update][0][custom_fields][0][values][0][value]");
                if (!int.TryParse(operacionIdStr, out var operacionId))
                    return new { success = false, error = "No se encontró ID de operación válido" };

                var operacion = await _operacionRepository.GetByIdAsync(operacionId);
                if (operacion == null)
                    return new { success = false, error = "No existe la operación en base de datos" };

                // Extraer los campos relevantes
                operacion.MontoAprobado = TryGetDecimal(Get("leads[update][0][custom_fields][5][values][0][value]"));
                operacion.TasaAprobada = TryGetDecimal(Get("leads[update][0][custom_fields][6][values][0][value]"));
                operacion.MesesAprobados = TryGetInt(Get("leads[update][0][custom_fields][7][values][0][value]"));
                operacion.PlanAprobadoNombre = Get("leads[update][0][custom_fields][3][values][0][value]");

                var tagName = Get("leads[update][0][tags][0][name]");
                operacion.Estado = DeterminarEstadoDesdeTag(tagName);

                await _operacionRepository.UpdateAsync(operacion);

                return new
                {
                    success = true,
                    operacionId,
                    cambios = new
                    {
                        operacion.MontoAprobado,
                        operacion.TasaAprobada,
                        operacion.MesesAprobados,
                        operacion.PlanAprobadoNombre,
                        operacion.Estado
                    }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private static decimal? TryGetDecimal(string input)
            => decimal.TryParse(input, out var val) ? val : null;

        private static int? TryGetInt(string input)
            => int.TryParse(input, out var val) ? val : null;

        private static string DeterminarEstadoDesdeTag(string tag)
        {
            if (tag == "Demorado") return "EN ANALISIS";

            var mapeo = new Dictionary<string, string>
    {
        { "Aprobado definitivo", "APROBADO DEFINIT" },
        { "Pasa a análisis", "EN ANALISIS" },
        { "Aprobado Provisorio", "EN ANALISIS" },
        { "Completar documentación", "COMPLETANDO DOCU" },
        { "Firmar documentación", "FIRMAS DOCU" },
        { "Incripción de prenda propio", "LIQUIDADA" },
        { "Incripción prenda canal", "LIQUIDADA" },
        { "Envío a liquidar", "LIQUIDADA" },
        { "Rechazado BCRA", "RECHAZADO" },
        { "Rechazado Banco", "RECHAZADO" },
    };

            return mapeo.TryGetValue(tag, out var estado) ? estado : "EN ANALISIS";
        }


    }
}
