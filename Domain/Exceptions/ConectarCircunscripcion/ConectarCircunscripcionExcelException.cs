using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class ConectarCircunscripcionExcelException : ApiException
{
    public ConectarCircunscripcionExcelException(string message) : base(message)
    {

    }
}
