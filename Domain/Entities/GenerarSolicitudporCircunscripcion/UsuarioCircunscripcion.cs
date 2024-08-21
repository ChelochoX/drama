using System.Collections.Generic;

public class UsuarioCircunscripcion
{
    public List<Circunscripcion> Circunscripcion { get; set; }
    public List<CentroResponsabilidad> CentrosResponsabilidad { get; set; }
    public List<Materia> Materias { get; set; }
}

public class Circunscripcion
{
    public int CodigoCircunscripcion { get; set; }
    public string NombreCircunscripcion { get; set; }
}
public class CentroResponsabilidad
{
    public int CodigoCentroResponsabilidad { get; set; }
    public string DescripcionCentroResponsabilidad { get; set; }
}

public class Materia
{
    public int CodigoMateria { get; set; }
    public string DescripcionMateria { get; set; }
}
