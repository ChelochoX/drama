using Application.Exceptions;

namespace Domain.Exceptions.AsignacionContratoPorObjetosGastos;

public class ConsolidadoAnteproyectodePresupuestosException : ApiException
{
    public ConsolidadoAnteproyectodePresupuestosException(string message) : base(message) 
    {
    }
}
