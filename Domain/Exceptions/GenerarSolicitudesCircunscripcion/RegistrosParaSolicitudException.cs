using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class RegistrosParaSolicitudException : ApiException
{
    public RegistrosParaSolicitudException(string message) : base(message)
    {
    }
}
