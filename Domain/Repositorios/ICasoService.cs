using Domain.Models;

public interface ICasoService
{
    Task<IEnumerable<Caso>> ObtenerTodosAsync();
    Task<Caso?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Caso caso);
    Task ActualizarAsync(Caso caso);
    Task EliminarAsync(int id);
}