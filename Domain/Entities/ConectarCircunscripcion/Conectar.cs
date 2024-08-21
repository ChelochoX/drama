public class Conectar
{
    public Conectar()
    {
        
    }

    private IEnumerable<DatosConexion> Cadenas
    {
        get
        {
            //estas son las conexiones por base
            var cadenas = new List<DatosConexion>()
            {
                new DatosConexion(codCircunscripcion: 1, db: "administracion_pj", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 2, db: "administracion_cde", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 4, db: "administracion_con", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 5, db: "administracion_pjc", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 6, db: "administracion_vil", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 7, db: "administracion_cov", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 8, db: "administracion_pil", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 9, db: "administracion_mis", servidor: "SVR-FACTORYDB"),
                new DatosConexion(codCircunscripcion: 18, db: "administracion_ap", servidor: "SVR-FACTORYDB")
            };
            return cadenas;
        }
    }

    public DatosConexion ObtenerCadena(int codCircunscripcion)
    {
        var cadena = Cadenas.SingleOrDefault(e => e.CodCircunscripcion == codCircunscripcion);

        if (cadena is null)
        {
            throw new Exception("No existe una cadena para ese código de circunscripción.");
        }

        return cadena;
    }
}

public class DatosConexion
{
    public long CodCircunscripcion { get; }
    public string Db { get; }
    public string Servidor { get; }

    public DatosConexion(long codCircunscripcion, string db, string servidor)
    {
        this.CodCircunscripcion = codCircunscripcion;
        this.Db = db;
        this.Servidor = servidor;
    }
}