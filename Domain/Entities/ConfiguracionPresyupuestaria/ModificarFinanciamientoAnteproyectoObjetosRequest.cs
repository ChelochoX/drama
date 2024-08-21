using FluentValidation;

namespace Domain.Entities.Request;

public class ModificarFinanciamientoAnteproyectoObjetosRequest
{
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }

    public int CodigoAnteProyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int ConfiguracionPresupuestaria { get; set; }
}

