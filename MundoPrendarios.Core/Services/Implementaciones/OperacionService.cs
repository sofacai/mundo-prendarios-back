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
                VendedorId = operacionDto.VendedorId,
                SubcanalId = operacionDto.SubcanalId ?? 0,
                CanalId = operacionDto.CanalId ?? cliente.CanalId,
                FechaCreacion = DateTime.Now,
                Estado = operacionDto.Estado ?? "Propuesta",
                EstadoDashboard = operacionDto.EstadoDashboard ?? "INGRESADA",
                UsuarioCreadorId = operacionDto.UsuarioCreadorId ?? usuarioId,
                CuotaInicial = operacionDto.CuotaInicial,
                CuotaPromedio = operacionDto.CuotaPromedio,
                AutoInicial = operacionDto.AutoInicial,
                Observaciones = operacionDto.Observaciones,

                // *** NUEVOS CAMPOS AGREGADOS ***
                GastoInicial = operacionDto.GastoInicial,
                BancoInicial = operacionDto.BancoInicial
            };

            // Lógica existente para asignar subcanal si el usuario es vendor...
            if (usuarioId.HasValue && !operacion.VendedorId.HasValue)
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario != null && usuario.RolId == 3) // Si es vendor
                {
                    operacion.VendedorId = usuarioId.Value;

                    var subcanalVendors = await _subcanalRepository.GetSubcanalesByVendorAsync(usuarioId.Value);
                    if (subcanalVendors.Any() && operacion.SubcanalId == 0)
                    {
                        operacion.SubcanalId = subcanalVendors.First().Id;
                        operacion.CanalId = subcanalVendors.First().CanalId;
                    }
                }
            }

            await _operacionRepository.AddAsync(operacion);

            if (operacion.VendedorId.HasValue)
            {
                await ActualizarEstadisticasVendorAsync(operacion.VendedorId.Value);
            }

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
        public async Task<bool> EliminarOperacionAsync(int operacionId)
        {
            var operacion = await _operacionRepository.GetByIdAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            await _operacionRepository.DeleteAsync(operacion);
            return true; // Indica que la eliminación fue exitosa
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

            // Si no se especificó un UsuarioCreadorId, usar el usuario actual
            if (!operacionDto.UsuarioCreadorId.HasValue)
            {
                operacionDto.UsuarioCreadorId = usuarioId;
            }

            // Crear la operación usando el método existente
            return await CrearOperacionAsync(operacionDto, usuarioId);
        }

        public async Task<OperacionDto> AprobarOperacionAsync(int operacionId, OperacionAprobarDto aprobarDto)
        {
            var operacion = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            // Actualizar los campos de aprobación existentes
            operacion.MontoAprobado = aprobarDto.MontoAprobado;
            operacion.MesesAprobados = aprobarDto.MesesAprobados;
            operacion.TasaAprobada = aprobarDto.TasaAprobada;
            operacion.PlanAprobadoId = aprobarDto.PlanAprobadoId;
            operacion.PlanAprobadoNombre = aprobarDto.PlanAprobadoNombre;
            operacion.FechaAprobacion = DateTime.Now;
            operacion.Estado = "Aprobada";
            operacion.EstadoDashboard = DeterminarEstadoDashboard("Aprobada");
            operacion.CuotaInicialAprobada = aprobarDto.CuotaInicialAprobada;
            operacion.CuotaPromedioAprobada = aprobarDto.CuotaPromedioAprobada;
            operacion.AutoAprobado = aprobarDto.AutoAprobado;
            operacion.UrlAprobadoDefinitivo = aprobarDto.UrlAprobadoDefinitivo;

            // *** NUEVOS CAMPOS AGREGADOS ***
            operacion.GastoAprobado = aprobarDto.GastoAprobado;
            operacion.BancoAprobado = aprobarDto.BancoAprobado;

            await _operacionRepository.UpdateAsync(operacion);

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
            operacion.EstadoDashboard = DeterminarEstadoDashboard("Liquidada");
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

        public async Task<OperacionDto> ActualizarDesdeWebhookAsync(OperacionWebhookUpdateDto dto)
        {
            var operacion = await _operacionRepository.GetByIdAsync(dto.OperacionId);
            if (operacion == null)
                throw new KeyNotFoundException($"No se encontró la operación con ID {dto.OperacionId}");

            // Actualizar campos existentes
            if (dto.MontoAprobado.HasValue)
                operacion.MontoAprobado = dto.MontoAprobado;

            if (dto.MontoAprobadoBanco.HasValue)
                operacion.MontoAprobadoBanco = dto.MontoAprobadoBanco;

            if (dto.TasaAprobada.HasValue)
                operacion.TasaAprobada = dto.TasaAprobada;

            if (dto.MesesAprobados.HasValue)
                operacion.MesesAprobados = dto.MesesAprobados;

            if (!string.IsNullOrEmpty(dto.PlanAprobadoNombre))
                operacion.PlanAprobadoNombre = dto.PlanAprobadoNombre;

            if (!string.IsNullOrEmpty(dto.EstadoDesdeEtiqueta))
                operacion.Estado = dto.EstadoDesdeEtiqueta;

            if (dto.CuotaInicialAprobada.HasValue)
                operacion.CuotaInicialAprobada = dto.CuotaInicialAprobada;

            if (dto.CuotaPromedioAprobada.HasValue)
                operacion.CuotaPromedioAprobada = dto.CuotaPromedioAprobada;

            if (!string.IsNullOrEmpty(dto.AutoAprobado))
                operacion.AutoAprobado = dto.AutoAprobado;

            if (!string.IsNullOrEmpty(dto.Observaciones))
                operacion.Observaciones = dto.Observaciones;

            // *** NUEVOS CAMPOS AGREGADOS ***
            if (dto.GastoAprobado.HasValue)
                operacion.GastoAprobado = dto.GastoAprobado;

            if (!string.IsNullOrEmpty(dto.BancoAprobado))
                operacion.BancoAprobado = dto.BancoAprobado;

            operacion.FechaAprobacion = DateTime.UtcNow;

            await _operacionRepository.UpdateAsync(operacion);

            var actualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacion.Id);
            return _mapper.Map<OperacionDto>(actualizada);
        }

        public async Task<OperacionDto> ActualizarFechaAprobacionAsync(int operacionId, DateTime? fechaAprobacion)
        {
            var operacion = await _operacionRepository.GetByIdAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            // Actualizar la FechaAprobacion (puede ser null para quitarla)
            operacion.FechaAprobacion = fechaAprobacion;

            await _operacionRepository.UpdateAsync(operacion);

            // Devolver la operación actualizada con todos los detalles
            var operacionActualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            return _mapper.Map<OperacionDto>(operacionActualizada);
        }

        public async Task<OperacionDto> ActualizarFechaLiquidacionAsync(int operacionId, DateTime? fechaLiquidacion)
        {
            var operacion = await _operacionRepository.GetByIdAsync(operacionId);
            if (operacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la operación con ID {operacionId}");
            }

            // Actualizar la FechaLiquidacion (puede ser null para quitarla)
            operacion.FechaLiquidacion = fechaLiquidacion;
            
            // Si se establece fecha, marcar como liquidada; si se quita, desmarcar
            operacion.Liquidada = fechaLiquidacion.HasValue;

            await _operacionRepository.UpdateAsync(operacion);

            // Devolver la operación actualizada con todos los detalles
            var operacionActualizada = await _operacionRepository.GetOperacionWithDetailsAsync(operacionId);
            return _mapper.Map<OperacionDto>(operacionActualizada);
        }

        private static string DeterminarEstadoDashboard(string estado)
        {
            return estado switch
            {
                // LIQUIDADAS
                "LIQUIDADO" => "LIQUIDADA",     // Estado de Kommo
                "Liquidada" => "LIQUIDADA",     // Estado manual del servicio
                
                // INGRESADAS
                "RECHAZADO" => "INGRESADA",     // Rechazada de Kommo
                "Rechazada" => "INGRESADA",     // Rechazada manual
                "ENVIADA MP" => "INGRESADA",    // Enviada a Mundo Prendarios
                "Ingresada" => "INGRESADA",     // Estado base
                
                // APROBADAS (estados en proceso)
                "APROBADO DEF" => "APROBADA",   // Aprobado definitivo
                "APROBADO PROV." => "APROBADA", // Aprobado provisorio
                "EN PROC. LIQ." => "APROBADA",  // En proceso de liquidación
                "Aprobada" => "APROBADA",       // Estado manual de aprobación
                "CONFEC. PRENDA" => "APROBADA", // Confección de prenda va al grupo APROBADA
                
                // Estados legacy que van a APROBADA
                "En gestión" => "APROBADA",     // Legacy: ahora va al grupo APROBADA
                "Propuesta" => "APROBADA",      // Legacy: ahora va al grupo APROBADA
                
                _ => "APROBADA" // Cualquier otro estado va a APROBADA por defecto
            };
        }
    }
}