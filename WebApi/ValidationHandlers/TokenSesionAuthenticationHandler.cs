using Application.Services.Interfaces;
using Domain.Entities.ConectarCircunscripcion;
using Google.Protobuf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WebApi.Protos;

namespace WebApi.ValidationHandlers;

public class TokenSesionAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "TokenSesion";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;    
}
public class TokenSesionAuthenticationHandler : AuthenticationHandler<TokenSesionAuthenticationSchemeOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenSesionAuthenticationHandler> _logger;
    private readonly PermisosConfiguration _rolesConfiguration;

    private const string AuthHeaderName = "authorization";
    private const string RolHeaderName = "usuario-rol";
    private readonly IDatosUsuarioService _usuarioService;

    public TokenSesionAuthenticationHandler(IOptionsMonitor<TokenSesionAuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock,
        IHttpContextAccessor httpContextAccessor, PermisosConfiguration rolesConfiguration, IDatosUsuarioService usuarioService) : base(options, loggerFactory, encoder, clock)
    {

        _httpContextAccessor = httpContextAccessor;
        _logger = loggerFactory.CreateLogger<TokenSesionAuthenticationHandler>(); ;
        _rolesConfiguration = rolesConfiguration;
        _usuarioService = usuarioService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogInformation("Inicio de proceso de Gestion de la logica de Autenticacion Token Sesion");
            
        if (!Request.Headers.ContainsKey("usuario-sesion") || !Request.Headers.ContainsKey(RolHeaderName))
        {
            _logger.LogWarning("Ocurrio un Error en la Autenticacion Token Sesion,Cabecera no puede estar vacia");
            return AuthenticateResult.NoResult();
            //throw new ErrorAutorizacionUsuarioException("Usuario no Autorizado para consumir el recurso seleccionado");
        }

        var rolSeleccionado = int.Parse(Request.Headers["Usuario-Rol"].ToString());
        var datosUsuario = new RespuestaDatosUsuario();
        var value = _rolesConfiguration.Mark;

        if (string.IsNullOrEmpty(value))
        {
            var sesionBase64 = Request.Headers["usuario-sesion"]!;
            datosUsuario = RespuestaDatosUsuario.Parser.ParseFrom(Base64UrlTextEncoder.Decode(sesionBase64));
        }
        else
        {
            //Esta Marca usamos  cuando realizamos Test Rapidos sin usar Bytes
            RespuestaDatosUsuario respuestaDatosUsuario = new RespuestaDatosUsuario();
            datosUsuario = JsonParser.Default.Parse<RespuestaDatosUsuario>(estructuraJson());
        }

        if (datosUsuario != null)
        {
            _usuarioService.DatosUsuario = new UsuarioInfo
            {
                NombreUsuario = datosUsuario.NombreUsuario,
                NumeroDocumento = datosUsuario.NumeroDocumento                
            };

            var appRoles = datosUsuario.Aplicaciones.FirstOrDefault(rol => {
                return rol.AppId == int.Parse(_rolesConfiguration.AppId.ToString());
            });
            if (appRoles != null)
            {
                var rol = appRoles.Roles.FirstOrDefault(rol => rol.RolId == rolSeleccionado);
                if (rol != null)
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, datosUsuario.NombreUsuario));
                    claims.Add(new Claim("RolSeleccionado", rolSeleccionado.ToString()));
                    claims.Add(new Claim("NumeroDocumento", datosUsuario.NumeroDocumento));
                    claims.Add(new Claim("CodUsuario", datosUsuario.CodigoUsuario.ToString()));

                    foreach (var permisosApp in datosUsuario.PermisosAplicacion)
                    {
                        claims.AddRange(
                          from rolApp in permisosApp.Roles
                          from permiso in rolApp.Permisos
                          select new Claim("Permiso", permiso.Descripcion)
                        );
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, nameof(TokenSesionAuthenticationHandler));
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                return AuthenticateResult.Fail("Usuario no Autorizado para consumir el recurso seleccionado");
            }
            return AuthenticateResult.Fail("Roles de Usuario no encontrado");
        }
        _logger.LogInformation("Fin de proceso de Gestion de la logica de Generar JWT");
        return AuthenticateResult.Fail("Usuario no Autorizado para consumir el recurso seleccionado");
    }

    private string estructuraJson()
    {
        string json = @"{
          ""nombre_usuario"": ""Informatico, Administrador"",
          ""numero_documento"": ""20000000"",
          ""codigo_usuario"": 37593,
          ""roles"": [
            {
              ""app_id"": 26,
              ""roles"": [
                {
                  ""rol_id"": 16,
                  ""descripcion_rol"": ""Abogados""
                }
              ]
            },
            {
              ""app_id"": 49,
              ""roles"": [
                {
                  ""rol_id"": 16,
                  ""descripcion_rol"": ""Abogados""
                },
                {
                  ""rol_id"": 25,
                  ""descripcion_rol"": ""Fiscal Penal""
                }
              ]
            },
            {
              ""app_id"": 50,
              ""roles"": [
                {
                  ""rol_id"": 16,
                  ""descripcion_rol"": ""Abogados""
                }
              ]
            },
            {
              ""app_id"": 3062,
              ""roles"": [
                {
                  ""rol_id"": 16,
                  ""descripcion_rol"": ""Abogados""
                },
                {
                  ""rol_id"": 25,
                  ""descripcion_rol"": ""Fiscal Penal""
                },
                {
                  ""rol_id"": 4159,
                  ""descripcion_rol"": ""Asistente""
                }
              ]
            },
            {
              ""app_id"": 3065,
              ""roles"": [
                {
                  ""rol_id"": 16,
                  ""descripcion_rol"": ""Abogados""
                },
                {
                  ""rol_id"": 25,
                  ""descripcion_rol"": ""Fiscal Penal""
                }
              ]
            }
          ],        
          ""sesion_id"": """"        
        }";

        return json;
    }
}

