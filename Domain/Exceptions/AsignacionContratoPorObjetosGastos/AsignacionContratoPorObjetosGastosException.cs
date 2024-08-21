using Application.Exceptions;

namespace Domain.Exceptions.AsignacionContratoPorObjetosGastos;

public class AsignacionContratoPorObjetosGastosException : ApiException
{
    public AsignacionContratoPorObjetosGastosException(string message) : base(message) 
    {
    }
}
