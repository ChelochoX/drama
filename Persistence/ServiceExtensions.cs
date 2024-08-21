using Application.Services.Interfaces.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;

namespace Persistence
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<DbConnections>();
          
            services.AddSingleton<IGenerarSolicitudCircunscripcionRepository, GenerarSolicitudCircunscripcionRepository>();
            services.AddSingleton<IReferenciasRepository, ReferenciasRepository>();
            services.AddSingleton<IGenerarSolicitudporCircunscripcionRepository, GenerarSolicitudporCircunscripcionRepository>();
            services.AddSingleton<IConfiguracionPresupuestariaRepository, ConfiguracionPresupuestariaRepository>();
            services.AddSingleton<IPlanificacionFinancieraRepository, PlanificacionFinancieraRepository>();
            services.AddSingleton<IAsignacionContratoPorObjetosGastosRepository, AsignacionContratoPorObjetosGastosRepository>();
            services.AddSingleton<IConsolidadoAnteproyectoPresupuestoRepository, ConsolidadoAnteproyectoPresupuestoRepository>();
            services.AddSingleton<IVersionadoAnteproyectoRepository, VersionadoAnteproyectoRepository>();
            services.AddSingleton<IConsolidarAnteproyectoModificarObjetoGastoRepository, ConsolidarAnteproyectoModificarObjetoGastoRepository>();
            services.AddSingleton<ISincronizacionDatosCircunscripcionRepository, SincronizacionDatosCircunscripcionRepository>();
            services.AddSingleton<IConectarCircunscripcionRepository, ConectarCircunscripcionRepository>();
        }
    }
}
