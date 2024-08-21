using Application.Exceptions;

namespace Domain.Exceptions.SincronizacionDatosCircunscripcion;

public class SincronizacionDatosException : ApiException
{
    public SincronizacionDatosException(string message) : base(message)
    {
    }
}
