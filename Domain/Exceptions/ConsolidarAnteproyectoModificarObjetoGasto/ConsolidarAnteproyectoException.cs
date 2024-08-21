using Application.Exceptions;

namespace Domain.Exceptions;

public class ConsolidarAnteproyectoException : ApiException
{
    public ConsolidarAnteproyectoException(string message) : base(message)
    {
    }
}
