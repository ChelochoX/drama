using Application.Exceptions;

namespace Domain.Exceptions.Referencias;

public class ReferenciasException : ApiException
{
    public ReferenciasException(string message) : base(message)
    {
    }
}
