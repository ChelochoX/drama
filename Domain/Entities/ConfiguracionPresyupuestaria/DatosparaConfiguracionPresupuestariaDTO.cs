namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class DatosparaConfiguracionPresupuestariaDTO
{
    //Seccion Anteproyecto
    public int UsuarioInserto { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int Ejercicio { get; set; }
    public string DescripcionCircunscripcion { get; set; }

    //Seccion Configuracion Presupuestaria
    public int CodigoObjetoGasto { get; set; }
    public string NumeroObjetoGasto { get; set; }
    public string DescripcionObjetoGasto { get; set; }
    public int CodigoPrograma { get; set; }
    public string NumeroPrograma { get; set; }
    public string DescripcionPrograma { get; set; }
    public int CodigoActividad { get; set; }
    public string NumeroActividad { get; set; }
    public string DescripcionActividad { get; set; }
    public int CodigoTipoPresupuesto { get; set; }
    public string NumeroTipoPresupuesto { get; set; }
    public string DescripcionTipoPresupuesto { get; set; }
    public int CodigoDepartamento { get; set; }
    public string NumeroDepartamento { get; set; }
    public string DescripcionDepartamento { get; set; }
    public int Grupo { get; set; }
    public int CodigoGrupo { get; set; }
    public string DescripcionGrupo { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public string DescripcionCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public string DescripcionMateria { get; set; }
    public int SubGrupo { get; set; }
    public int CodigoSubGrupo { get; set; }
    public string DescripcionSubGrupo { get; set; }

}
