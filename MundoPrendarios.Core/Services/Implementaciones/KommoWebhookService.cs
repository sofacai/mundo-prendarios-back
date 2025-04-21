using Microsoft.AspNetCore.Http;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;
using System.Globalization;

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
                // Buscar ID de operación
                var operacionIdStr = "";
                foreach (var key in form.Keys)
                {
                    if (key.Contains("field_id") && form[key] == "500886")
                    {
                        string valueKey = key.Replace("field_id", "values][0][value");
                        if (form.ContainsKey(valueKey))
                        {
                            operacionIdStr = form[valueKey];
                            break;
                        }
                    }
                }

                // Backup: intentar por índice directo
                if (string.IsNullOrEmpty(operacionIdStr))
                {
                    var key = "leads[update][0][custom_fields][0][values][0][value]";
                    operacionIdStr = form.ContainsKey(key) ? form[key].ToString() : "";
                }

                if (!int.TryParse(operacionIdStr, out var operacionId))
                    return new { success = false, error = "No se encontró ID de operación válido" };

                var operacion = await _operacionRepository.GetByIdAsync(operacionId);
                if (operacion == null)
                    return new { success = false, error = "No existe la operación en base de datos" };

                // DTO para trackear cambios
                var updateDto = new OperacionWebhookUpdateDto
                {
                    OperacionId = operacionId
                };

                // Extraer campos usando valores específicos
                foreach (var key in form.Keys)
                {
                    // MontoAprobado: field_id 964766
                    if (key.Contains("field_id") && form[key] == "964766")
                    {
                        string valueKey = key.Replace("field_id", "values][0][value");
                        if (form.ContainsKey(valueKey))
                        {
                            var value = form[valueKey].ToString();
                            var monto = ParseDecimal(value);
                            operacion.MontoAprobado = monto;
                            updateDto.MontoAprobado = monto;
                        }
                    }
                    // TasaAprobada: field_id 964768
                    else if (key.Contains("field_id") && form[key] == "964768")
                    {
                        string valueKey = key.Replace("field_id", "values][0][value");
                        if (form.ContainsKey(valueKey))
                        {
                            var value = form[valueKey].ToString();
                            var tasa = ParseDecimal(value);
                            operacion.TasaAprobada = tasa;
                            updateDto.TasaAprobada = tasa;
                        }
                    }
                    // MesesAprobados: field_id 964772
                    else if (key.Contains("field_id") && form[key] == "964772")
                    {
                        string valueKey = key.Replace("field_id", "values][0][value");
                        if (form.ContainsKey(valueKey))
                        {
                            var value = form[valueKey].ToString();
                            if (int.TryParse(value, out var meses))
                            {
                                operacion.MesesAprobados = meses;
                                updateDto.MesesAprobados = meses;
                            }
                        }
                    }

                    else if (key.Contains("field_id") && form[key] == "964770")
                    {
                        string valueKey = key.Replace("field_id", "values][0][value");
                        if (form.ContainsKey(valueKey))
                        {
                            var value = form[valueKey].ToString();
                            operacion.PlanAprobadoNombre = value;
                            updateDto.PlanAprobadoNombre = value;
                        }
                    }
                }

                // Procesar tags para determinar estado
                List<string> tags = new List<string>();
                for (int i = 0; i < 5; i++)
                {
                    string tagKey = $"leads[update][0][tags][{i}][name]";
                    if (form.ContainsKey(tagKey))
                    {
                        tags.Add(form[tagKey].ToString());
                    }
                }

                // Determinar estado según tags
                string nuevoEstado = DeterminarEstadoDesdeTagsMultiples(tags);
                operacion.Estado = nuevoEstado;
                updateDto.EstadoDesdeEtiqueta = nuevoEstado;

                // Actualizar en BD
                await _operacionRepository.UpdateAsync(operacion);

                return new
                {
                    success = true,
                    operacionId,
                    cambios = updateDto,
                    tags
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private static decimal? ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 1. Intentar con cultura invariante (punto como separador decimal)
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;

            // 2. Intentar con cultura local
            if (decimal.TryParse(input, out value))
                return value;

            // 3. Intentar reemplazando punto por coma
            if (decimal.TryParse(input.Replace(".", ","), out value))
                return value;

            return null;
        }

        private static string DeterminarEstadoDesdeTagsMultiples(List<string> tags)
        {
            // Si no hay tags o hay tags ignorados (Demorado/Duplicado)
            if (tags == null || !tags.Any() || tags.Contains("Demorado") || tags.Contains("Duplicado"))
                return "EN ANALISIS";

            // Si hay más de un tag (excepto los ignorados), usar EN ANALISIS
            if (tags.Count > 1)
                return "EN ANALISIS";

            // Mapeo de tags a estados
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

            // Usar el mapeo para un solo tag
            return mapeo.TryGetValue(tags[0], out var estado) ? estado : "EN ANALISIS";
        }
    }
}