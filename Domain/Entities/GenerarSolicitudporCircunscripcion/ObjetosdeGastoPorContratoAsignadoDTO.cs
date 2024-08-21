namespace Domain.DTOs;

public class ObjetosdeGastoPorContratoAsignadoDTO
{
    public int NumeroObjetoGasto { get; set; }
    public int NumeroFuenteFinanciamiento { get; set; }
    public int NumeroOrganismoFinanciador { get; set; }
    public int NumeroDepartamento { get; set; }
    public string Fundamentacion { get; set; }
    public string CentroResponsabilidad { get; set; }
    public string DescripcionMateria { get; set; }
    public double MontoObjetoGasto { get; set; }
    public double Saldo { get; set; }
    public double TotalContrato { get; set; }
}
