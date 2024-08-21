using Domain.Entities.Request;
using FluentValidation;

namespace Domain.Entities.Request;

public class DatosAnteproyectoObjetoparaAnteproyectoBienesRequest
{
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public string UsuarioInserto { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int Ejercicio { get; set; }
    public List<ItemBien> Bienes { get; set; }
}

public class ItemBien
{
    public string NumeroBien { get; set; }
}

public class DatosFinanciamientoConlosBienesValidator : AbstractValidator<DatosAnteproyectoObjetoparaAnteproyectoBienesRequest>
{
    public DatosFinanciamientoConlosBienesValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjeto)
               .NotEmpty().WithMessage("El campo CodigoAnteproyectoObjeto no puede estar vacío")
               .GreaterThan(0).WithMessage("El campo CodigoAnteproyectoObjeto debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo CodigoVersion no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo CodigoVersion debe ser mayor que cero");
    }   
}