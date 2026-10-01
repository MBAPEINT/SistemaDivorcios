using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Negocio.Excepciones
{
    public sealed class ConflictoNegocioException : Exception
    {
        public ConflictoNegocioException(string mensaje) 
            : base(mensaje)
        {
        }
        public ConflictoNegocioException(string mensaje, Exception excepcionInterna)
            : base(mensaje, excepcionInterna)
        {
        }
    }
}
