using FluentValidation;

namespace Domain.Entities.Request;

public class BienesparaConfiguracionPresupuestariaRequest
{
    public int? NumeroBien { get; set; }
    public string? DescripcionBien { get; set; }
    public string? CostoUnitario { get; set; }
    public string? Cantidad { get; set; }
    public string? MontoTotal { get; set; }
    public string? TerminoDeBusqueda { get; set; }


    public int CodigoVersion { get; set; }
    public int CodigoVersionOBG { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int Ejercicio { get; set; }   
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }

    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class BienesparaConfiguracionPresupuestariaValidator : AbstractValidator<BienesparaConfiguracionPresupuestariaRequest>
{
    public BienesparaConfiguracionPresupuestariaValidator()
    {    
       
    }
}
