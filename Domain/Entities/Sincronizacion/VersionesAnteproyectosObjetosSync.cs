using System.Numerics;

namespace Domain.Entities.Sincronizacion;

public class VersionesAnteproyectosObjetosSync
{
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }
    public decimal PresupuestoInicial { get; set; }
    public decimal Modificaciones { get; set; }
    public decimal MontoPlanificado { get; set; }
    public int CodigoCircunscripcionOrigen { get; set; }
    public int CodigoAOOrigen { get; set; }
    public int CodigoCCPOrigen { get; set; }
    public int CodigoVersionOrigen { get; set; }
}
