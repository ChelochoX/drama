namespace Domain.Entities.Request.ConfiguracionPresupuestaria;

public class DatosparaAnteproyectoObjetosyBienesRequest
{
    // Seccion para Anteproyecto Objetos
    public int CodigoVersionAnteproyecto { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }

    // Seccion para Anteproyecto de Bienes   
    public string? CedulaUsuario { get; set; }
    public int Ejercicio { get; set; }
    public List<ItemBien> Bienes { get; set; }

    // Bandera Modificar
    public bool BanderaModificarVersionesBienes { get; set; }
    public int CodigoAnteproyectoOBJ { get; set; }
}