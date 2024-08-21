using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class GeneracionSolicitudesException : ApiException
{
    public GeneracionSolicitudesException(string message) : base(message)
    {
    }
}
