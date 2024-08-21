using FluentValidation;
using System.Globalization;

namespace Domain.Entities.SincronizacionDatosCircunscripcion
{
    public class SincronizacionDatosRequest
    {
        public int? CodigoTarea { get; set; }
        public int? CodigoVersion { get; set; }
        public int? CodigoCircunscripcion { get; set; }
        public string? DescripcionCircunscripcion { get; set; }
        public string? DescripcionTarea { get; set; }
        public string? Estado { get; set; }
        public string? FechaInicio { get; set; }
        public string? UsuarioEjecucion { get; set; }
        public string? FechaFinalizacion { get; set; }

        public string? TerminoDeBusqueda { get; set; }
        public int ParamCodigoCircunscripcion { get; set; }
        public int Pagina { get; set; }
        public int CantidadRegistros { get; set; }   

    }

    public class SincronizacionDatosRequestValidator : AbstractValidator<SincronizacionDatosRequest>
    {
        public SincronizacionDatosRequestValidator()
        {       
            RuleFor(x => x.ParamCodigoCircunscripcion)
                .GreaterThan(0).WithMessage("El código de ParamCodigoCircunscripcion debe ser mayor que cero");

            RuleFor(x => x.Pagina)
                .GreaterThan(0).WithMessage("La página debe ser mayor que cero");

            RuleFor(x => x.CantidadRegistros)
                .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero");
        }
    }
}
