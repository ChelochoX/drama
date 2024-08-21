using Application.Exceptions;

public class VersionadoAnteproyectoException : ApiException
{
    public VersionadoAnteproyectoException(string message) : base(message) 
    {
    }
}
