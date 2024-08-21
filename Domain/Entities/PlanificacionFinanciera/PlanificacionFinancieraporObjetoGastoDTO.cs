namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionFinancieraporObjetoGastoDTO
{
    public int CodigoAnteproyectoObjeto { get; set; }
    public string Actividad { get; set; }
    public int Ejercicio { get; set; }
    public string ObjetoGasto { get; set; }
    public int CodigoFF { get; set; }
    public int FuenteFinanciamiento { get; set; }
    public int CodigoOG { get; set; }
    public int OrganismoFinanciador { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public string CentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public string Materia { get; set; }
    public int Evento { get; set; }
    public decimal SumaValorTotal { get; set; }

}
