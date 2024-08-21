using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.Entities.Referencias;
using Domain.Entities.Request;
using Domain.Exceptions.Referencias;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;

namespace Persistence.Repositories
{
    public class ReferenciasRepository : IReferenciasRepository
    {
        private readonly DbConnections _conexion;
        private readonly ILogger<ReferenciasRepository> _logger;

        public ReferenciasRepository(DbConnections conexion, ILogger<ReferenciasRepository> logger)
        {
            _conexion = conexion;
            _logger = logger;
        }
            public async Task<Datos<IEnumerable<TipoReferenciaDTO>>> ObtenerTiposReferencias(TipoReferenciasRequest request)
            {
            
                _logger.LogInformation("Inicio de Proceso de obtener listado de tipos de referencia");
               
                int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

                
                string query = @"
                SELECT 
                    codigo_tipo_referencia as CodigoTipoReferencia,
                    descripcion_tipo_referencia as DescripcionTipoReferencia,
                    dominio_tipo_referencia as DominioTipoReferencia,
                    estado
                FROM 
                    tipo_referencias
                WHERE 1 = 1";

            if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.DescripcionTipoReferencia) ||
                        !string.IsNullOrEmpty(request.DominioTipoReferencia) ||
                        !string.IsNullOrEmpty(request.Estado)
                        )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    query += @"
                                    AND (
                                        descripcion_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR dominio_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
	                                    OR estado = CASE 
                                                          WHEN 'INACTIVO' LIKE @terminoDeBusqueda + '%' 
                                                              AND 'ACTIVO' NOT LIKE @terminoDeBusqueda + '%' THEN 2
                                                          WHEN 'ACTIVO' LIKE '%' + @terminoDeBusqueda + '%' THEN 1
                                                      END
                                    )";
                }
                else
                {
                    query += @"
                                    AND (
                                        (@parametroDescripcionTipoReferencia IS NULL OR descripcion_tipo_referencia LIKE '%' + @parametroDescripcionTipoReferencia + '%')
                                        AND (@parametroDominioTipoReferencia IS NULL OR dominio_tipo_referencia LIKE '%' + @parametroDominioTipoReferencia + '%')
                                        AND (@parametroEstado IS NULL OR (estado = CASE 
                                                       WHEN 'ACTIVO' LIKE '%' + @parametroEstado + '%' THEN 1 
                                                       WHEN 'INACTIVO' LIKE '%' + @parametroEstado + '%' THEN 2
                                                   END))
                                    )";
                }
            }
            else
            {
                query += @"
                               AND (estado = 1) ";
            }
            query += @"
                    ORDER BY codigo_tipo_referencia
                    OFFSET @saltarRegistros ROWS
                    FETCH NEXT @cantidadRegistros ROWS ONLY";

            string queryCantidadTotalRegistros = @"
                SELECT 
                    COUNT(*)
                FROM 
                    tipo_referencias
                WHERE 1 = 1";

            if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.DescripcionTipoReferencia) ||
                        !string.IsNullOrEmpty(request.DominioTipoReferencia) ||
                        !string.IsNullOrEmpty(request.Estado)
                        )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    queryCantidadTotalRegistros += @"
                                    AND (
                                        descripcion_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR dominio_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
	                                    OR estado = CASE 
                                                          WHEN 'INACTIVO' LIKE @terminoDeBusqueda + '%' 
                                                              AND 'ACTIVO' NOT LIKE @terminoDeBusqueda + '%' THEN 2
                                                          WHEN 'ACTIVO' LIKE '%' + @terminoDeBusqueda + '%' THEN 1
                                                      END
                                    )";
                }
                else
                {
                    queryCantidadTotalRegistros += @"
                                    AND (
                                        (@parametroDescripcionTipoReferencia IS NULL OR descripcion_tipo_referencia LIKE '%' + @parametroDescripcionTipoReferencia + '%')
                                        AND (@parametroDominioTipoReferencia IS NULL OR dominio_tipo_referencia LIKE '%' + @parametroDominioTipoReferencia + '%')
                                        AND (@parametroEstado IS NULL OR (estado = CASE 
                                                       WHEN 'ACTIVO' LIKE '%' + @parametroEstado + '%' THEN 1 
                                                       WHEN 'INACTIVO' LIKE '%' + @parametroEstado + '%' THEN 2 
                                                   END))
                                    )";
                }
            }
            else
            {
                queryCantidadTotalRegistros += @"
                               AND (estado = 1) ";
            }

            var parametros = new DynamicParameters();
            parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);

            parametros.Add("@parametroDescripcionTipoReferencia", request.DescripcionTipoReferencia);
            parametros.Add("@parametroDominioTipoReferencia", request.DominioTipoReferencia);
            parametros.Add("@parametroEstado", request.Estado);

            try
                {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<TipoReferenciaDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<TipoReferenciaDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener listado de referencias");

                    return listado;
                }
            }
            catch (Exception ex)
                {
                    throw new ReferenciasException("Ocurrió un error al obtener el listado de tipos de referencia. Detalles: " + ex.Message);
                }
            }
            public async Task<List<TipoReferenciaDTO>> ObtenerDescripcionTiposReferencias(string? descripcionTipoReferencia, int codigoTipoReferencia, int estado)
            { _logger.LogInformation("Inicio de Proceso de obtener descripcion de tipos de referencia");
                string query = @"
                    SELECT 
                        codigo_tipo_referencia as CodigoTipoReferencia,
                        descripcion_tipo_referencia as DescripcionTipoReferencia,
                        estado
                    FROM 
                        tipo_referencias
                    WHERE 1 = 1";
                if (!string.IsNullOrEmpty(descripcionTipoReferencia))
                {
                    query += " AND UPPER(descripcion_tipo_referencia) = UPPER(@descripcionTipoReferencia)";
                }
 
                string estadoVacio = "";
                string codigoVacio = "";
                if (estado == 0)
                {
                    estadoVacio = "1";
                }
                else
                {
                    estadoVacio = estado.ToString();
                }

                if (estadoVacio != "")
                {
                    query += " AND estado = @estado";
                }
                if(codigoTipoReferencia == 0)
                {
                    codigoVacio = "";
                }
                else
                { 
                    codigoVacio = codigoTipoReferencia.ToString();
                }
                if (codigoVacio != "")
                {
                    query += " AND codigo_tipo_referencia = @codigoTipoReferencia";
                }
                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {

                        var parametros = new { descripcionTipoReferencia,codigoTipoReferencia = codigoVacio, estado = estadoVacio};

                        var resultados = await connection.QueryAsync<TipoReferenciaDTO>(query, parametros);
                        var lista = resultados.ToList();

                        _logger.LogInformation("Fin de Proceso de obtener descripciones de tipos de referencia");

                        return lista;
                    }
                }
                catch (Exception ex)
                {
                    throw new ReferenciasException("Ocurrió un error al obtener descripciones de tipos de referencia. Detalles: " + ex.Message);
                }
            }
            public async Task<List<TipoReferenciaDTO>> ObtenerDominiosTiposReferencias(string? dominioTipoReferencia,int codigoTipoReferencia, int estado)
            {
                _logger.LogInformation("Inicio de Proceso de obtener descripcion de tipos de referencia");
                string query = @"
                        SELECT 
                            codigo_tipo_referencia,
                            dominio_tipo_referencia,
                            estado
                        FROM 
                            tipo_referencias
                        WHERE 1 = 1";
                if (!string.IsNullOrEmpty(dominioTipoReferencia))
                {
                    query += " AND UPPER(dominio_tipo_referencia) = UPPER(@descripcionTipoReferencia)";
                }

                string estadoVacio = "";
                string codigoVacio = "";

                if (estado == 0)
                {
                    estadoVacio = "1";
                }
                else
                {
                    estadoVacio = estado.ToString();
                }

                if (estadoVacio != "")
                {
                    query += " AND estado = @estado";
                }
                if (codigoTipoReferencia == 0)
                {
                    codigoVacio = "";
                }
                if (codigoVacio != "")
                {
                    query += " AND codigo_tipo_referencia = @codigoTipoReferencia";
                }
                else
                {
                    codigoVacio = codigoTipoReferencia.ToString();
                }
                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {

                        var parametros = new {dominioTipoReferencia,codigoTipoReferencia = codigoVacio, estado = estadoVacio };

                        var resultados = await connection.QueryAsync<TipoReferenciaDTO>(query, parametros);
                        var lista = resultados.ToList();

                        _logger.LogInformation("Fin de Proceso de obtener descripciones de tipos de referencia");

                        return lista;
                    }
                }
                catch (Exception ex)
                {
                    throw new ReferenciasException("Ocurrió un error al obtener descripciones de tipos de referencia. Detalles: " + ex.Message);
                }
            }

        public async Task<List<ReferenciasDTO>> ObtenerReferencias(int pagina, int cantidadRegistros, string? dominioReferencia, int codigoTipoReferencia,
                                                            string? descripcionReferencia, int estado, DateTime valorFecha, string valorAlfanumerico,
                                                            string descripcionLarga, decimal? valorDecimal)
        {
            _logger.LogInformation("Inicio de Proceso de obtener listado de referencias");

            int saltarRegistros = (pagina - 1) * cantidadRegistros;

            string query = @"
                            SELECT 
                                r.Codigo_Referencia as CodigoReferencia,
                                r.Dominio_Referencia as DominioReferencia,
                                r.Codigo_Tipo_Referencia as CodigoTipoReferencia,
                                r.Descripcion_Referencia as DescripcionReferencia,
                                r.Estado,
                                r.Valor_Fecha as ValorFecha,
                                r.Valor_Alfanumerico as ValorAlfanumerico,
                                r.Descripcion_Larga as DescripcionLarga,
                                r.Valor_Decimal as ValorDecimal,
                                t.descripcion_tipo_referencia as DescripcionTipoReferencia
                            FROM 
                                referencias r
                            JOIN
                                tipo_referencias t ON r.Codigo_Tipo_Referencia = t.codigo_tipo_referencia
                            WHERE 1 = 1";

            if (!string.IsNullOrEmpty(dominioReferencia))
            {
                query += " AND UPPER(r.Dominio_Referencia) = UPPER(@dominioReferencia)";
            }
            if (!string.IsNullOrEmpty(descripcionReferencia))
            {
                query += " AND UPPER(r.Descripcion_Referencia) = UPPER(@descripcionReferencia)";
            }
            if (!string.IsNullOrEmpty(valorAlfanumerico))
            {
                query += " AND UPPER(r.Valor_Alfanumerico) = UPPER(@valorAlfanumerico)";
            }
            if (!string.IsNullOrEmpty(descripcionLarga))
            {
                query += " AND UPPER(r.Descripcion_Larga) = UPPER(@descripcionLarga)";
            }
            if (valorFecha != DateTime.MinValue)
            {
                string fechaFormateada = valorFecha.ToString("yyyy-MM-dd");
                query += $" AND r.Valor_Fecha = '{fechaFormateada}'";
            }
            if (codigoTipoReferencia != 0)
            {
                query += " AND r.Codigo_Tipo_Referencia = @codigoTipoReferencia";
            }
            if (estado != 0)
            {
                query += " AND r.Estado = @estado";
            }
            if (valorDecimal.HasValue && valorDecimal != 0)
            {
                query += " AND r.Valor_Decimal = @valorDecimal";
            }

                                query += @"
                            ORDER BY Codigo_Referencia
                            OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        pagina,
                        cantidadRegistros,
                        dominioReferencia,
                        codigoTipoReferencia,
                        descripcionReferencia,
                        estado,
                        valorFecha,
                        valorAlfanumerico,
                        descripcionLarga,
                        valorDecimal,
                        saltarRegistros
                    };

                    var resultados = await connection.QueryAsync<ReferenciasDTO>(query, parametros);
                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener listado de referencias");

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener el listado de tipos de referencia. Detalles: " + ex.Message);
            }
        }
        public async Task<List<ReferenciasDTO>> ObtenerDominioReferencias(string? dominioReferencia, int codigoReferencia,
                                                                          int estado)
        {
            _logger.LogInformation("Inicio de Proceso de obtener dominios de referencias");
            string query = @"
                SELECT 
                    Codigo_Referencia as CodigoReferencia,
                    Dominio_Referencia as DominioReferencia,
                    Estado, descripcion_referencia as DescripcionTipoReferencia
                FROM 
                    referencias
                WHERE 1 = 1";
            //if (!string.IsNullOrEmpty(descripcionTipoReferencia))
            //{
            //    query += " AND UPPER(descripcion_referencia) LIKE '%' + @UPPER(@descripcionTipoReferencia) + '%'  ";
            //}

            if (!string.IsNullOrEmpty(dominioReferencia))
            {
                query += " AND UPPER(Dominio_Referencia) = UPPER(@dominioReferencia)";
            }
            string estadoVacio = "";
            string codigoVacio = "";

            if (estado == 0)
            {
                estadoVacio = "1";
            }
            else
            {
                estadoVacio = estado.ToString();
            }

            if (estadoVacio != "")
            {
                query += " AND estado = @estado";
            }
            if (codigoReferencia == 0)
            {
                codigoVacio = "";
            }
            if (codigoVacio != "")
            {
                query += " AND CodigoReferencia = @codigoReferencia";
            }
            else
            {
                codigoVacio = codigoReferencia.ToString();
            }
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var parametros = new { dominioReferencia, codigoReferencia = codigoVacio, estado = estadoVacio };

                    var resultados = await connection.QueryAsync<ReferenciasDTO>(query, parametros);
                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener descripciones de tipos de referencia");

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener descripciones de tipos de referencia. Detalles: " + ex.Message);
            }
        }
        public async Task<List<ReferenciasDTO>> ObtenerDescripcionReferencias(string? descripcionReferencia, int codigoReferencia,
                                                                          int estado)
        {
            _logger.LogInformation("Inicio de Proceso de obtener dominios de referencias");
            string query = @"
                SELECT 
                    Codigo_Referencia as CodigoReferencia,
                    Descripcion_Referencia as DescripcionReferencia,
                    Estado
                FROM 
                    referencias
                WHERE 1 = 1";
            if (!string.IsNullOrEmpty(descripcionReferencia))
            {
                query += " AND UPPER(Descripcion_Referencia) = UPPER(@descripcionReferencia)";
            }
            string estadoVacio = "";
            string codigoVacio = "";

            if (estado == 0)
            {
                estadoVacio = "1";
            }
            else
            {
                estadoVacio = estado.ToString();
            }

            if (estadoVacio != "")
            {
                query += " AND estado = @estado";
            }
            if (codigoReferencia == 0)
            {
                codigoVacio = "";
            }
            if (codigoVacio != "")
            {
                query += " AND CodigoReferencia = @codigoReferencia";
            }
            else
            {
                codigoVacio = codigoReferencia.ToString();
            }
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var parametros = new { descripcionReferencia, codigoReferencia = codigoVacio, estado = estadoVacio };

                    var resultados = await connection.QueryAsync<ReferenciasDTO>(query, parametros);
                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener descripciones de tipos de referencia");

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener descripciones de tipos de referencia. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<int>> InsertarTipoReferencia(TipoReferencias tipoReferencia)
        {
            _logger.LogInformation("Inicio de Proceso de insertar tipo de referencia {@tipo_referencias}", tipoReferencia);

            string queryUltimoValorCodigo = "SELECT ISNULL(MAX(codigo_tipo_referencia),0) FROM tipo_referencias";

            string query = @"
                            INSERT INTO tipo_referencias
                            (codigo_tipo_referencia,descripcion_tipo_referencia, dominio_tipo_referencia, estado, usuario_inserto, fecha_inserto)
                            VALUES
                            (@codigoTipoReferencia,@descripcionTipoReferencia, @dominioTipoReferencia, @estado, @usuarioInserto, GETDATE())";

            string validarExiste = @"SELECT COUNT(*) FROM tipo_referencias WHERE dominio_tipo_referencia = @dominioTipoReferencia ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int existe = await connection.ExecuteScalarAsync<int>(validarExiste, tipoReferencia);
                    int nuevoCodigoTipoReferencia = 0;
                    int totalRegistro = 0;

                    if (existe == 0)
                    {
                        int ultimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                        nuevoCodigoTipoReferencia = ultimoValorCodigo + 1;

                        tipoReferencia.CodigoTipoReferencia = nuevoCodigoTipoReferencia;


                        var resultado = await connection.ExecuteAsync(query, tipoReferencia);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }

                    var listado = new Datos<int>
                    {
                        Items = nuevoCodigoTipoReferencia,
                        TotalRegistros = totalRegistro
                    };

                    _logger.LogInformation("Fin de Proceso de insertar tipo de referencia {@tipoReferencia}", tipoReferencia);
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrio un error al insertar el tipo de refencia" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<int>> InsertarReferencia(Referencias referencia)
        {
            _logger.LogInformation("Inicio de Proceso de insertar tipo de referencia {@referencia}", referencia);

            string queryUltimoValorCodigo = "SELECT ISNULL(MAX(codigo_referencia),0) FROM referencias";

            string query = @"
                            INSERT INTO referencias
                            (codigo_referencia,dominio_referencia, codigo_tipo_referencia, descripcion_referencia, estado, valor_fecha, valor_alfanumerico, descripcion_larga, valor_decimal, fecha_inserto, usuario_inserto)
                            VALUES
                            (@codigoReferencia,@dominioReferencia, @codigoTipoReferencia, @descripcionReferencia, @estado, @valorFecha, @valorAlfanumerico, @descripcionLarga, @valorDecimal, GETDATE(), @usuarioInserto)";

            string validarExiste = @"SELECT COUNT(*) FROM referencias WHERE dominio_referencia = @dominioReferencia ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int existe = await connection.ExecuteScalarAsync<int>(validarExiste, referencia);
                    int nuevoCodigoReferencia = 0;
                    int totalRegistro = 0;

                    if (existe == 0)
                    {
                        int ultimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                        nuevoCodigoReferencia = ultimoValorCodigo + 1;

                        referencia.CodigoReferencia = nuevoCodigoReferencia;


                        var resultado = await connection.ExecuteAsync(query, referencia);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }

                    var listado = new Datos<int>
                    {
                        Items = nuevoCodigoReferencia,
                        TotalRegistros = totalRegistro
                    };

                    _logger.LogInformation("Fin de Proceso de insertar referencia {@referencia}", referencia);
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrio un error al insertar refencia" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<int>> ModificarTipoReferencia(TipoReferencias tipoReferencia)
        {
            _logger.LogInformation("Inicio del Proceso de modificar el tipo de referencia {@tipoReferencia}", tipoReferencia);

            string query = @"UPDATE tipo_referencias 
            SET descripcion_tipo_referencia = @DescripcionTipoReferencia, 
                dominio_tipo_referencia = @DominioTipoReferencia, 
                estado = @estado, 
                usuario_modificacion = @UsuarioModificacion, 
                fecha_modificacion = GETDATE() 
            WHERE codigo_tipo_referencia = @CodigoTipoReferencia";

            string validarExiste = @"SELECT COUNT(*) FROM tipo_referencias WHERE dominio_tipo_referencia = @dominioTipoReferencia ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int existe = await connection.ExecuteScalarAsync<int>(validarExiste, tipoReferencia);
                    int codigoTipoReferencia = 0;
                    int totalRegistro = 0;

                    if (existe == 0)
                    {
                        var resultado = await connection.ExecuteAsync(query, tipoReferencia);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }

                    var listado = new Datos<int>
                    {
                        Items = codigoTipoReferencia,
                        TotalRegistros = totalRegistro
                    };

                    _logger.LogInformation("Fin del Proceso de modificar tipo de referencia {@tipoReferencia}", tipoReferencia);
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al modificar los datos en la tabla tipo de referencias" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<int>> ModificarReferencia(Referencias referencia)
        {
            _logger.LogInformation("Inicio del Proceso de modificar referencia {@referencia}", referencia);

            string query = @"UPDATE referencias 
            SET dominio_referencia = @DominioReferencia, 
                codigo_tipo_referencia = @CodigoTipoReferencia, 
                descripcion_referencia = @DescripcionReferencia, 
                estado = @Estado, 
                valor_fecha = @ValorFecha, 
                valor_alfanumerico = @ValorAlfanumerico, 
                descripcion_larga = @DescripcionLarga, 
                valor_decimal = @ValorDecimal, 
                fecha_modificacion = GETDATE(), 
                usuario_modificacion = @UsuarioModificacion 
            WHERE codigo_referencia = @CodigoReferencia ";

            string validarExiste = @"SELECT COUNT(*) FROM referencias WHERE dominio_referencia = @dominioReferencia ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    int existe = await connection.ExecuteScalarAsync<int>(validarExiste, referencia);
                    int codigoReferencia = referencia.CodigoReferencia;
                    int totalRegistro = 0;

                    if (existe == 0)
                    {
                        var resultado = await connection.ExecuteAsync(query, referencia);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }

                    var listado = new Datos<int>
                    {
                        Items = codigoReferencia,
                        TotalRegistros = totalRegistro
                    };

                    _logger.LogInformation("Fin del Proceso de modificar la referencia {@referencia}", referencia);
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al modificar los datos en la tabla referencias" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<List<TipoReferenciaDTO>> ObtenerTipoReferenciaDescripcion(string dominioTipoReferencia, int? codigoTipoReferencia = null)
        {
            _logger.LogInformation("Inicio de Proceso de obtener tipo de referencia por dominio: {dominio}", dominioTipoReferencia);

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string query = @"
                SELECT 
                    codigo_tipo_referencia,
                    descripcion_tipo_referencia,
                    dominio_tipo_referencia,
                    estado
                FROM 
                    tipo_referencias
                WHERE 
                    UPPER(dominio_tipo_referencia) = UPPER(@dominioTipoReferencia)";

                    if (codigoTipoReferencia.HasValue)
                    {
                        query += " AND codigo_tipo_referencia != @codigoTipoReferencia";
                    }

                    var parametros = new { dominioTipoReferencia, codigoTipoReferencia };

                    var resultados = await connection.QueryAsync<TipoReferenciaDTO>(query, parametros);
                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener tipo de referencia {@dominio}", dominioTipoReferencia);

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener tipo de referencia por descripción. Detalles: " + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<List<TipoReferenciaDTO>> ObtenerReferenciaDominio(string dominioReferencia, int? codigoReferencia = null)
        {
            _logger.LogInformation("Inicio de Proceso de obtener tipo de referencia por descripción: {dominio}", dominioReferencia);

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string query = @"
                SELECT 
                    codigo_referencia,
                    dominio_referencia,
                    codigo_tipo_referencia,
                    descripcion_referencia,
                    estado,
                    valor_fecha,
                    valor_alfanumerico,
                    descripcion_larga,
                    valor_decimal
                FROM 
                    referencias
                WHERE 
                    UPPER(dominio_referencia) = UPPER(@dominioReferencia)";

                    if (codigoReferencia.HasValue)
                    {
                        query += " AND codigo_referencia != @codigoReferencia";
                    }

                    var parametros = new { dominioReferencia, codigoReferencia };

                    var resultados = await connection.QueryAsync<TipoReferenciaDTO>(query, parametros);
                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener tipo de referencia {@dominio}", dominioReferencia);

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener referencia por dominio. Detalles: " + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<List<TipoReferenciaDTO>> ObtenerTipoReferenciaCodigo(int codigoTipoReferencia)
        {
            _logger.LogInformation("Inicio de Proceso de obtener listado de tipos de referencia");
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string query = @"
                    SELECT 
                    codigo_tipo_referencia,
                    descripcion_tipo_referencia,
                    dominio_tipo_referencia,
                    estado
                    FROM 
                    tipo_referencias
                    WHERE 
                    codigo_tipo_referencia = @codigoTipoReferencia)";

                    var resultados = await connection.QueryAsync<TipoReferenciaDTO>(query, new { codigoTipoReferencia });

                    var lista = resultados.ToList();

                    _logger.LogInformation("Fin de Proceso de obtener tipo de referencia por codigoTipoReferencia");

                    return lista;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener tipo de referencia por codigoTipoReferencia. Detalles: " + ex.Message);
            }
        }
        public async Task<string> ObtenerNombreUsuario(string cedula)
        {
            _logger.LogInformation("Inicio de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);

            string queryUsuarioSesion = "SELECT username FROM usuarios_poder_judicial WHERE cedula_identidad LIKE '%' + @cedula + '%'";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        cedula = $"%{cedula.Trim()}%"
                    };

                    var username = await connection.QuerySingleOrDefaultAsync<string>(
                      queryUsuarioSesion, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);


                    return username;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrio un error al obtener el nombre de usuario de la tabla usuarios_poder_judicial" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ObtenerCodigoUsuario(string cedula)
        {
            _logger.LogInformation("Inicio de Proceso de obtener el codigo del usuario con cedula {@cedula}", cedula);

            string queryUsuarioSesion = "SELECT codigo_usuario FROM usuarios_poder_judicial WHERE cedula_identidad LIKE '%' + @cedula + '%'";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        cedula = $"%{cedula.ToString().Trim()}%"
                    };

                    var username = await connection.QuerySingleOrDefaultAsync<int>(
                      queryUsuarioSesion, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener el codigo del usuario con cedula {@cedula}", cedula);


                    return username;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrio un error al obtener el codigo de usuario de la tabla usuarios_poder_judicial" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<IEnumerable<ReferenciasDTO>>> ObtenerListadoReferencias(ReferenciasRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de obtener listado de referencias");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
            var query = string.Empty;

            try
            {
                query = @"
                                SELECT 
                                    r.Codigo_Referencia as CodigoReferencia,
                                    r.Dominio_Referencia as DominioReferencia,
                                    r.Codigo_Tipo_Referencia as CodigoTipoReferencia,
                                    r.Descripcion_Referencia as DescripcionReferencia,
                                    r.Estado,
                                    r.Valor_Fecha as ValorFecha,
                                    r.Valor_Alfanumerico as ValorAlfanumerico,
                                    r.Descripcion_Larga as DescripcionLarga,
                                    r.Valor_Decimal as ValorDecimal,
                                    t.descripcion_tipo_referencia as DescripcionTipoReferencia
                                FROM 
                                    referencias r
                                JOIN
                                    tipo_referencias t ON r.Codigo_Tipo_Referencia = t.codigo_tipo_referencia
                                WHERE 1 = 1";

                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.DominioReferencia) ||
                        !string.IsNullOrEmpty(request.CodigoTipoReferencia.ToString()) ||
                        !string.IsNullOrEmpty(request.DescripcionReferencia) ||
                        !string.IsNullOrEmpty(request.ValorFecha.ToString()) ||
                        !string.IsNullOrEmpty(request.ValorAlfanumerico) ||
                        !string.IsNullOrEmpty(request.DescripcionLarga) ||
                        !string.IsNullOrEmpty(request.ValorDecimal.ToString()) ||
                        !string.IsNullOrEmpty(request.Estado)||
                        !string.IsNullOrEmpty(request.DescripcionTipoReferencia) 
                        )
                {
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        query += @"
                                    AND (
                                        r.Dominio_Referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR CONVERT(NVARCHAR(MAX), r.Codigo_Tipo_Referencia) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.Descripcion_Referencia LIKE '%' + @terminoDeBusqueda + '%'
                                        OR CONVERT(NVARCHAR(MAX), r.Valor_Fecha, 126) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.Valor_Alfanumerico LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR t.descripcion_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR r.Descripcion_Larga LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR CONVERT(NVARCHAR(MAX), r.Valor_Decimal) LIKE '%' + @terminoDeBusqueda + '%'
	                                    OR r.Estado = CASE 
                                                          WHEN 'INACTIVO' LIKE @terminoDeBusqueda + '%' 
                                                              AND 'ACTIVO' NOT LIKE @terminoDeBusqueda + '%' THEN 2
                                                          WHEN 'ACTIVO' LIKE '%' + @terminoDeBusqueda + '%' THEN 1
                                                      END
                                    )";
                    }
                    else
                    {
                        query += @"
                                    AND (
                                        (@parametroDominioReferencia IS NULL OR r.Dominio_Referencia LIKE '%' + @parametroDominioReferencia + '%')
                                        AND (@parametroCodigoTipoReferencia IS NULL OR (@parametroCodigoTipoReferencia != 0 AND r.Codigo_Tipo_Referencia = @parametroCodigoTipoReferencia))
                                        AND (@parametroDescripcionReferencia IS NULL OR r.Descripcion_Referencia LIKE '%' + @parametroDescripcionReferencia + '%')
                                        AND (@parametroValorFecha IS NULL OR CAST(r.Valor_Fecha AS DATE) = @parametroValorFecha)
                                        AND (@parametroValorAlfanumerico IS NULL OR r.Valor_Alfanumerico LIKE '%' + @parametroValorAlfanumerico + '%')
                                        AND (@parametroDescripcionTipoReferencia IS NULL OR t.descripcion_tipo_referencia LIKE '%' + @parametroDescripcionTipoReferencia + '%')
                                        AND (@parametroDescripcionLarga IS NULL OR r.Descripcion_Larga LIKE '%' + @parametroDescripcionLarga + '%')
                                        AND (@parametroValorDecimal IS NULL OR (@parametroValorDecimal != 0 AND r.Valor_Decimal = @parametroValorDecimal))
                                        AND (@parametroEstado IS NULL OR (r.Estado = CASE 
                                                       WHEN 'ACTIVO' LIKE '%' + @parametroEstado + '%' THEN 1 
                                                       WHEN 'INACTIVO' LIKE '%' + @parametroEstado + '%' THEN 2 
                                                   END))
                                    )";
                    }
                }
                else
                {
                    query += @"
                               AND (r.Estado = 1) ";
                }

                query += @"
                                ORDER BY Codigo_Referencia
                                OFFSET @saltarRegistros ROWS
                                FETCH NEXT @cantidadRegistros ROWS ONLY";

                string queryCantidadTotalRegistros = @"
                                SELECT 
                                    COUNT(*)
                                FROM 
                                    referencias r
                                JOIN
                                    tipo_referencias t ON r.Codigo_Tipo_Referencia = t.codigo_tipo_referencia
                                WHERE 1 = 1";
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.DominioReferencia) ||
                        !string.IsNullOrEmpty(request.CodigoTipoReferencia.ToString()) ||
                        !string.IsNullOrEmpty(request.DescripcionReferencia) ||
                        !string.IsNullOrEmpty(request.ValorFecha.ToString()) ||
                        !string.IsNullOrEmpty(request.ValorAlfanumerico) ||
                        !string.IsNullOrEmpty(request.DescripcionLarga) ||
                        !string.IsNullOrEmpty(request.ValorDecimal.ToString()) ||
                        !string.IsNullOrEmpty(request.Estado) ||
                        !string.IsNullOrEmpty(request.DescripcionTipoReferencia)
                        )
                {
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        queryCantidadTotalRegistros += @"
                                    AND (
                                        r.Dominio_Referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR CONVERT(NVARCHAR(MAX), r.Codigo_Tipo_Referencia) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.Descripcion_Referencia LIKE '%' + @terminoDeBusqueda + '%'
                                        OR CONVERT(NVARCHAR(MAX), r.Valor_Fecha, 126) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.Valor_Alfanumerico LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR t.descripcion_tipo_referencia LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR r.Descripcion_Larga LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR CONVERT(NVARCHAR(MAX), r.Valor_Decimal) LIKE '%' + @terminoDeBusqueda + '%'
	                                    OR r.Estado = CASE 
                                                          WHEN 'INACTIVO' LIKE @terminoDeBusqueda + '%' 
                                                              AND 'ACTIVO' NOT LIKE @terminoDeBusqueda + '%' THEN 2
                                                          WHEN 'ACTIVO' LIKE '%' + @terminoDeBusqueda + '%' THEN 1
                                                      END
                                    )";
                    }
                    else
                    {
                        queryCantidadTotalRegistros += @"
                                    AND (
                                        (@parametroDominioReferencia IS NULL OR r.Dominio_Referencia LIKE '%' + @parametroDominioReferencia + '%')
                                        AND (@parametroCodigoTipoReferencia IS NULL OR (@parametroCodigoTipoReferencia != 0 AND r.Codigo_Tipo_Referencia = @parametroCodigoTipoReferencia))
                                        AND (@parametroDescripcionReferencia IS NULL OR r.Descripcion_Referencia LIKE '%' + @parametroDescripcionReferencia + '%')
                                        AND (@parametroValorFecha IS NULL OR CAST(r.Valor_Fecha AS DATE) = @parametroValorFecha)
                                        AND (@parametroValorAlfanumerico IS NULL OR r.Valor_Alfanumerico LIKE '%' + @parametroValorAlfanumerico + '%')
                                        AND (@parametroDescripcionTipoReferencia IS NULL OR t.descripcion_tipo_referencia LIKE '%' + @parametroDescripcionTipoReferencia + '%')
                                        AND (@parametroDescripcionLarga IS NULL OR r.Descripcion_Larga LIKE '%' + @parametroDescripcionLarga + '%')
                                        AND (@parametroValorDecimal IS NULL OR (@parametroValorDecimal != 0 AND r.Valor_Decimal = @parametroValorDecimal))
                                        AND (@parametroEstado IS NULL OR (r.Estado = CASE 
                                                       WHEN 'ACTIVO' LIKE '%' + @parametroEstado + '%' THEN 1 
                                                       WHEN 'INACTIVO' LIKE '%' + @parametroEstado + '%' THEN 2
                                                   END))
                                    )";
                    }
                }
                else
                {
                    queryCantidadTotalRegistros += @"
                               AND (r.Estado = 1) ";
                }

                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                parametros.Add("@estado", request.Estado);
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);

                parametros.Add("@parametroDominioReferencia", request.DominioReferencia);
                parametros.Add("@parametroCodigoTipoReferencia", request.CodigoTipoReferencia);
                parametros.Add("@parametroDescripcionReferencia", request.DescripcionReferencia);
                parametros.Add("@parametroValorFecha", request.ValorFecha);
                parametros.Add("@parametroValorAlfanumerico", request.ValorAlfanumerico);
                parametros.Add("@parametroDescripcionLarga", request.DescripcionLarga);
                parametros.Add("@parametroValorDecimal", request.ValorDecimal);
                parametros.Add("@parametroEstado", request.Estado);
                parametros.Add("@parametroDescripcionTipoReferencia", request.DescripcionTipoReferencia);

                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<ReferenciasDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ReferenciasDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener listado de referencias");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new ReferenciasException("Ocurrió un error al obtener el listado de tipos de referencia. Detalles: " + ex.Message);
            }
        }
    }
}
