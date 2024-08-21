using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Persistence
{
    public class DbConnections
    {       
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Conectar _conectar;

        public DbConnections(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, Conectar conectar)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _conectar = conectar;
        }

        public IDbConnection CreateSqlConnection()
        {
            string sqlConnectionString = BuildConnectionString(_configuration, "ConexionDB");
            return new SqlConnection(sqlConnectionString);
        }

        public IDbConnection CreateSqlConnectionCapital()
        {
            string sqlConnectionString = BuildConnectionStringCapital(_configuration, "ConexionDBCapital");
            return new SqlConnection(sqlConnectionString);
        }

        private string BuildConnectionString(IConfiguration configuration, string name)
        {
            var connectionSettings = configuration.GetSection($"ConnectionStrings:{name}");

            if (connectionSettings == null || !connectionSettings.Exists())
            {
                throw new ParametroFaltanteCadenaConexionException($"Detalles de conexión para '{name}' no encontrados.");
            }

            var server = connectionSettings["Server"];
            var initialCatalog = connectionSettings["InitialCatalog"];
            var userId = connectionSettings["UserId"];
            var password = connectionSettings["Pwd"];
            var multipleActiveResultSets = connectionSettings.GetValue<bool?>("MultipleActiveResultSets");
            var pooling = connectionSettings.GetValue<bool?>("Pooling");
            var maxPoolSize = connectionSettings.GetValue<int?>("MaxPoolSize");
            var minPoolSize = connectionSettings.GetValue<int?>("MinPoolSize");
            var encrypt = connectionSettings.GetValue<bool?>("Encrypt");
            var trustServerCertificate = connectionSettings.GetValue<bool?>("TrustServerCertificate");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(initialCatalog))
            {
                throw new ParametroFaltanteCadenaConexionException("Uno o más parámetros requeridos de la cadena de conexión son nulos o están vacíos.");
            }

            string dbUser = string.Empty;
            string db = initialCatalog;

            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("codCircunscripcion"))
            {
                dbUser = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
                if (!string.IsNullOrEmpty(dbUser))
                {
                    var codCircunscripcion = int.Parse(dbUser);
                    var datosConexion = _conectar.ObtenerCadena(codCircunscripcion);
                    db = datosConexion.Db;                   
                }
            }
            
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                UserID = userId,
                Password = password
            };

            if (multipleActiveResultSets.HasValue) builder.MultipleActiveResultSets = multipleActiveResultSets.Value;
            if (pooling.HasValue) builder.Pooling = pooling.Value;
            if (maxPoolSize.HasValue) builder.MaxPoolSize = maxPoolSize.Value;
            if (minPoolSize.HasValue) builder.MinPoolSize = minPoolSize.Value;
            if (encrypt.HasValue) builder.Encrypt = encrypt.Value;
            if (trustServerCertificate.HasValue) builder.TrustServerCertificate = trustServerCertificate.Value;

            return builder.ConnectionString;
        }

        // Conexión exclusiva a capital para la sincronización de datos de versiones circunscripción-capital
        // Se requiere ya que se debe poder ejecutar el proceso desde la circunscripción
        private string BuildConnectionStringCapital(IConfiguration configuration, string name)
        {
            var connectionSettings = configuration.GetSection($"ConnectionStrings:{name}");

            if (connectionSettings == null || !connectionSettings.Exists())
            {
                throw new ParametroFaltanteCadenaConexionException($"Detalles de conexión para '{name}' no encontrados.");
            }

            var server = connectionSettings["Server"];
            var initialCatalog = connectionSettings["InitialCatalog"];
            var userId = connectionSettings["UserId"];
            var password = connectionSettings["Pwd"];
            var multipleActiveResultSets = connectionSettings.GetValue<bool?>("MultipleActiveResultSets");
            var pooling = connectionSettings.GetValue<bool?>("Pooling");
            var maxPoolSize = connectionSettings.GetValue<int?>("MaxPoolSize");
            var minPoolSize = connectionSettings.GetValue<int?>("MinPoolSize");
            var encrypt = connectionSettings.GetValue<bool?>("Encrypt");
            var trustServerCertificate = connectionSettings.GetValue<bool?>("TrustServerCertificate");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(initialCatalog))
            {
                throw new ParametroFaltanteCadenaConexionException("Uno o más parámetros requeridos de la cadena de conexión son nulos o están vacíos.");
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = initialCatalog,
                UserID = userId,
                Password = password
            };

            if (multipleActiveResultSets.HasValue) builder.MultipleActiveResultSets = multipleActiveResultSets.Value;
            if (pooling.HasValue) builder.Pooling = pooling.Value;
            if (maxPoolSize.HasValue) builder.MaxPoolSize = maxPoolSize.Value;
            if (minPoolSize.HasValue) builder.MinPoolSize = minPoolSize.Value;
            if (encrypt.HasValue) builder.Encrypt = encrypt.Value;
            if (trustServerCertificate.HasValue) builder.TrustServerCertificate = trustServerCertificate.Value;

            return builder.ConnectionString;
        }
    }
}
