// MundoPrendarios.Core.Services.Implementaciones/OperacionService.cs
using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class OperacionService : IOperacionService
    {
        private readonly IOperacionRepository _operacionRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IPlanRepository _planRepository;
        private readonly ISubcanalRepository _subcanalRepository;
        private readonly IGastoRepository _gastoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPlanCanalRepository _planCanalRepository;
        private readonly IReglaCotizacionRepository _reglaCotizacionRepository;
        private readonly IMapper _mapper;

        public OperacionService(
            IOperacionRepository operacionRepository,
            IClienteRepository clienteRepository,
            IPlanRepository planRepository,
            ISubcanalRepository subcanalRepository,
            IGastoRepository gastoRepository,
            IUsuarioRepository usuarioRepository,
            IPlanCanalRepository planCanalRepository,
            IReglaCotizacionRepository reglaCotizacionRepository,
            IMapper mapper)
        {
            _operacionRepository = operacionRepository;
            _clienteRepository = clienteRepository;
            _planRepository = planRepository;
            _subcanalRepository = subcanalRepository;
            _gastoRepository = gastoRepository;
            _usuarioRepository = usuarioRepository;
            _planCanalRepository = planCanalRepository;
            _reglaCotizacionRepository = reglaCotizacionRepository;
            _mapper = mapper;
        }

        public async Task<OperacionDto> CrearOperacionAsync(OperacionCrearDto operacionDto, int? usuarioId)
        {
            // Validar que el cliente exista
            var cliente = await _clienteRepository.GetByIdAsync(operacionDto.ClienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"No se encontró el cliente con ID {operacionDto.ClienteId}");
            }

            // Validar que el plan exista
            var plan = await _planRepository.GetByIdAsync(operacionDto.PlanId);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {operacionDto.PlanId}");
            }

            // Crear entidad Operacion
            var operacion = new Operacion
            {
                Monto = operacionDto.Monto,
                Meses = operacionDto.Meses,
                Tasa = operacionDto.Tasa,
                ClienteId = operacionDto.ClienteId,
                PlanId = operacionDto.PlanId,
                VendedorId = operacionDto.VendedorId ?? usuarioId ?? 0,
                SubcanalId = operacionDto.SubcanalId ?? 0,
                CanalId = operacionDto.CanalId ?? cliente.CanalId,
                FechaCreacion = DateTime.Now,
                Estado = operacionDto.Estado,
                UsuarioCreadorId = operacionDto.UsuarioCreadorId ?? usuarioId
            };

            // Si el usuario está logueado y es un vendor, asignar su subcanal si no viene especificado
            if (usuarioId.HasValue && operacionDto.SubcanalId == null)
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario != null && usuario.RolId == 3) // Rol Vendor
                {
                    // Obtener el primer subcanal al que pertenece el vendor
                    var subcanalVendors = await _subcanalRepository.GetSubcanalesByVendorAsync(usuarioId.Value);
                    if (subcanalVendors.Any())
                    {
                        operacion.SubcanalId = subcanalVendors.First().Id;
                        // También asignar el canal basándonos en el subcanal
                        operacion.CanalId = subcanalVendors.First().CanalId;
                    }
                }
            }

            await _operacionRepository.AddAsync(operacion);

            // Actualizar información del vendor
            if (operacion.VendedorId > 0)
            {
                await ActualizarEstadisticasVendorAsync(operacion.VendedorId);
            }

            // Cargar datos relacionados para el DTO
            var operacionDetallada = await _operacionRepository.GetOperacionWithDetailsAsync(operacion.Id);
            return _mapper.Map<OperacionDto>(operacionDetallada);
        }

        // Método para actualizar las estadísticas del vendor
        public async Task ActualizarEstadisticasVendorAsync(int vendorId)
        {
            var vendor = await _usuarioRepository.GetByIdAsync(vendorId);

            // Verificar que sea un vendor (RolId = 3)
            if (vendor != null && vendor.RolId == 3)
            {
                // Actualizar fecha de última operación
                vendor.FechaUltimaOperacion = DateTime.Now;

                // Incrementar el contador de operaciones
                vendor.CantidadOperaciones += 1;

                await _usuarioRepository.UpdateAsync(vendor);
            }
        }

        public async Task<OperacionDto> ObtenerOperacionPorIdAsync(int id)
        {
            var operacion = await _operacionRepository.GetOperacionWithDetailsAsync(id);
            if (operacion == null)
            {
                return null;
            }
            return _mapper.Map<OperacionDto>(operacion);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerTodasOperacionesAsync()
        {
            var operaciones = await _operacionRepository.GetAllOperacionesWithDetailsAsync();
            // Aquí está el cambio clave - mapear explícitamente a List<OperacionDto>
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorClienteAsync(int clienteId)
        {
            var operaciones = await _operacionRepository.GetOperacionesByClienteAsync(clienteId);
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorVendedorAsync(int vendedorId)
        {
            var operaciones = await _operacionRepository.GetOperacionesByVendedorAsync(vendedorId);
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorSubcanalAsync(int subcanalId)
        {
            var operaciones = await _operacionRepository.GetOperacionesBySubcanalAsync(subcanalId);
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorCanalAsync(int canalId)
        {
            var operaciones = await _operacionRepository.GetOperacionesByCanalAsync(canalId);
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<CotizacionResultadoDto> CotizarSinLoginAsync(OperacionCotizarDto cotizacionDto)
        {
            // Buscar reglas de cotización aplicables
            var reglas = await _reglaCotizacionRepository.GetReglasByRangeAsync(cotizacionDto.Monto, cotizacionDto.Meses);
            if (!reglas.Any())
            {
                throw new InvalidOperationException("No se encontraron reglas de cotización aplicables para el monto y cuotas especificados.");
            }

            // Usar la primera regla aplicable (podría mejorarse con una lógica más sofisticada)
            var regla = reglas.First();

            // Calcular cuota mensual (fórmula simple, podría ser más compleja según necesidades)
            decimal tasaMensual = regla.Tasa / 12 / 100;
            decimal cuotaMensual = (cotizacionDto.Monto * tasaMensual * (decimal)Math.Pow(1 + (double)tasaMensual, cotizacionDto.Meses)) /
                                    ((decimal)Math.Pow(1 + (double)tasaMensual, cotizacionDto.Meses) - 1);

            if (regla.GastoOtorgamiento > 0)
            {
                cuotaMensual += regla.GastoOtorgamiento / cotizacionDto.Meses;
            }

            decimal montoTotal = cuotaMensual * cotizacionDto.Meses;

            return new CotizacionResultadoDto
            {
                Monto = cotizacionDto.Monto,
                Meses = cotizacionDto.Meses,
                Tasa = regla.Tasa,
                GastoOtorgamiento = regla.GastoOtorgamiento,
                CuotaMensual = decimal.Round(cuotaMensual, 2),
                MontoTotal = decimal.Round(montoTotal, 2),
                PlanNombre = regla.Nombre,
                PlanId = regla.Id,
                GastosAplicados = new List<GastoAplicadoDto>() // sin gastos para cotización sin login
            };
        }

        public async Task<CotizacionResultadoDto> CotizarConLoginAsync(OperacionCotizarDto cotizacionDto, int usuarioId)
        {
            // Verificar si el usuario es vendor y tiene asignado un subcanal
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {usuarioId}");
            }

            int subcanalId;
            if (cotizacionDto.SubcanalId.HasValue)
            {
                // Usar el subcanal proporcionado en la cotización
                subcanalId = cotizacionDto.SubcanalId.Value;

                // Verificar que si el usuario es vendor, pertenezca a este subcanal
                if (usuario.RolId == 3) // Rol Vendor
                {
                    bool pertenece = await _subcanalRepository.CheckVendorAssignmentAsync(subcanalId, usuarioId);
                    if (!pertenece)
                    {
                        throw new InvalidOperationException("El vendedor no pertenece al subcanal especificado.");
                    }
                }
            }
            else
            {
                // Si no se proporciona subcanal, buscar uno para el vendor
                if (usuario.RolId == 3) // Rol Vendor
                {
                    var subcanales = await _subcanalRepository.GetSubcanalesByVendorAsync(usuarioId);
                    if (!subcanales.Any())
                    {
                        throw new InvalidOperationException("El vendedor no está asignado a ningún subcanal.");
                    }
                    subcanalId = subcanales.First().Id;
                }
                else if (usuario.RolId == 2) // AdminCanal
                {
                    // Para AdminCanal, buscar el primer subcanal que administra
                    var subcanales = await _subcanalRepository.GetAllSubcanalesWithDetailsAsync();
                    var subcanalAdministrado = subcanales.FirstOrDefault(s => s.AdminCanalId == usuarioId);
                    if (subcanalAdministrado == null)
                    {
                        throw new InvalidOperationException("El administrador de canal no tiene asignado ningún subcanal.");
                    }
                    subcanalId = subcanalAdministrado.Id;
                }
                else
                {
                    throw new InvalidOperationException("Se requiere especificar un subcanal para la cotización.");
                }
            }

            // Obtener el subcanal con sus gastos
            var subcanal = await _subcanalRepository.GetSubcanalWithDetailsAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            // Obtener los planes aplicables al canal del subcanal
            var planesCanal = await _planCanalRepository.GetPlanCanalByCanalAsync(subcanal.CanalId);
            if (!planesCanal.Any())
            {
                throw new InvalidOperationException("No hay planes asociados al canal de este subcanal.");
            }

            // Filtrar planes por monto y cuotas
            var planesAplicables = new List<Plan>();
            foreach (var planCanal in planesCanal)
            {
                var plan = planCanal.Plan;
                if (plan != null && plan.Activo &&
                    plan.MontoMinimo <= cotizacionDto.Monto &&
                    plan.MontoMaximo >= cotizacionDto.Monto &&
                    plan.CuotasAplicables.Contains(cotizacionDto.Meses.ToString()) &&
                    DateTime.Now >= plan.FechaInicio &&
                    DateTime.Now <= plan.FechaFin)
                {
                    planesAplicables.Add(plan);
                }
            }

            if (!planesAplicables.Any())
            {
                throw new InvalidOperationException("No se encontraron planes aplicables para el monto y cuotas especificados en este canal.");
            }

            // Usar el primer plan aplicable (podría mejorarse con una lógica más sofisticada)
            var planElegido = planesAplicables.First();

            // Calcular gastos aplicables
            var gastosAplicados = new List<GastoAplicadoDto>();
            decimal totalGastos = 0;

            foreach (var gasto in subcanal.Gastos)
            {
                decimal montoGasto = cotizacionDto.Monto * (gasto.Porcentaje / 100);
                totalGastos += montoGasto;

                gastosAplicados.Add(new GastoAplicadoDto
                {
                    Nombre = gasto.Nombre,
                    Porcentaje = gasto.Porcentaje,
                    MontoAplicado = decimal.Round(montoGasto, 2)
                });
            }

            // Calcular cuota mensual
            decimal tasaMensual = planElegido.Tasa / 12 / 100;
            decimal montoConGastos = cotizacionDto.Monto + totalGastos;
            decimal cuotaMensual = (montoConGastos * tasaMensual * (decimal)Math.Pow(1 + (double)tasaMensual, cotizacionDto.Meses)) /
                                 ((decimal)Math.Pow(1 + (double)tasaMensual, cotizacionDto.Meses) - 1);

            if (planElegido.GastoOtorgamiento > 0)
            {
                cuotaMensual += planElegido.GastoOtorgamiento / cotizacionDto.Meses;
            }

            decimal montoTotal = cuotaMensual * cotizacionDto.Meses;

            return new CotizacionResultadoDto
            {
                Monto = cotizacionDto.Monto,
                Meses = cotizacionDto.Meses,
                Tasa = planElegido.Tasa,
                GastoOtorgamiento = planElegido.GastoOtorgamiento,
                CuotaMensual = decimal.Round(cuotaMensual, 2),
                MontoTotal = decimal.Round(montoTotal, 2),
                PlanNombre = planElegido.Nombre,
                PlanId = planElegido.Id,
                GastosAplicados = gastosAplicados
            };
        }

        public async Task<OperacionDto> CrearClienteYOperacionAsync(ClienteOperacionServicioDto clienteDto, OperacionCrearDto operacionDto, int? usuarioId)
        {
            // Crear el cliente
            var cliente = new Cliente
            {
                Nombre = clienteDto.Nombre,
                Apellido = clienteDto.Apellido,
                Email = clienteDto.Email,
                Telefono = clienteDto.Telefono,
                Cuil = clienteDto.Cuil,
                Dni = clienteDto.Dni,
                Provincia = clienteDto.Provincia,
                Sexo = clienteDto.Sexo,
                EstadoCivil = clienteDto.EstadoCivil,
                CanalId = clienteDto.CanalId ?? 1, // Usar el canal proporcionado o un valor predeterminado
                UsuarioCreadorId = usuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            await _clienteRepository.AddAsync(cliente);

            // Asignar el ID del cliente recién creado a la operación
            operacionDto.ClienteId = cliente.Id;

            // Crear la operación
            var operacion = new Operacion
            {
                Monto = operacionDto.Monto,
                Meses = operacionDto.Meses,
                Tasa = operacionDto.Tasa,
                ClienteId = cliente.Id,
                PlanId = operacionDto.PlanId,
                VendedorId = operacionDto.VendedorId ?? usuarioId ?? 0,
                SubcanalId = operacionDto.SubcanalId ?? 0,
                CanalId = operacionDto.CanalId ?? cliente.CanalId,
                FechaCreacion = DateTime.Now
            };

            // Si el usuario está logueado y es un vendor, asignar su subcanal si no viene especificado
            if (usuarioId.HasValue && operacionDto.SubcanalId == null)
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario != null && usuario.RolId == 3) // Rol Vendor
                {
                    // Obtener el primer subcanal al que pertenece el vendor
                    var subcanalVendors = await _subcanalRepository.GetSubcanalesByVendorAsync(usuarioId.Value);
                    if (subcanalVendors.Any())
                    {
                        operacion.SubcanalId = subcanalVendors.First().Id;
                        // También asignar el canal basándonos en el subcanal
                        operacion.CanalId = subcanalVendors.First().CanalId;
                    }
                }
            }

            await _operacionRepository.AddAsync(operacion);

            // Actualizar información del vendor
            if (operacion.VendedorId > 0)
            {
                await ActualizarEstadisticasVendorAsync(operacion.VendedorId);
            }

            // Cargar datos relacionados para el DTO
            var operacionDetallada = await _operacionRepository.GetOperacionWithDetailsAsync(operacion.Id);
            return _mapper.Map<OperacionDto>(operacionDetallada);
        }

        public async Task<OperacionDto> AprobarOperacionAsync(int operacionId, OperacionAprobarDto aprobarDto)
        {
            var operacion = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            // Actualizar los campos de aprobación
            operacion.MontoAprobado = aprobarDto.MontoAprobado;
            operacion.MesesAprobados = aprobarDto.MesesAprobados;
            operacion.TasaAprobada = aprobarDto.TasaAprobada;
            operacion.PlanAprobadoId = aprobarDto.PlanAprobadoId;
            operacion.FechaAprobacion = DateTime.Now;
            operacion.Estado = "Aprobada";

            await _operacionRepository.UpdateAsync(operacion);

            // Cargar datos relacionados para el DTO
            var operacionActualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            return _mapper.Map<OperacionDto>(operacionActualizada);
        }

        public async Task<OperacionDto> CambiarEstadoOperacionAsync(int operacionId, string estado)
        {
            var operacion = await _operacionRepository.GetByIdAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            operacion.Estado = estado;
            await _operacionRepository.UpdateAsync(operacion);

            // Cargar datos relacionados para el DTO
            var operacionActualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            return _mapper.Map<OperacionDto>(operacionActualizada);
        }

        public async Task<OperacionDto> LiquidarOperacionAsync(int operacionId, DateTime fechaLiquidacion)
        {
            var operacion = await _operacionRepository.GetByIdAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            operacion.Liquidada = true;
            operacion.FechaLiquidacion = fechaLiquidacion;
            operacion.Estado = "Liquidada";
            await _operacionRepository.UpdateAsync(operacion);

            // Cargar datos relacionados para el DTO
            var operacionActualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            return _mapper.Map<OperacionDto>(operacionActualizada);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorEstadoAsync(string estado)
        {
            var operaciones = await _operacionRepository.GetOperacionesByEstadoAsync(estado);
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesLiquidadasAsync()
        {
            var operaciones = await _operacionRepository.GetOperacionesLiquidadasAsync();
            return _mapper.Map<List<OperacionDto>>(operaciones);
        }
    }
}