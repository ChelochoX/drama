namespace Domain.Entities.AsignacionContratos;

public class ObjetosdeGastoPorContratoAsignado
{
    public int NumeroObjetoGasto { get; set; }
    public int NumeroFuenteFinanciamiento { get; set; }
    public int NumeroOrganismoFinanciador { get; set; }
    public int NumeroDepartamento { get; set; }
    public string Fundamentacion { get; set; }
    public int? MontoObjetoGasto { get; set; }
    public int? Saldo { get; set; }

}
