using Application.Exceptions;

namespace Domain.Exceptions.ImportarArchivoSIPOIExcepcions;

public class ListadoBienesNoInsertadosException : ApiException
{
    public ListadoBienesNoInsertadosException(string message) : base(message)
    {
    }
}
