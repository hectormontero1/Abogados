using Domain.Models;
using Domain.Repositorios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ILogger = NLog.ILogger;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbogadoController : ControllerBase
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAbogadoRepository _abogadoRepository;

        // Inyección de dependencias
        public AbogadoController(IAbogadoRepository abogadoRepository)
        {
            _abogadoRepository = abogadoRepository;
        }

        // GET: api/abogado
        [HttpGet]
        public async Task<IActionResult> GetAbogados()
        {
            Logger.Warn("Iniciando la obtención de la lista de abogados.");
            var abogados = await _abogadoRepository.GetAllAsync();
            if (abogados == null || !abogados.Any())
            {
                return NotFound("No se encontraron abogados.");
            }

            return Ok(abogados);
        }

        // GET: api/abogado/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAbogado(int id)
        {
            var abogado = await _abogadoRepository.GetByIdAsync(id);
            if (abogado == null)
            {
                return NotFound($"No se encontró el abogado con ID {id}");
            }

            return Ok(abogado);
        }

        // POST: api/abogado
        [HttpPost]
        public async Task<IActionResult> PostAbogado([FromBody] Abogado abogado)
        {
            if (abogado == null)
            {
                return BadRequest("El abogado no puede ser nulo.");
            }

            await _abogadoRepository.AddAsync(abogado);
            return CreatedAtAction(nameof(GetAbogado), new { id = abogado.IdAbogado }, abogado);
        }

        // PUT: api/abogado/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAbogado(int id, [FromBody] Abogado abogado)
        {
            if (id != abogado.IdAbogado)
            {
                return BadRequest("El ID del abogado no coincide con el ID proporcionado.");
            }

            var existingAbogado = await _abogadoRepository.GetByIdAsync(id);
            if (existingAbogado == null)
            {
                return NotFound($"No se encontró el abogado con ID {id}");
            }

            await _abogadoRepository.UpdateAsync(abogado);
            return NoContent(); // OK, sin cuerpo
        }

        // DELETE: api/abogado/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAbogado(int id)
        {
            var abogado = await _abogadoRepository.GetByIdAsync(id);
            if (abogado == null)
            {
                return NotFound($"No se encontró el abogado con ID {id}");
            }

            await _abogadoRepository.DeleteAsync(id);
            return NoContent(); // OK, sin cuerpo
        }
    }
}
