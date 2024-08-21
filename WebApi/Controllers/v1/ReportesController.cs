using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Server;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;
using WebApi.ValidationHandlers;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public ReportesController(IReportesService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }

    [HttpGet("GenerarReporteFG03ResumenMateria")]
    [Endpoint("GenerarReporteFG03ResumenMateria")]
    [SwaggerOperation(
        Summary = "Permite generar el reporte del FG03",
        Description = "Permitir generar el reporte del FG03")]
    public async Task<IActionResult> GenerarReporteFG03ResumenMateria(
      [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion,
      [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (codigoVersion <= 0 || string.IsNullOrWhiteSpace(usuario) || ejercicio <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, usuario y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new FG03Request
        {
            CodigoVersion = codigoVersion,
            Usuario = _usuarioService.DatosUsuario.NombreUsuario,
            Ejercicio = ejercicio
        };

        try
        {
            var resultado = await _service.GenerarReporteFG03(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG07")]
    [Endpoint("GenerarReporteFG07")]
    [SwaggerOperation(
       Summary = "Permite generar el reporte del FG07",
       Description = "Permitir generar el reporte del FG07")]
    public async Task<IActionResult> GenerarReportFG07(
     [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion,
     [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (codigoVersion <= 0 || string.IsNullOrWhiteSpace(usuario))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        if (ejercicio <= 0 || string.IsNullOrWhiteSpace(usuario))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new FG07Request
        {
            CodigoVersion = codigoVersion,
            Usuario = usuario,
            Ejercicio = ejercicio
        };

        try
        {
            var resultado = await _service.GenerarReporteFG07(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG07.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteSolicitudesBienes")]
    [Endpoint("GenerarReporteSolicitudesBienes")]
    [SwaggerOperation(
       Summary = "Permite generar el reporte de Solicitudes Bienes",
       Description = "Permitir generar el reporte de Solicitudes Bienes")]
    public async Task<IActionResult> GenerarSolicitudesBienes(
     [FromQuery][Description("Valor que indica el codigo de la version")] int codigoSolicitud,
     [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (codigoSolicitud <= 0 || string.IsNullOrWhiteSpace(usuario) || ejercicio <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, usuario y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new SolicitudesBienesRequest
        {
            CodigoSolicitud = codigoSolicitud,
            Usuario = usuario,
            Ejercicio = ejercicio
        };

        try
        {
            var resultado = await _service.GenerarReporteSolicitudesBienes(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG03FuenteFinanciacionCircunscripcion")]
    [Endpoint("GenerarReporteFG03FuenteFinanciacionCircunscripcion")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte de FG03-FF",
    Description = "Permitir generar el reporte de FG03-FF")]
    public async Task<IActionResult> GenerarReporteFG03FF(
    [FromQuery][Description("Valor que indica el codigo de la version")] int? codigoFF,
    [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (string.IsNullOrWhiteSpace(usuario) || ejercicio <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, usuario y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new FG03FFRequest
        {
            CodigoFuenteFinanciamiento = codigoFF,
            Usuario = usuario,
            Ejercicio = ejercicio           
        };

        try
        {
            var resultado = await _service.GenerarReporteFG03FF(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG03FuenteFinanciacionGeneral")]
    [Endpoint("GenerarReporteFG03FuenteFinanciacionGeneral")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte de FG03-FF General",
    Description = "Permitir generar el reporte de FG03-FF General")]
    public async Task<IActionResult> GenerarReporteFG03FFGeneral(
    [FromQuery][Description("Valor que indica el codigo de la fuente de financiación")] int? codigoFF,
    [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio,
    [FromQuery][Description("Valor que indica el código de circunscripción")] int codigoCircunscripcion)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (string.IsNullOrWhiteSpace(usuario) || ejercicio <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, usuario y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new FG03FFGeneralRequest
        {
            CodigoFuenteFinanciamiento = codigoFF,
            Usuario = usuario,
            Ejercicio = ejercicio,
            CodigoCircunscripcion = codigoCircunscripcion
        };

        try
        {
            var resultado = await _service.GenerarReporteFG03FFGeneral(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG02")]
    [Endpoint("GenerarReporteFG02")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte del FG02",
    Description = "Permitir generar el reporte del FG02")]
    public async Task<IActionResult> GenerarReportFG02(
    [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion)
    {
        var usuario = _usuarioService.DatosUsuario.NombreUsuario;

        if (codigoVersion <= 0 || string.IsNullOrWhiteSpace(usuario))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parámetros inválidos. Verifique los valores de codigoVersion, usuario y ejercicio.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var request = new FG02Request
        {
            CodigoVersion = codigoVersion,
            Usuario = usuario
        };

        try
        {
            var resultado = await _service.GenerarReporteFG02(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG02.pdf");

        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG03Consolidado")]
    [Endpoint("GenerarReporteFG03Consolidado")]
    [SwaggerOperation(
     Summary = "Permite generar el reporte del FG03 Consolidado",
     Description = "Permitir generar el reporte del FG03 Consolidado")]
    public async Task<IActionResult> GenerarReporteFG03Consolidado(   
    [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte FG03 Consolidado")] FG03Consolidado request)
    {
        request.Usuario =  _usuarioService.DatosUsuario.NombreUsuario;

        var validationResult = new FG03ConsolidadoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var resultado = await _service.GenerarReporteFG03_Consolidado(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03_Consolidado.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteFG02Consolidado")]
    [Endpoint("GenerarReporteFG02Consolidado")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte del FG02 Consolidado",
    Description = "Permitir generar el reporte del FG02 Consolidado")]
    public async Task<IActionResult> GenerarReporteFG02Consolidado(
   [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte FG02 Consolidado")] FG03Consolidado request)
    {
        request.Usuario = _usuarioService.DatosUsuario.NombreUsuario;

        var validationResult = new FG03ConsolidadoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var resultado = await _service.GenerarReporteFG02_Consolidado(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG02_Consolidado.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReportePorGrupoGasto")]
    [Endpoint("GenerarReportePorGrupoGasto")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte del Grupo Gastos",
    Description = "Permitir generar el reporte del Grupo Gastos")]
    public async Task<IActionResult> GenerarReportePorGrupoGasto(
    [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte Grupo Gasto")] ReportePorGrupoGasto request)
    {
        request.Usuario = _usuarioService.DatosUsuario.NombreUsuario; 

        var validationResult = new ReportePorGrupoGastoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {

            var resultado = await _service.GenerarReportePorGrupoGasto(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "GrupoGastos.pdf");

        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }



    [HttpGet("GenerarFG03FuenteFinanciacionCircunscripcion")]
    [Endpoint("GenerarFG03FuenteFinanciacionCircunscripcion")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte del FG03_FuenteFinanciacion_Circunscripcion",
    Description = "Permitir generar el reporte del FG03_FuenteFinanciacion_Circunscripcion")]
    public async Task<IActionResult> GenerarFG03FuenteFinanciacionCircunscripcion(
    [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte FG03FuenteFinanciacionCircunscripcion")] FG03FuenteFinanciacionCircunscripcion request)
    {
        request.Usuario = _usuarioService.DatosUsuario.NombreUsuario;

        var validationResult = new FG03FuenteFinanciacionCircunscripcionValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var resultado = await _service.GenerarFG03FuenteFinanciacionCircunscripcion(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "FG03_FuenteFinanciacion_Circunscripcion.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }
    

    [HttpGet("GenerarJustificacionCreditosPresupuestarios")]
    [Endpoint("GenerarJustificacionCreditosPresupuestarios")]
    [SwaggerOperation(
    Summary = "Permite generar el reporte del JustificacionCreditosPresupuestarios",
    Description = "Permitir generar el reporte del JustificacionCreditosPresupuestarios")]
    public async Task<IActionResult> GenerarJustificacionCreditosPresupuestarios(
    [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte JustificacionCreditosPresupuestarios")] JustificacionCreditosPresupuestarios request)
    {
        request.Usuario = _usuarioService.DatosUsuario.NombreUsuario;

        var validationResult = new JustificacionCreditosPresupuestariosValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var resultado = await _service.GenerarJustificacionCreditosPresupuestarios(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "JustificacionCreditosPresupuestarios.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }


    [HttpGet("GenerarReporteVersionadoAnteproyecto")]
    [Endpoint("GenerarReporteVersionadoAnteproyecto")]
    [SwaggerOperation(
     Summary = "Permite generar el reporte del Versionado Anteproyecto",
     Description = "Permitir generar el reporte del Versionado Anteproyecto")]
    public async Task<IActionResult> GenerarReporteVersionadoAnteproyecto(
    [FromQuery][Description("Valores que indican la propiedades necesarias para la generacion del Reporte Versionado Anteproyecto")]
    ConsultaVersionadoAnteproyecto request)
    {
        request.Usuario = _usuarioService.DatosUsuario.NombreUsuario;

        var validationResult = new ConsultaVersionadoAnteproyectoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var resultado = await _service.GenerarReporteVersionadoAnteproyecto(request);

            if (resultado == null || resultado.Length == 0)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El reporte solicitado no se ha generado o no fue encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return File(resultado, "application/pdf", "consultaVersionadoAnteproyecto.pdf");
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al generar el reporte",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Ocurrió un error inesperado",
                StatusCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

}
