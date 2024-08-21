using Application.Services;
using Application.Services.Interfaces;
using Domain.Entities.ConectarCircunscripcion;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());           
            services.AddSingleton<ISolicitudObjetosBienesService, SolicitudObjetosBienesService>();
            services.AddSingleton<IReferenciaService, ReferenciaService>();
            services.AddSingleton<IAsignacionContratoPorObjetosGastosService, AsignacionContratoPorObjetosGastosService>();
            services.AddSingleton<IReportesService, ReportesService>();
            services.AddSingleton<IGenerarSolicitudesporCircunscripcionService, GenerarSolicitudesporCircunscripcionService>();
            services.AddSingleton<IConfiguracionPresupuestariaService, ConfiguracionPresupuestariaService>();
            services.AddSingleton<IPlanificacionFinancieraService, PlanificacionFinancieraService>();
            services.AddSingleton<IConsolidadoAnteproyectoPresupuestoService, ConsolidadoAnteproyectoPresupuestoService>();
            services.AddSingleton<IVersionadoAnteproyectoService, VersionadoAnteproyectoService>();
            services.AddSingleton<IConsolidarAnteproyectoModificarObjetoGastoService, ConsolidarAnteproyectoModificarObjetoGastoService>();
            services.AddSingleton<ISincronizacionDatosCircunscripcionService, SincronizacionDatosCircunscripcionService>();
            services.AddSingleton<IConectarCircunscripcionService, ConectarCircunscripcionService>();
            services.AddSingleton<IDatosUsuarioService, DatosUsuarioService>();
        }
    }
}
