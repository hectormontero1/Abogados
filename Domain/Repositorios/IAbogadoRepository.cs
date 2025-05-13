using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;


namespace Domain.Repositorios
{
    public interface IAbogadoRepository
    {
        Task<Abogado> GetByIdAsync(int id);
        Task<Abogado> GetByCedulaAsync(string cedula);
        Task<IEnumerable<Abogado>> GetAllAsync();
        Task AddAsync(Abogado abogado);
        Task UpdateAsync(Abogado abogado);
        Task DeleteAsync(int id);
    }
}
