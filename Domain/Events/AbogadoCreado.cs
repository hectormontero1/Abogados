using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events
{
    public class AbogadoCreado
    {
        public int IdAbogado { get; private set; }
        public string Nombre { get; private set; }

        public AbogadoCreado(int idAbogado, string nombre)
        {
            IdAbogado = idAbogado;
            Nombre = nombre;
        }
    }
}
