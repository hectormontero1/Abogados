using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repositorios;

namespace Domain.Servicio
{
    public class AbogadoService
    {
        private readonly IAbogadoRepository _abogadoRepository;

        public AbogadoService(IAbogadoRepository abogadoRepository)
        {
            _abogadoRepository = abogadoRepository;
        }

        public async Task<bool> ValidarYCrearAbogadoAsync(Abogado abogado)
        {
            // Validar negocio (por ejemplo, no permitir cédulas duplicadas)
            var abogadoExistente = await _abogadoRepository.GetByCedulaAsync(abogado.Cedula);
            if (abogadoExistente != null)
            {
                throw new InvalidOperationException("Ya existe un abogado con esa cédula");
            }

            // Si pasa validaciones, guardar el abogado
            await _abogadoRepository.AddAsync(abogado);
            return true;
        }
    }
}
