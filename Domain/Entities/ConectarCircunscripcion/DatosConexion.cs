namespace Domain.Entities.ConectarCircunscripcion;

public class DatosConexion
{
    private static DatosConexion _instance;
    private static readonly object _lock = new object();

    // Propiedades para almacenar los datos necesarios
    public int? CodigoCircunscripcion { get; private set; }
    public string? NombreCircunscripcion { get; private set; }
    public string? Servidor { get; private set; }
    public string? BasedeDatos { get; private set; }

    private DatosConexion() { }

    public static DatosConexion Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new DatosConexion();
                }
                return _instance;
            }
        }
    }

    public void Initialize(int codigoCircunscripcion, string nombreCircunscripcion, string servidor, string bbdd)
    {
        CodigoCircunscripcion = codigoCircunscripcion;
        NombreCircunscripcion = nombreCircunscripcion;
        Servidor = servidor;
        BasedeDatos = bbdd;
    }

    public void Reset()
    {
        CodigoCircunscripcion = null;
        NombreCircunscripcion = null;
        Servidor = null;
        BasedeDatos = null;
    }
}
