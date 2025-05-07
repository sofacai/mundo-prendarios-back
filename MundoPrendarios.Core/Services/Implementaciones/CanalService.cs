using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class CanalService : ICanalService
    {
        private readonly ICanalRepository _canalRepository;
        private readonly ICanalOficialComercialRepository _canalOficialComercialRepository;
        private readonly ICanalOficialComercialService _canalOficialComercialService;
        private readonly IMapper _mapper;

        public CanalService(
            ICanalRepository canalRepository,
            ICanalOficialComercialRepository canalOficialComercialRepository,
            ICanalOficialComercialService canalOficialComercialService,
            IMapper mapper)
        {
            _canalRepository = canalRepository;
            _canalOficialComercialRepository = canalOficialComercialRepository;
            _canalOficialComercialService = canalOficialComercialService;
            _mapper = mapper;
        }

        public async Task<CanalDto> CrearCanalAsync(CanalCrearDto canalDto)
        {
            // Crear entidad Canal a partir del DTO
            var canal = new Canal
            {
                NombreFantasia = canalDto.NombreFantasia,
                RazonSocial = canalDto.RazonSocial,
                Provincia = canalDto.Provincia,
                Localidad = canalDto.Localidad,
                Direccion = canalDto.Direccion,
                Cuit = canalDto.Cuit,
                CBU = canalDto.CBU,
                Alias = canalDto.Alias,
                Banco = canalDto.Banco,
                NumCuenta = canalDto.NumCuenta,
                TipoCanal = canalDto.TipoCanal,
                OpcionesCobro = canalDto.OpcionesCobro,
                Foto = canalDto.Foto,
                FechaAlta = DateTime.Now,
                TitularNombreCompleto = canalDto.TitularNombreCompleto,
                TitularTelefono = canalDto.TitularTelefono,
                TitularEmail = canalDto.TitularEmail,
                Activo = true
            };

            await _canalRepository.AddAsync(canal);

            // Asignar Oficiales Comerciales al canal si se proporcionaron
            var oficialesComerciales = new List<OficialComercialResumenDto>();
            if (canalDto.OficialesComerciales != null && canalDto.OficialesComerciales.Any())
            {
                foreach (var ocId in canalDto.OficialesComerciales)
                {
                    try
                    {
                        var dto = new CanalOficialComercialCrearDto
                        {
                            CanalId = canal.Id,
                            OficialComercialId = ocId
                        };

                        var asignacion = await _canalOficialComercialService.AsignarOficialComercialACanalAsync(dto);

                        // Agregar a la lista para la respuesta
                        if (asignacion != null)
                        {
                            oficialesComerciales.Add(new OficialComercialResumenDto
                            {
                                Id = asignacion.OficialComercialId,
                                Nombre = asignacion.OficialComercialNombre.Split(' ')[0], // Aproximación simple
                                Apellido = asignacion.OficialComercialNombre.Contains(' ') ?
                                           asignacion.OficialComercialNombre.Substring(asignacion.OficialComercialNombre.IndexOf(' ') + 1) : "",
                                FechaAsignacion = asignacion.FechaAsignacion
                            });
                        }
                    }
                    catch
                    {
                        // Continuar con el siguiente OC si hay error en uno
                        continue;
                    }
                }
            }

            // Mapear la entidad al DTO de respuesta
            var canalDtoResult = _mapper.Map<Canal, CanalDto>(canal);
            canalDtoResult.OficialesComerciales = oficialesComerciales;

            return canalDtoResult;
        }

        public async Task<IReadOnlyList<CanalDto>> ObtenerTodosCanalesAsync()
        {
            // Utilizamos el método del repositorio que incluye los subcanales
            var canales = await _canalRepository.GetCanalesWithSubcanalesAsync();

            // Mapear cada entidad al DTO
            var result = new List<CanalDto>();
            foreach (var canal in canales)
            {
                var canalDto = _mapper.Map<Canal, CanalDto>(canal);

                // Asegurarse de que los planes se mapean correctamente
                if (canal.PlanesCanales != null && canal.PlanesCanales.Any())
                {
                    canalDto.PlanesCanal = new List<PlanCanalDto>();
                    foreach (var planCanal in canal.PlanesCanales)
                    {
                        canalDto.PlanesCanal.Add(new PlanCanalDto
                        {
                            Id = planCanal.Id,
                            PlanId = planCanal.PlanId,
                            CanalId = planCanal.CanalId,
                            Activo = planCanal.Activo,
                            Plan = _mapper.Map<Plan, PlanDto>(planCanal.Plan)
                        });
                    }
                }

                // Obtener los oficiales comerciales asignados a este canal específico
                var oficialesComerciales = await _canalOficialComercialRepository.GetOficialesComercialesByCanalAsync(canal.Id);
                if (oficialesComerciales != null && oficialesComerciales.Any())
                {
                    canalDto.OficialesComerciales = oficialesComerciales
                        .Select(oc => new OficialComercialResumenDto
                        {
                            Id = oc.OficialComercialId,
                            Nombre = oc.OficialComercial?.Nombre ?? "",
                            Apellido = oc.OficialComercial?.Apellido ?? "",
                            FechaAsignacion = oc.FechaAsignacion
                        })
                        .ToList();
                }
                else
                {
                    canalDto.OficialesComerciales = new List<OficialComercialResumenDto>();
                }

                result.Add(canalDto);
            }

            return result;
        }

        public async Task<CanalDto> ObtenerCanalPorIdAsync(int id)
        {
            var canal = await _canalRepository.GetCanalWithSubcanalesAndPlanesAsync(id);
            if (canal == null)
            {
                return null;
            }

            // Mapear la entidad al DTO
            var canalDto = _mapper.Map<Canal, CanalDto>(canal);

            // Obtener los oficiales comerciales asignados a este canal
            var oficialesComerciales = await _canalOficialComercialRepository.GetOficialesComercialesByCanalAsync(id);
            if (oficialesComerciales != null && oficialesComerciales.Any())
            {
                canalDto.OficialesComerciales = oficialesComerciales
                    .Select(oc => new OficialComercialResumenDto
                    {
                        Id = oc.OficialComercialId,
                        Nombre = oc.OficialComercial?.Nombre ?? "",
                        Apellido = oc.OficialComercial?.Apellido ?? "",
                        FechaAsignacion = oc.FechaAsignacion
                    })
                    .ToList();
            }
            else
            {
                canalDto.OficialesComerciales = new List<OficialComercialResumenDto>();
            }

            return canalDto;
        }

        public async Task<CanalDto> ObtenerCanalConDetallesAsync(int id)
        {
            var canal = await _canalRepository.GetCanalWithSubcanalesAndPlanesAsync(id);
            if (canal == null)
            {
                return null;
            }

            // Mapear la entidad al DTO incluyendo todos los detalles
            var canalDto = _mapper.Map<Canal, CanalDto>(canal);

            // Asegurarse de que los planes se mapean correctamente
            if (canal.PlanesCanales != null && canal.PlanesCanales.Any())
            {
                canalDto.PlanesCanal = new List<PlanCanalDto>();
                foreach (var planCanal in canal.PlanesCanales)
                {
                    canalDto.PlanesCanal.Add(new PlanCanalDto
                    {
                        Id = planCanal.Id,
                        PlanId = planCanal.PlanId,
                        CanalId = planCanal.CanalId,
                        Activo = planCanal.Activo,
                        Plan = _mapper.Map<Plan, PlanDto>(planCanal.Plan)
                    });
                }
            }

            // Obtener los oficiales comerciales asignados a este canal
            var oficialesComerciales = await _canalOficialComercialRepository.GetOficialesComercialesByCanalAsync(id);
            if (oficialesComerciales != null && oficialesComerciales.Any())
            {
                canalDto.OficialesComerciales = oficialesComerciales
                    .Select(oc => new OficialComercialResumenDto
                    {
                        Id = oc.OficialComercialId,
                        Nombre = oc.OficialComercial?.Nombre ?? "",
                        Apellido = oc.OficialComercial?.Apellido ?? "",
                        FechaAsignacion = oc.FechaAsignacion
                    })
                    .ToList();
            }
            else
            {
                canalDto.OficialesComerciales = new List<OficialComercialResumenDto>();
            }

            return canalDto;
        }

        public async Task ActivarDesactivarCanalAsync(int canalId, bool activar)
        {
            await _canalRepository.ActivateDesactivateAsync(canalId, activar);
        }

        public async Task ActivarDesactivarSubcanalesAsync(int canalId, bool activar)
        {
            await _canalRepository.ActivateDesactivateSubcanalesAsync(canalId, activar);
        }

        public async Task<bool> EliminarCanalAsync(int canalId)
        {
            var canal = await _canalRepository.GetByIdAsync(canalId);

            if (canal == null)
                throw new KeyNotFoundException($"No se encontró el canal con ID {canalId}");

            // Opcional: Verificar si hay relaciones que impidan eliminar el canal
            // Por ejemplo, verificar si tiene subcanales, planes asignados, etc.

            await _canalRepository.DeleteAsync(canal);
            return true; // Indica que la eliminación fue exitosa
        }
        public async Task ActualizarCanalAsync(int id, CanalCrearDto canalDto)
        {
            var canal = await _canalRepository.GetByIdAsync(id);
            if (canal == null)
            {
                throw new KeyNotFoundException($"No se encontró el canal con ID {id}");
            }

            // Actualizar solo las propiedades que no sean nulas o vacías
            if (!string.IsNullOrEmpty(canalDto.NombreFantasia))
                canal.NombreFantasia = canalDto.NombreFantasia;

            if (!string.IsNullOrEmpty(canalDto.RazonSocial))
                canal.RazonSocial = canalDto.RazonSocial;

            if (!string.IsNullOrEmpty(canalDto.Provincia))
                canal.Provincia = canalDto.Provincia;

            if (!string.IsNullOrEmpty(canalDto.Localidad))
                canal.Localidad = canalDto.Localidad;

            if (!string.IsNullOrEmpty(canalDto.Direccion))
                canal.Direccion = canalDto.Direccion;

            if (!string.IsNullOrEmpty(canalDto.Cuit))
                canal.Cuit = canalDto.Cuit;

            if (!string.IsNullOrEmpty(canalDto.CBU))
                canal.CBU = canalDto.CBU;

            if (!string.IsNullOrEmpty(canalDto.Alias))
                canal.Alias = canalDto.Alias;

            if (!string.IsNullOrEmpty(canalDto.Banco))
                canal.Banco = canalDto.Banco;

            if (!string.IsNullOrEmpty(canalDto.NumCuenta))
                canal.NumCuenta = canalDto.NumCuenta;

            if (!string.IsNullOrEmpty(canalDto.TipoCanal))
                canal.TipoCanal = canalDto.TipoCanal;

            if (!string.IsNullOrEmpty(canalDto.OpcionesCobro))
                canal.OpcionesCobro = canalDto.OpcionesCobro;

            if (!string.IsNullOrEmpty(canalDto.Foto))
                canal.Foto = canalDto.Foto;

            if (!string.IsNullOrEmpty(canalDto.TitularNombreCompleto))
                canal.TitularNombreCompleto = canalDto.TitularNombreCompleto;

            if (!string.IsNullOrEmpty(canalDto.TitularTelefono))
                canal.TitularTelefono = canalDto.TitularTelefono;

            if (!string.IsNullOrEmpty(canalDto.TitularEmail))
                canal.TitularEmail = canalDto.TitularEmail;

            await _canalRepository.UpdateAsync(canal);

            // Actualizar oficiales comerciales si se proporcionaron
            if (canalDto.OficialesComerciales != null && canalDto.OficialesComerciales.Any())
            {
                // Obtener los OC actuales
                var ocActuales = await _canalOficialComercialRepository.GetOficialesComercialesByCanalAsync(id);
                var idsActuales = ocActuales.Select(oc => oc.OficialComercialId).ToList();

                // Agregar nuevos OC que no estén ya asignados
                foreach (var ocId in canalDto.OficialesComerciales.Where(ocId => !idsActuales.Contains(ocId)))
                {
                    var dto = new CanalOficialComercialCrearDto
                    {
                        CanalId = id,
                        OficialComercialId = ocId
                    };

                    await _canalOficialComercialService.AsignarOficialComercialACanalAsync(dto);
                }

                // Desasignar OC que ya no estén en la lista
                foreach (var ocActual in ocActuales)
                {
                    if (!canalDto.OficialesComerciales.Contains(ocActual.OficialComercialId))
                    {
                        await _canalOficialComercialService.DesasignarOficialComercialDeCanalAsync(
                            id, ocActual.OficialComercialId);
                    }
                }
            }
        }
    }
}