using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class EstadoDocumentoExcelException : ApiException
{
    public EstadoDocumentoExcelException(string message) : base(message)
    {

    }
}
