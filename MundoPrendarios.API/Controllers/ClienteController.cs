using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Implementaciones;
using MundoPrendarios.Core.Services.Interfaces;
using MundoPrendarios.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly IOperacionService _operacionService;
        private readonly IClienteRepository _clienteRepository;

        public ClientesController(IClienteService clienteService,
                          IOperacionService operacionService,
                          IClienteRepository clienteRepository)
        {
            _clienteService = clienteService;
            _operacionService = operacionService;
            _clienteRepository = clienteRepository;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _clienteService.ObtenerTodosClientesAsync();
            return Ok(clientes);
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _clienteService.ObtenerClientePorIdAsync(id);

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        // GET: api/clientes/dni/12345678
        [HttpGet("dni/{dni}")]
        public async Task<ActionResult<ClienteDto>> GetClienteByDni(string dni)
        {
            var cliente = await _clienteService.ObtenerClientePorDniAsync(dni);

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        // GET: api/clientes/cuil/20123456789
        [HttpGet("cuil/{cuil}")]
        public async Task<ActionResult<ClienteDto>> GetClienteByCuil(string cuil)
        {
            var cliente = await _clienteService.ObtenerClientePorCuilAsync(cuil);

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        // GET: api/clientes/canal/5
        [HttpGet("canal/{canalId}")]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientesByCanal(int canalId)
        {
            var clientes = await _clienteService.ObtenerClientesPorCanalAsync(canalId);
            return Ok(clientes);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] ClienteCrearDto clienteDto)
        {
            // Validar que al menos se proporcione CUIL o DNI
            if (string.IsNullOrEmpty(clienteDto.Cuil) && string.IsNullOrEmpty(clienteDto.Dni))
            {
                return BadRequest("Se debe proporcionar al menos un CUIL o DNI");
            }

            var nuevoCliente = await _clienteService.CrearClienteAsync(clienteDto, null);
            return CreatedAtAction(nameof(GetCliente), new { id = nuevoCliente.Id }, nuevoCliente);
        }



        // PUT: api/clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteCrearDto clienteDto)
        {
            var clienteExistente = await _clienteService.ObtenerClientePorIdAsync(id);

            if (clienteExistente == null)
                return NotFound();

            await _clienteService.ActualizarClienteAsync(id, clienteDto);
            return NoContent();
        }

      


    }
}