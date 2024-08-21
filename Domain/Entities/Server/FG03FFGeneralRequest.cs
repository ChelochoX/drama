namespace Domain.Entities.Server;

public class FG03FFGeneralRequest
{
    public int? CodigoFuenteFinanciamiento { get; set; }
    public string Usuario { get; set; }
    public int Ejercicio { get; set; }
    public int CodigoCircunscripcion { get; set; }
}
