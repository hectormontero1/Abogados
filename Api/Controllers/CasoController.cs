using Domain.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CasoController : ControllerBase
{
    private readonly ICasoService _casoService;

    public CasoController(ICasoService casoService)
    {
        _casoService = casoService;
    }
     
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Caso>>> GetCasos()
    {
        var casos = await _casoService.ObtenerTodosAsync();
        return Ok(casos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Caso>> GetCaso(int id)
    {
        var caso = await _casoService.ObtenerPorIdAsync(id);
        if (caso == null)
            return NotFound();

        return Ok(caso);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCaso([FromBody] Caso caso)
    {
        await _casoService.CrearAsync(caso);
        return CreatedAtAction(nameof(GetCaso), new { id = caso.IdCaso }, caso);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCaso(int id, [FromBody] Caso caso)
    {
        if (id != caso.IdCaso)
            return BadRequest("ID del caso no coincide");

        await _casoService.ActualizarAsync(caso);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCaso(int id)
    {
        await _casoService.EliminarAsync(id);
        return NoContent();
    }
}