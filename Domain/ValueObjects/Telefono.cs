using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public class Telefono
    {
        public string Numero { get; private set; }

        public Telefono(string numero)
        {
            // Aquí podrías agregar validaciones (por ejemplo, formato de número de teléfono)
            if (string.IsNullOrEmpty(numero)) throw new ArgumentException("El número no puede estar vacío");
            Numero = numero;
        }

        // Igualdad por valor
        public override bool Equals(object obj)
        {
            if (obj is Telefono other)
            {
                return this.Numero == other.Numero;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Numero.GetHashCode();
        }
    }
}
