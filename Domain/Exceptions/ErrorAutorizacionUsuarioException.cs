namespace Application.Exceptions;

public class ErrorAutorizacionUsuarioException : ApiException
{
    public ErrorAutorizacionUsuarioException(string message) : base(message)
    {

    }
}