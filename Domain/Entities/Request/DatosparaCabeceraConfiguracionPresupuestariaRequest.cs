namespace Domain.Entities.Request;

public class DatosparaCabeceraConfiguracionPresupuestariaRequest
{
    //Seccion Anteproyecto
    public int UsuarioInserto { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int Ejercicio { get; set; }

    //Seccion Configuracion Presupuestaria
    public int CodigoObjetoGasto { get; set; }     
    public int CodigoPrograma { get; set; }  
    public int CodigoActividad { get; set; }    
    public int CodigoTipoPresupuesto { get; set; }   
    public int CodigoDepartamento { get; set; }    
    public int Grupo { get; set; }   
    public int CodigoCentroResponsabilidad { get; set; }   
    public int CodigoMateria { get; set; }    
    public int SubGrupo { get; set; }   
}
