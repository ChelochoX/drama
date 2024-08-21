namespace Domain.Entities.Anteproyecto;

public class ClaseConfiguracionPresupuestaria
{
    // Tabla configuracion_presupuestaria
    public int CodigoConfiguracionPresupuestaria { get; set; }    
    public int CodigoObjetoGasto { get; set; }
    public int CodigoPrograma { get; set; }
    public int CodigoSubPrograma { get; set; }
    public int CodigoActividad { get; set; }
    public int CodigoTipoPresupuesto { get; set; }
    public int CodigoDepartamento { get; set; }
    public int CodigoGrupo { get; set; }
    public int CodigoSubGrupo { get; set; }
    public int CodigoCentroResponsabilidad	{ get; set; }
    public int CodigoMateria { get; set; }
    public int CodigoCircunscripcionOrigen { get; set; }
    

    // Tabla versiones_anteproyectos_objetos
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }
    public Int64 PresupuestoInicial { get; set; }
    public Int64 MontoPlanificado { get; set; }
    public Int64 Modificaciones { get; set; }
    public int UsuarioInserto { get; set; }

    public int Ejercicio { get; set; }


    // Tabla versiones_anteproyectos_objetos
    public int CodigoAnteproyectoBien { get; set; }
    public int NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public int ValorUnitario { get; set; }
   //public DateTime Fechainserto { get; set; }
    //public DateTime FechaModificacion { get; set; }    
    public string? Fundamentacion { get; set; }
    public int UsuarioModificacion { get; set; }
    public int UnidadMedida { get; set; }

}
