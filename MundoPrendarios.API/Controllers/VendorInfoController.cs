using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;
using System.Text.Json;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VendorInfoController : ControllerBase
    {
        private readonly ICanalService _canalService;
        private readonly ISubcanalService _subcanalService;
        private readonly IPlanCanalService _planCanalService;
        private readonly ICurrentUserService _currentUserService;

        public VendorInfoController(
            ICanalService canalService,
            ISubcanalService subcanalService,
            IPlanCanalService planCanalService,
            ICurrentUserService currentUserService)
        {
            _canalService = canalService;
            _subcanalService = subcanalService;
            _planCanalService = planCanalService;
            _currentUserService = currentUserService;
        }

        // GET: api/VendorInfo/wizard-data
        [HttpGet("wizard-data")]
        public async Task<ActionResult> GetVendorWizardData()
        {
            try
            {
                int usuarioId = _currentUserService.GetUserId();

                // Solo permitir para vendors y admincanal
                if (!_currentUserService.IsVendor() && !_currentUserService.IsAdminCanal())
                {
                    return StatusCode(403, new { mensaje = "No tienes permisos para acceder a esta información." });
                }

                // Obtener subcanales relacionados con el usuario
                var subcanales = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);
                if (subcanales == null || !subcanales.Any())
                {
                    return NotFound(new { mensaje = "No se encontraron subcanales asignados a este usuario." });
                }

                // Preparar lista de información de subcanales
                var subcanalInfoList = new List<object>();

                foreach (var subcanal in subcanales)
                {
                    int subcanalId = subcanal.Id;
                    int canalId = subcanal.CanalId;

                    // Obtener gastos del subcanal
                    var gastosInfo = new List<object>();
                    if (subcanal.Gastos != null && subcanal.Gastos.Any())
                    {
                        gastosInfo = subcanal.Gastos.Select(g => new
                        {
                            id = g.Id,
                            nombre = g.Nombre,
                            porcentaje = g.Porcentaje
                        }).ToList<object>();
                    }

                    // Obtener planes disponibles para el canal
                    var planesCanal = await _planCanalService.ObtenerPlanesPorCanalAsync(canalId);
                    var planesDisponibles = planesCanal
                        .Where(pc => pc.Activo)
                        .Select(pc => new
                        {
                            id = pc.PlanId,
                            nombre = pc.Plan != null ? pc.Plan.Nombre : "",
                            tasa = pc.Plan != null ? pc.Plan.Tasa : 0,
                            montoMinimo = pc.Plan != null ? pc.Plan.MontoMinimo : 0,
                            montoMaximo = pc.Plan != null ? pc.Plan.MontoMaximo : 0,
                            cuotasAplicables = pc.Plan != null && !string.IsNullOrEmpty(pc.Plan.CuotasAplicables)
                                ? pc.Plan.CuotasAplicables.Split(',').Select(c => int.TryParse(c, out int cuota) ? cuota : 0).Where(c => c > 0).ToList()
                                : new List<int>()
                        }).ToList();

                    // Crear objeto para este subcanal
                    subcanalInfoList.Add(new
                    {
                        subcanalId,
                        subcanalNombre = subcanal.Nombre,
                        subcanalActivo = subcanal.Activo,
                        canalId,
                        gastos = gastosInfo,
                        planesDisponibles
                    });
                }

                return Ok(new { subcanales = subcanalInfoList });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetVendorWizardData: {ex}");
                return StatusCode(500, new { mensaje = "Error al procesar la solicitud. " + ex.Message });
            }
        }
    }
}