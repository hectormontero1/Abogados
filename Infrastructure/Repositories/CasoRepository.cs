using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CasoRepository : ICasoRepository
{
    private readonly AbogadosContext _context;

    public CasoRepository(AbogadosContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Caso>> GetAllAsync()
    {
        return await _context.Set<Caso>()
            .Include(c => c.IdClienteNavigation)
            .Include(c => c.IdAbogadoResponsableNavigation)
            .ToListAsync();
    }

    public async Task<Caso?> GetByIdAsync(int id)
    {
        return await _context.Set<Caso>()
            .Include(c => c.Documentos)
            .Include(c => c.Tareas)
            .FirstOrDefaultAsync(c => c.IdCaso == id);
    }

    public async Task AddAsync(Caso caso)
    {
        await _context.Set<Caso>().AddAsync(caso);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Caso caso)
    {
        _context.Set<Caso>().Update(caso);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var caso = await _context.Set<Caso>().FindAsync(id);
        if (caso != null)
        {
            _context.Set<Caso>().Remove(caso);
            await _context.SaveChangesAsync();
        }
    }
}