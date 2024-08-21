using Domain.Entities.ConectarCircunscripcion;
using Domain.Entities.Server;
using Microsoft.AspNetCore.Authorization;
using WebApi.ValidationHandlers;

namespace WebApi.Extensions;

public static class ServiceExtensions
{
    public static void AddConfiguration(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<Conectar>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TokenSesionAuthenticationSchemeOptions.DefaultScheme;
            options.DefaultChallengeScheme = TokenSesionAuthenticationSchemeOptions.DefaultScheme;
        })
            .AddScheme<TokenSesionAuthenticationSchemeOptions, TokenSesionAuthenticationHandler>
            (TokenSesionAuthenticationSchemeOptions.DefaultScheme, options => {});

        
        services.AddSingleton<IAuthorizationHandler, PermisosAuthorizationHandler>();
        services.AddAuthorization(options => {
            options.AddPolicy("Endpoint", policyBuilder => {
                policyBuilder.Requirements.Add(new PermissionAuthorizationRequirement());
            });
        });
        services.AddCors(options =>
        {
            options.AddPolicy("UrlsGenericas",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        var rolesConfiguration = LoadRolesConfiguration(webHostEnvironment);
        services.AddSingleton(rolesConfiguration);       

        services.Configure<JasperServer>(configuration.GetSection("JasperServer"));
        services.Configure<GestionArchivos>(configuration.GetSection("GestionarArchivos"));

        services.AddHttpClient("JasperClient", client =>
        {
            var options = configuration.GetSection("JasperServer").Get<JasperServer>();
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient("GestionArchivosClient", client =>
        {
            var options = configuration.GetSection("GestionarArchivos").Get<GestionArchivos>();
            client.BaseAddress = new Uri(options.BaseUrl);
        });

    }
    private static PermisosConfiguration LoadRolesConfiguration(IWebHostEnvironment webHostEnvironment)
    {
        string configFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "ValidationHandlers", "ConfiguracionPermisos.json");

        var builder = new ConfigurationBuilder()
           .SetBasePath(webHostEnvironment.ContentRootPath)
           .AddJsonFile(configFilePath, optional: false, reloadOnChange: true);

        var configuration = builder.Build();
        var permisosSection = configuration.GetSection("Settings:Permisos");
        var appIdSection = configuration.GetSection("Settings:AppId");
        var markSection = configuration.GetSection("Mark");

        var rolesConfiguration = new PermisosConfiguration
        {
            Permisos = new Dictionary<string, List<string>>()
        };
        permisosSection.Bind(rolesConfiguration.Permisos);
        rolesConfiguration.AppId = Convert.ToInt32(appIdSection.Value);
        rolesConfiguration.Mark = markSection.Value;

        return rolesConfiguration;
    }
}



