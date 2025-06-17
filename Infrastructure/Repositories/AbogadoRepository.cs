using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repositorios;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.NewFolder
{
    public class AbogadoRepository :IAbogadoRepository
    {
        private readonly AbogadosContext _context;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public AbogadoRepository(AbogadosContext context)
        {
            _context = context;
        }

        public async Task<Abogado> GetByIdAsync(int id)
        {
            return await _context.Set<Abogado>().FindAsync(id);
        }

        public async Task<Abogado> GetByCedulaAsync(string cedula)
        {
            return await _context.Set<Abogado>().FirstOrDefaultAsync(a => a.Cedula == cedula);
        }

        public async Task<IEnumerable<Abogado>> GetAllAsync()
        {
            Logger.Info("Iniciando la obtención de la lista de abogados.");
            return await _context.Set<Abogado>().ToListAsync();
        }

        public async Task AddAsync(Abogado abogado)
        {
            await _context.Set<Abogado>().AddAsync(abogado);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Abogado abogado)
        {
            _context.Set<Abogado>().Update(abogado);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var abogado = await GetByIdAsync(id);
            if (abogado != null)
            {
                _context.Set<Abogado>().Remove(abogado);
                await _context.SaveChangesAsync();
            }
        }
    }
}
