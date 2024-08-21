using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class BienesdeAnteproyectoBienesConsolidadoRequest
{
    public int CodigoVersion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }    
}
public class BienesdeAnteproyectoBienesConsolidadoRequestValidator : AbstractValidator<BienesdeAnteproyectoBienesConsolidadoRequest>
{
    public BienesdeAnteproyectoBienesConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoVersion)
            .GreaterThan(0).WithMessage("El CodigoVersion debe ser mayor a 0.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .GreaterThan(0).WithMessage("El CodigoAnteproyectoObjeto debe ser mayor a 0.");              
    }
}

