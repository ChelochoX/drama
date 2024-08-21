using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class ParametroFaltanteCadenaConexionException : ApiException
{
    public ParametroFaltanteCadenaConexionException(string message) : base(message)
    {

    }
}
