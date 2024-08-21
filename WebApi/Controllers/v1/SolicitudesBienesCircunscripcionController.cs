using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Anteproyecto;
using Domain.Entities.Request;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;
using WebApi.ValidationHandlers;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SolicitudesBienesCircunscripcionController : ControllerBase
{
    private readonly ISolicitudObjetosBienesService _service;
    private readonly IDatosUsuarioService _usuarioService;


    public SolicitudesBienesCircunscripcionController(ISolicitudObjetosBienesService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }


    #region DESARROLLO DE LA INCIDENCIA CSJ-152


    [HttpGet("VersionesAnteproyecto")]
    [Endpoint("VersionesAnteproyecto")]
    [SwaggerOperation(
     Summary = "Nos permite obtener versiones de Anteproyectos",
     Description = "Obtiene información de las versiones de Anteproyectos.")]
    public async Task<ActionResult<IEnumerable<VersionAnteproyectoDTO>>> VersionesAnteproyecto(
     [FromQuery][Description("Valor que indica el valor de la fecha")] DateTime? fecha,
     [FromQuery][Description("Valor que indica el estado del Anterpoyecto")] string? descripcionEstado,
     [FromQuery][Description("Valor que identifica el Ejercicio buscar en el filtro generico")] int? ejercicio,
     [FromQuery][Description("Valor que identifica el número de versión de Anteproyecto")] int? codigoVersion,
     [FromQuery][Description("Valor que identifica el número de versión de Anteproyecto")] int? numeroVersion,
     [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
     [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new VersionAnteproyectoRequest
        {
            Fecha = fecha,
            Ejercicio = ejercicio,
            Estado = descripcionEstado,
            TerminoDeBusqueda = terminoDeBusqueda,
            CodigoVersion = codigoVersion,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            NumeroVersion = numeroVersion
        };

        var validationResult = new VersionAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.VersionAnteproyectoRequest(request);

        if (resultado == null)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<Datos<IEnumerable<VersionAnteproyectoDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

    [HttpGet("AnteproyectoPresupuestario")]
    [Endpoint("AnteproyectoPresupuestario")]
    [SwaggerOperation(
     Summary = "Nos permite obtener Anteproyectos Presupuestario",
     Description = "Obtiene información de los Anteproyectos Presupuestario.")]
    public async Task<ActionResult<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestario(
     [FromQuery][Description("Valor que indica el valor de la numero_og")] int? numeroOg,
     [FromQuery][Description("Valor que indica el estado del Anterpoyecto")] int? numeroFf,
     [FromQuery][Description("Valor que identifica el Ejercicio buscar en el filtro generico")] int? numeroOf,
     [FromQuery][Description("Valor que identifica el número de versión de Anteproyecto")] int? numeroDpto,
     [FromQuery][Description("Valor que identifica como fundamentacion")] string? fundamentacion,
     [FromQuery][Description("Valor que identifica el número de presupuesto_inicial")] string? presupuestoInicial,
     [FromQuery][Description("Valor que identifica el número de presupuesto_aprobado")] string? presupuestoAprobado,
     [FromQuery][Description("Valor que identifica el número de modificaciones")] string? modificaciones,
     [FromQuery][Description("Valor que identifica el número de presupuesto_vigente")] string? presupuestoVigente,
     [FromQuery][Description("Valor que identifica el número de proyecto_presupuesto")] string? proyectoPresupuesto,
     [FromQuery][Description("Valor que identifica el número de diferencia")] string? diferencia,
     [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
     [FromQuery][Description("Expresion que se utilizara como total contrato")] string? totalContrato,
     [FromQuery][Description("Valor que identifica el número de porcentaje")] string? porcentaje,
     [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] int? codigoVersion,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] int? codigoAnteproyectoObjeto,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? descripcionMateria,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? descripcionCentroResponsabilidad)
    {
        var request = new AnteproyectoPresupuestarioRequest
        {
            numeroOf = numeroOf,
            numeroOg = numeroOg,
            numeroFf = numeroFf,
            numeroDpto = numeroDpto,
            fundamentacion = fundamentacion,
            presupuestoInicial = presupuestoInicial,
            presupuestoAprobado = presupuestoAprobado,
            modificaciones = modificaciones,
            presupuestoVigente = presupuestoVigente,
            terminoDeBusqueda = terminoDeBusqueda,
            proyectoPresupuesto = proyectoPresupuesto,
            diferencia = diferencia,
            pagina = pagina,
            cantidadRegistros = cantidadRegistros,
            porcentaje = porcentaje,
            totalContrato = totalContrato,
            codigoVersion = codigoVersion,
            codigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
            descripcionMateria = descripcionMateria,
            descripcionCentroResponsabilidad = descripcionCentroResponsabilidad
        };

        var validationResult = new AnteproyectoPresupuestarioRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.AnteproyectoPresupuestarioRequest(request);

        if (resultado == null)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpGet("ContadorDias")]
    [Endpoint("ContadorDias")]
    [SwaggerOperation(
   Summary = "Nos permite obtener los totales de bienes y montos",
   Description = "Obtiene información de los totales de bienes y montos.")]
    public async Task<ActionResult<CantidadTotalDiasDTO>> CantidadTotalDias()
    {

        var listado = await _service.CantidadTotalDias();

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron registros para los datos proporcionados.");
        }

        return Ok(new ApiResponse<CantidadTotalDiasDTO>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpPut("ActualizarVersionAnteproyectoObjeto")]
    [Endpoint("ActualizarVersionAnteproyectoObjeto")]
    [SwaggerOperation(
    Summary = "Actualiza Objeto Version Anteproyecto",
    Description = "Actualiza la información del Objeto Version Anteproyecto.")]
    public async Task<ActionResult> ActualizarVersionAnteproyectoObjeto(
    [FromBody] ActualizarVersionAnteproyectoObjetoRequest version)
    {
        
       version.UsuarioUltimaModificacion = _usuarioService.DatosUsuario.NumeroDocumento;
       

        if (version.CodigoVersion == 0)
        {
            throw new ReglasdeNegocioException("El código de Version Anteproyecto no puede estar vacío o tener valor igual a cero");
        }
        if (version.UsuarioUltimaModificacion == "")
        {
            throw new ReglasdeNegocioException("El código de Usuario de Version Anteproyecto no puede estar vacío ");
        }

        var filasActualizadas = await _service.ActualizarVersionAnteproyectoObjeto(version);

        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }

        throw new KeyNotFoundException();
    }



    [HttpPut("ActualizarEstadoVersionAnteproyecto")]
    [Endpoint("ActualizarEstadoVersionAnteproyecto")]
    [SwaggerOperation(
          Summary = "Actualiza Estado Version Anteproyecto",
          Description = "Actualiza la información del Estado Version Anteproyecto.")]
    public async Task<ActionResult> ActualizarEstadoVersionAnteproyecto(
      [FromBody] ActualizarEstadoVersionAnteproyectoRequest version)
    {
        version.CodigoUsuarioLoggeado = _usuarioService.DatosUsuario.NumeroDocumento;

        var filasActualizadas = await _service.ActualizarEstadoVersionAnteproyecto(version);

        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }

        throw new KeyNotFoundException();
    }

    

    [HttpPut("CerrarEstadoVersionAnteproyecto")]
    [Endpoint("CerrarEstadoVersionAnteproyecto")]
    [SwaggerOperation(
        Summary = "Actualiza Estado Version Anteproyecto",
        Description = "Actualiza la información del Estado Version Anteproyecto.")]
    public async Task<ActionResult> CerrarEstadoVersionAnteproyecto(
    [FromBody] ActualizarEstadoVersionAnteproyectoRequest version)
    {
        version.CodigoUsuarioLoggeado = _usuarioService.DatosUsuario.NumeroDocumento;

        var filasActualizadas = await _service.CerrarEstadoVersionAnteproyecto(version);

        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }

        throw new KeyNotFoundException();
    }
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-154

    [HttpGet("AnteproyectoPresupuestarioVerificaciones")]
    [Endpoint("AnteproyectoPresupuestarioVerificaciones")]
    [SwaggerOperation(
     Summary = "Nos permite obtener Anteproyectos Presupuestario",
     Description = "Obtiene información de los Anteproyectos Presupuestario.")]
    public async Task<ActionResult> AnteproyectoPresupuestarioVerificacionesRequest(
     [FromQuery][Description("Valor que indica el valor de la numero_og")] int? numeroOg,
     [FromQuery][Description("Valor que indica el estado del Anterpoyecto")] int? numeroFf,
     [FromQuery][Description("Valor que identifica el Ejercicio buscar en el filtro generico")] int? numeroOf,
     [FromQuery][Description("Valor que identifica el número de versión de Anteproyecto")] int? numeroDpto,
     [FromQuery][Description("Valor que identifica como fundamentacion")] string? fundamentacion,
     [FromQuery][Description("Valor que identifica el número de presupuesto_inicial")] string? presupuestoInicial,
     [FromQuery][Description("Valor que identifica el número de presupuesto_aprobado")] string? presupuestoAprobado,
     [FromQuery][Description("Valor que identifica el número de modificaciones")] string? modificaciones,
     [FromQuery][Description("Valor que identifica el número de presupuesto_vigente")] string? presupuestoVigente,
     [FromQuery][Description("Valor que identifica el número de proyecto_presupuesto")] string? proyectoPresupuesto,
     [FromQuery][Description("Valor que identifica el número de diferencia")] string? diferencia,
     [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
     [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
     [FromQuery][Description("Expresion que se utilizara como Ejercicio")] int? ejercicio,
     [FromQuery][Description("Valor que identifica el número de porcentaje")] string? porcentaje,
     [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
     [FromQuery][Description("Expresion que se utilizara como codigo version")] int? codigoVersion,
     [FromQuery][Description("Expresion que se utilizara como estado")] string? estado,
     [FromQuery][Description("Expresion que se utilizara como circunscripcion")] string? circunscripcion)
    {
        var request = new AnteproyectoPresupuestarioRequest
        {

            fundamentacion = fundamentacion,
            modificaciones = modificaciones,
            circunscripcion = circunscripcion,
            diferencia = diferencia,
            porcentaje = porcentaje,
            ejercicio = ejercicio,
            estado = estado,
            numeroOf = numeroOf,
            numeroOg = numeroOg,
            numeroFf = numeroFf,
            numeroDpto = numeroDpto,
            presupuestoInicial = presupuestoInicial,
            presupuestoAprobado = presupuestoAprobado,
            presupuestoVigente = presupuestoVigente,
            terminoDeBusqueda = terminoDeBusqueda,
            proyectoPresupuesto = proyectoPresupuesto,
            pagina = pagina,
            cantidadRegistros = cantidadRegistros,
            codigoVersion = codigoVersion


        };

        var validationResult = new AnteproyectoPresupuestarioRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.AnteproyectoPresupuestarioVerificacionesRequest(request);

        if (resultado == null)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpGet("AnteproyectoPresupuestarioCAB")]
    [Endpoint("AnteproyectoPresupuestarioCAB")]
    [SwaggerOperation(
           Summary = "Nos permite obtener informacion de los Anteproyecto Presupuestario",
           Description = "Obtiene información de los de los Anteproyecto Presupuestario.")]
    public async Task<ActionResult<IEnumerable<AnteproyectoPresupuestarioCABDTO>>> AnteproyectoPresupuestarioCAB(
       [FromQuery][Description("Código único de usuario")] int pagina, int cantidadRegistros, string? terminoDeBusqueda, int? numeroVersion, int? ejercicio, string? estado)
    {
       
        var listado = await _service.AnteproyectoPresupuestarioCAB(pagina, cantidadRegistros, terminoDeBusqueda, numeroVersion, ejercicio, estado );


        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }



        return Ok(new ApiResponse<Datos<IEnumerable<AnteproyectoPresupuestarioCABDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpPut("AbrirVersionAnteproyecto")]
    [Endpoint("AbrirVersionAnteproyecto")]
    [SwaggerOperation(
           Summary = "Nos permite Abrir Version Anteproyecto Presupuestario",
           Description = "Obtiene información de Version Anteproyecto Presupuestario.")]
    public async Task<ActionResult> AbrirVersionAnteproyecto(
    [FromQuery][Description("Código de version")] int version)
    {   
        var cedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var filasActualizadas = await _service.AbrirVersionAnteproyecto(version, cedulaUsuario);

        if (filasActualizadas == 0)
        {
            return NotFound(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }
        else
        {
            if (filasActualizadas != null && filasActualizadas == 1)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = (int)filasActualizadas,
                    StatusCode = (int)HttpStatusCode.OK,
                });
            }
        }

        throw new KeyNotFoundException();
    }


    [HttpDelete("EliminarObjetodeAnteproyectoyBienes")]
    [Endpoint("EliminarObjetodeAnteproyectoyBienes")]
    [SwaggerOperation(
           Summary = "Nos permite Abrir Version Anteproyecto Presupuestario",
           Description = "Obtiene información de Version Anteproyecto Presupuestario.")]
    public async Task<ActionResult> EliminarObjetodeAnteproyectoyBienes([FromQuery][Description("Código de version")] int codigoVersion, int codigoVersionAnteproyecto, int codigoConfigPresupuestaria)
    {
        var filasActualizadas = await _service.EliminarObjetodeAnteproyectoyBienes(codigoVersion, codigoVersionAnteproyecto, codigoConfigPresupuestaria);

        if (filasActualizadas == 0)
        {
            return NotFound(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }
        else
        {
            if (filasActualizadas != null)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = (int)filasActualizadas,
                    StatusCode = (int)HttpStatusCode.OK,
                });
            }
        }

        throw new KeyNotFoundException();
    }

    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-158

    [HttpGet("ObtenerCentroResponsabilidadReporte")]
    [Endpoint("ObtenerCentroResponsabilidadReporte")]
    [SwaggerOperation(
         Summary = "Nos permite agregar informacion de los Objetos de Gasto",
         Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> ObtenerCentroResponsabilidadReporte()
    {
        var Cedula = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        if (Cedula == 0)
        {
            throw new ReglasdeNegocioException("La cédula no puede ser cero ");
        }

        if (Cedula == null)
        {
            throw new ReglasdeNegocioException("El código de Circunscripcion no puede ser nulo");
        }
        var listado = await _service.ObtenerCentroResponsabilidadReporte(Cedula);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpGet("AgregarOGCentroResponsabilidad")]
    [Endpoint("AgregarOGCentroResponsabilidad")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCentroResponsabilidad(
    [FromQuery][Description("Código único de usuario")] string codigoCircunscripcion)
    {
        if (codigoCircunscripcion == "")
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (codigoCircunscripcion == null)
        {
            throw new ReglasdeNegocioException("El código de Circunscripcion no puede ser nulo");
        }
        var listado = await _service.AgregarOGCentroResponsabilidad(codigoCircunscripcion);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpGet("AgregarOGDependenciaCircuscripcion")]
    [Endpoint("AgregarOGDependenciaCircuscripcion")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDependenciaCircuscripcion()
    {
        var user = _usuarioService.DatosUsuario.NumeroDocumento;

        if (user == "")
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (user == null)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede ser nulo");
        }
        var listado = await _service.AgregarOGDependenciaCircuscripcion(user);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }
 

    [HttpGet("AgregarOGMateria")]
    [Endpoint("AgregarOGMateria")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGMateria()
    {
        var user = _usuarioService.DatosUsuario.NumeroDocumento;

        if (user == "")
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (user == null)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede ser nulo");
        }
        var listado = await _service.AgregarOGMateria(user);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("AgregarCircuscripcion")]
    [Endpoint("AgregarCircuscripcion")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarCircuscripcion()
    {
        var user = _usuarioService.DatosUsuario.NumeroDocumento;

        if (user == "")
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (user == null)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede ser nulo");
        }
        var listado = await _service.AgregarCircuscripcion(user);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Circunscripcion para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }
    

    [HttpGet("AgregarOGDepartamento")]
    [Endpoint("AgregarOGDepartamento")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDepartamento(
       [FromQuery][Description("Código único de usuario")] string codigoCentroResponsabilidad)
    {
        if (codigoCentroResponsabilidad == "")
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (codigoCentroResponsabilidad == null)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede ser nulo");
        }
        var listado = await _service.AgregarOGDepartamento(codigoCentroResponsabilidad);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("AgregarOG")]
    [Endpoint("AgregarOG")]
    [SwaggerOperation(
           Summary = "Nos permite agregar informacion de los Objetos de Gasto",
           Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOG(
          [FromQuery][Description("Código único de ejercicio")] int ejercicio)
    {
        if (ejercicio == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        if (ejercicio == null)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede ser nulo");
        }
        var listado = await _service.AgregarOG(ejercicio);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("AgregarOGCodigoOrganismoFinanciador")]
    [Endpoint("AgregarOGCodigoOrganismoFinanciador")]
    [SwaggerOperation(
         Summary = "Nos permite agregar informacion de los Objetos de Gasto",
         Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoOrganismoFinanciador()
    {
       var listado = await _service.AgregarOGCodigoOrganismoFinanciador();

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("AgregarOGCodigoFuenteFinanciamiento")]
    [Endpoint("AgregarOGCodigoFuenteFinanciamiento")]
    [SwaggerOperation(
         Summary = "Nos permite agregar informacion de los Objetos de Gasto",
         Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoFuenteFinanciamiento()
    {

        var listado = await _service.AgregarOGCodigoFuenteFinanciamiento();

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("DatosOGCentroResponsabilidad")]
    [Endpoint("DatosOGCentroResponsabilidad")]
    [SwaggerOperation(
          Summary = "Nos permite agregar informacion de los Objetos de Gasto",
          Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> DatosOGCentroResponsabilidad(
      [FromQuery][Description("Código único de responsabilidad")] int codigoCentroResponsabilidad)
    {
        if (codigoCentroResponsabilidad == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }
        
        var listado = await _service.DatosOGCentroResponsabilidad(codigoCentroResponsabilidad);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseCentroResponsabilidadDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("DatosOGGrupoSubGrupo")]
    [Endpoint("DatosOGGrupoSubGrupo")]
    [SwaggerOperation(
          Summary = "Nos permite agregar informacion de los Objetos de Gasto",
          Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>> DatosOGGrupoSubGrupo(
      [FromQuery][Description("Código único de responsabilidad")] int codigoObjetoGasto, int ejercicio)
    {
        if (codigoObjetoGasto == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        var listado = await _service.DatosOGGrupoSubGrupo(codigoObjetoGasto, ejercicio);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("DatosOGBienesprioritarios")]
    [Endpoint("DatosOGBienesprioritarios")]
    [SwaggerOperation(
          Summary = "Nos permite agregar informacion de los Objetos de Gasto",
          Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>> DatosOGBienesprioritarios(
      [FromQuery][Description("Código único de responsabilidad")] int version, int codigoAnteproyectoObjeto)
    {
        if (codigoAnteproyectoObjeto == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        var listado = await _service.DatosOGBienesprioritarios(version, codigoAnteproyectoObjeto);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("DatosOGBienesMonto")]
    [Endpoint("DatosOGBienesMonto")]
    [SwaggerOperation(
           Summary = "Nos permite obtener el monto de planificacion",
           Description = "Nos permite obtener el monto de planificacion.")]
    public async Task<ActionResult> DatosOGBienesMonto(
       [FromQuery][Description("Código único de responsabilidad")] int version, int codigoAnteproyectoObjeto)
    {
        if (codigoAnteproyectoObjeto == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        var listado = await _service.DatosOGBienesMonto(version, codigoAnteproyectoObjeto);

        
        return Ok(new ApiResponse<CantidadTotalGenericaDTO>
        {
            Success = true,
            Data= listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

   

    [HttpGet("BuscarBien")]
    [Endpoint("BuscarBien")]
    [SwaggerOperation(
       Summary = "Nos permite agregar informacion de los Objetos de Gasto",
       Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBien(
      [FromQuery][Description("Código único de objeto gasto")] int codigoObjetoGasto, int ejercicio, int pagina, int cantidadRegistros, string? MontoUnitario, 
      string? TerminoDeBusqueda, string? catalogo, string? descripcionCatalogo)
    { 
        var listado = await _service.BuscarBien(codigoObjetoGasto, ejercicio, pagina, cantidadRegistros,MontoUnitario,TerminoDeBusqueda,catalogo,descripcionCatalogo);
        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("BuscarBienesPrioritarios")]
    [Endpoint("BuscarBienesPrioritarios")]
    [SwaggerOperation(
          Summary = "Nos permite agregar informacion de los Objetos de Gasto",
          Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<ClaseGenericaCodigoDescripDTO>>> BuscarBienesPrioritarios(
      [FromQuery][Description("Código único de responsabilidad")] int codigoCatalogo)
    {
        if (codigoCatalogo == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        var listado = await _service.BuscarBienesPrioritarios(codigoCatalogo);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("BuscarBienesGrilla")]
    [Endpoint("BuscarBienesGrilla")]
    [SwaggerOperation(
         Summary = "Nos permite agregar informacion de los Objetos de Gasto",
         Description = "Agrega informacion de los Objetos de Gasto.")]
    public async Task<ActionResult<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBienesGrilla(
     [FromQuery][Description("Código único de responsabilidad")] int version, int codigoAnteproyectoObjeto)
    {
        if (codigoAnteproyectoObjeto == 0)
        {
            throw new ReglasdeNegocioException("El código de no puede estar vacío ");
        }

        var listado = await _service.BuscarBienesGrilla(version, codigoAnteproyectoObjeto);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpDelete("EliminarBienAnteproyecto")]
    [Endpoint("EliminarBienAnteproyecto")]
    [SwaggerOperation(
           Summary = "Nos permite obtener el monto de planificacion",
           Description = "Nos permite obtener el monto de planificacion.")]
    public async Task<ActionResult> EliminarBienAnteproyecto(
    [FromQuery][Description("Código único de objeto gasto")] int codigoAnteProyectoBien, int codigoObjetoGasto)
    {
        var listado = await _service.EliminarBienAnteproyecto(codigoAnteProyectoBien, codigoObjetoGasto);
        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        
        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpPost("AgregarConfiguracionPresupuestaria")]
    [Endpoint("AgregarConfiguracionPresupuestaria")]
    [SwaggerOperation(
           Summary = "Nos permite Agregar Configuracion Presupuestaria.",
           Description = "Agregar Configuracion Presupuestaria.")]
    public async Task<ActionResult> AgregarConfiguracionPresupuestaria([FromBody] ClaseConfiguracionPresupuestaria configuracion)
    {
        configuracion.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var filasActualizadas = await _service.AgregarConfiguracionPresupuestaria(configuracion);
        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<ClaseLlavePrimaria>
            {
                Success = true,
                Data = filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
            });
        }

        throw new KeyNotFoundException();
    }



    [HttpGet("ObtenerVersiones")]
    [Endpoint("ObtenerVersiones")]
    [SwaggerOperation(
        Summary = "Nos permite Obtener Versiones de anteproyectos",
        Description = "Agrega informacion de Versiones de anteproyectos.")]
    public async Task<ActionResult<IEnumerable<VersionesAnteproyectosDTO>>> ObtenerVersiones()
    {
        var listado = await _service.ObtenerVersiones();

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de Anteproyecto Presupuestario para el usuario proporcionado.");
        }
        return Ok(new ApiResponse<Datos<IEnumerable<VersionesAnteproyectosDTO>>>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ObtenerVersionAbierta")]
    [Endpoint("ObtenerVersionAbierta")]
    [SwaggerOperation(
      Summary = "Nos permite Obtener Versiones de anteproyectos",
      Description = "Agrega informacion de Versiones de anteproyectos.")]
    public async Task<ActionResult> ObtenerVersionAbierta()
    {


        var listado = await _service.ObtenerVersionAbierta();

       
        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }
    

    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-ARCHIVOS

    [HttpPost("PersistirArchivo")]
    [Endpoint("PersistirArchivo")]
    [SwaggerOperation(
      Summary = "Nos permite subir archivo",
      Description = "Nos permite subir archivo")]
    public async Task<IActionResult> PersistirArchivo(
    [FromQuery]  int codigoVersion, string? usuarioSesion, string dominio, string referencia, string nombre, string extension)
    {
         
        var usuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        // Validar que el CI del usuario de sesión no sea nulo o vacío
        if (usuarioInserto==0)
        {
            throw new ReglasdeNegocioException("El CI del usuario de sesión no puede estar vacío");
        }

        // Validar si el archivo es lun archivo Excel

        if (dominio == "FORMATO_IMPORTACION_SOLICITUDES")
        {
                       
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest("El archivo adjunto no es un archivo Excel válido.");
            }

        }


        // Importar archivo utilizando el servicio de la capa de aplicación
        var resultado = await _service.PersistirArchivo( codigoVersion,usuarioInserto, dominio, referencia , nombre, extension);
        if (resultado != "Error")
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Message = "El archivo se procesó correctamente.",
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return NotFound(new ApiResponse<int>
            {
                Success = false,
                Message = "El archivo no se procesó correctamente.",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("BajarArchivo")]
    [Endpoint("BajarArchivo")]
    [SwaggerOperation(
     Summary = "Nos permite subir archivo",
     Description = "Nos permite subir archivo")]
    public async Task<ActionResult> BajarArchivo(int codigoVersion, string dominio)
    {
        var usuarioInserto = _usuarioService.DatosUsuario.NumeroDocumento;
       
        // Validar que el CI del usuario de sesión no sea nulo o vacío
        if (string.IsNullOrEmpty(usuarioInserto))
        {
            throw new ReglasdeNegocioException("El CI del usuario de sesión no puede estar vacío");
        }       

        // Importar archivo utilizando el servicio de la capa de aplicación
        var resultado = await _service.BajarArchivo(codigoVersion, usuarioInserto, dominio);

        if (resultado.nombre != null )
        {
            return Ok(new ApiResponse<ClaseBajarArchivo>
            {
                Data = resultado,
                Success = true,
                Message = "El archivo se procesó correctamente.",
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return NotFound(new ApiResponse<int>
            {
                Success = false,
                Message = "El archivo no se procesó correctamente.",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }

    }

    
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-312

    [HttpPost("SincronizarDBCapital")]
    [Endpoint("SincronizarDBCapital")]
    [SwaggerOperation(
           Summary = "Sincroniza la base de datos de la circunscripcion con la de capital.",
           Description = "Crea una copia de las tablas de versiones de la circunscripcion en la base de datos de capital.")]
    public async Task<ActionResult> SincronizarDBCapital(
    [FromQuery][Description("Valor que indica el codigo de version del Anteproyecto.")] int codigoVersion,
    [FromQuery][Description("Valor que indica el codigo dela Circunscripcion.")] int codigoCircunscripcion,  
    [FromQuery][Description("Valor que indica el codigo de la tarea a ejecutar.")] int codigoTarea)
    {
        var usuarioEjecucion = _usuarioService.DatosUsuario.NumeroDocumento;

        if (codigoVersion <= 0)
        {
            throw new ReglasdeNegocioException("El código de version debe ser mayor a cero.");
        }
        if (codigoTarea <= 0)
        {
            throw new ReglasdeNegocioException("El código de tarea debe ser mayor a cero.");
        }
        if (usuarioEjecucion.IsNullOrEmpty())
        {
            throw new ReglasdeNegocioException("Se necesita numero de cedula del usuario.");
        }
        var filasActualizadas = await _service.SincronizarDBCapital(codigoVersion, codigoCircunscripcion, usuarioEjecucion, codigoTarea);
        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
            });
        }

        throw new KeyNotFoundException();
    }

    #endregion
}


