using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Datos.Excepciones
{
    public sealed class CodigoDuplicadoPersistenciaException : Exception
    {
        public CodigoDuplicadoPersistenciaException(
            string mensaje, Exception excepcionInterna) 
            : base(mensaje, excepcionInterna)
        {
        }
    }
}
