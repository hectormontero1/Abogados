using Domain.Models;

public interface ICasoRepository
{
    Task<IEnumerable<Caso>> GetAllAsync();
    Task<Caso?> GetByIdAsync(int id);
    Task AddAsync(Caso caso);
    Task UpdateAsync(Caso caso);
    Task DeleteAsync(int id);
}