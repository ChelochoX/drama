using FluentValidation;

namespace Domain.Entities.Request.ConfiguracionPresupuestaria;

public class ModificarFuenteFinanciamientoyBienesRequest
{
    //Seccion de la Fuente de Financiamiento
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }

    public int CodigoAnteProyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int ConfiguracionPresupuestaria { get; set; }

    public int CodigoObjetoGasto { get; set; }
    public int Ejercicio { get; set; }
    public string? CedulaUsuario { get; set; }

    //Seccion de los Bienes
    public List<BienItem> Bienes { get; set; }

    public int CodigoMateria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }   
}
public class BienItem
{
    public int CodigoVersionAnteproyectoBienes { get; set; }
    public int NumeroBien { get; set; }
    public bool Seleccionado { get; set; }
    public int CantidadBien { get; set; }
}

    public class FuenteFinanciamientoyBienesValidator : AbstractValidator<ModificarFuenteFinanciamientoyBienesRequest>
{
    public FuenteFinanciamientoyBienesValidator()
    {
        RuleFor(x => x.CodigoFuenteFinanciamiento)
            .NotEmpty().WithMessage("El campo CodigoFuenteFinanciamiento no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo CodigoFuenteFinanciamiento debe ser mayor que cero");

        RuleFor(x => x.CodigoOrganismoFinanciador)
            .NotEmpty().WithMessage("El campo CodigoOrganismoFinanciador no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo CodigoOrganismoFinanciador debe ser mayor que cero");

        RuleFor(x => x.Evento)
            .GreaterThan(0).WithMessage("El campo MontoTotal debe ser mayor que cero");


        RuleFor(x => x.CodigoAnteProyectoObjeto)
            .NotEmpty().WithMessage("El campo Parametro_CodigoAnteProyectoObjeto no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoAnteProyectoObjeto debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo Parametro_CodigoVersion no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoVersion debe ser mayor que cero");

        RuleFor(x => x.ConfiguracionPresupuestaria)
            .NotEmpty().WithMessage("El campo Parametro_Config_Presupuestaria no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_Config_Presupuestaria debe ser mayor que cero");
    }
}