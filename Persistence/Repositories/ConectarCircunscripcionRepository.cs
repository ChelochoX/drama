using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.Entities.ConectarCircunscripcion;
using Domain.Entities.Request;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories;

public class ConectarCircunscripcionRepository : IConectarCircunscripcionRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<ConectarCircunscripcionRepository> _logger;

    public ConectarCircunscripcionRepository(ILogger<ConectarCircunscripcionRepository> logger, DbConnections conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<ConectarCircunscripcion> ObtenerDatosparaConectarCircunscripcion(int codigoCircunscripcion)
    {
        _logger.LogInformation("Inicio del Proceso de obtener datos para conectar circunscripcion");

        string query = @"SELECT 
	                        cir.codigo_circunscripcion as CodigoCircunscripcion,
	                        cir.nombre_circunscripcion as NombreCircunscripcion,
                            cir.servidor as Servidor,
                            cir.base_datos as BasedeDatos

                        FROM circunscripciones cir
                        WHERE cir.codigo_circunscripcion_jurisdiccional = @codigoCircunscripcion";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoCircunscripcion", codigoCircunscripcion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {               
                var resultado = await connection.QueryFirstOrDefaultAsync<ConectarCircunscripcion>(query, parametros);
             
                _logger.LogInformation("Fin del Proceso de obtener datos para conectar circunscripcion");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConectarCircunscripcionExcelException("Ocurrió un error al obtener datos de la circunscripcion" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<string> ObtenerBaseDeDatosCircunscripcion(int codigoCircunscripcion)
    {
        _logger.LogInformation("Inicio del Proceso de obtener la base de datos de la circunscripcion");

        string query = @"SELECT 
                            cir.base_datos as BasedeDatos

                        FROM circunscripciones cir
                        WHERE cir.codigo_circunscripcion_jurisdiccional = @codigoCircunscripcion";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoCircunscripcion", codigoCircunscripcion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<string>(query, parametros);

                _logger.LogInformation("Fin del Proceso de obtener datos para conectar circunscripcion");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConectarCircunscripcionExcelException("Ocurrió un error al obtener datos de la circunscripcion" + "||-->" + ex.Message + "<--||");
        }
    }
}
