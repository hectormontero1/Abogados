using Domain.Models;

public class CasoService : ICasoService
{
    private readonly ICasoRepository _casoRepository;

    public CasoService(ICasoRepository casoRepository)
    {
        _casoRepository = casoRepository;
    }

    public async Task<IEnumerable<Caso>> ObtenerTodosAsync()
    {
        return await _casoRepository.GetAllAsync();
    }

    public async Task<Caso?> ObtenerPorIdAsync(int id)
    {
        return await _casoRepository.GetByIdAsync(id);
    }

    public async Task CrearAsync(Caso caso)
    {
        await _casoRepository.AddAsync(caso);
    }

    public async Task ActualizarAsync(Caso caso)
    {
        await _casoRepository.UpdateAsync(caso);
    }

    public async Task EliminarAsync(int id)
    {
        await _casoRepository.DeleteAsync(id);
    }
}