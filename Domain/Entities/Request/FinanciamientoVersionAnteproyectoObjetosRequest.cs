using FluentValidation;

namespace Domain.Entities.Request;

public class FinanciamientoVersionAnteproyectoObjetosRequest
{
    public int CodigoVersionAnteproyectos { get; set; }  
    public int? CodigoConfiguracionPresupuestaria { get; set; } 
   
}
