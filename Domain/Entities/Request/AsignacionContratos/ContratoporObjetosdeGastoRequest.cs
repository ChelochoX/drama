using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request.AsignacionContratos;

public class ContratoporObjetosdeGastoRequest
{
    public int CodigoAnteproyectoContrato { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoContrato { get; set; }
    public int? MontoContrato { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int UsuarioModificacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}

public class ContratoporObjetosdeGastoRequestValidator : AbstractValidator<ContratoporObjetosdeGastoRequest>
{
    public ContratoporObjetosdeGastoRequestValidator()
    {

        RuleFor(x => x.CodigoAnteproyectoContrato)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El código de anteproyecto del contrato debe ser mayor que cero.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El código de anteproyecto del objeto debe ser mayor que cero.");

        RuleFor(x => x.CodigoVersion)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El código de version debe ser mayor que cero.");

    }
}