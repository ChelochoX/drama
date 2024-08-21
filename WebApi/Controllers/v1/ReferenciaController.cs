using Application.Services.Interfaces;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.Entities;
using Domain.Entities.Referencias;
using Domain.Entities.Request;
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
public class ReferenciaController : ControllerBase
{
    private readonly IReferenciaService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public ReferenciaController(IReferenciaService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }
    [HttpGet("ObtenerTipoReferencia")]
    [Endpoint("ObtenerTipoReferencia")]
    [SwaggerOperation(
    Summary = "Permite visualizar los tipos de referencia",
    Description = "Listar todos los tipos de referencia")]
    public async Task<IActionResult> ObtenerTiposDeReferencia(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica la descripción del tipo de referencia")] string? descripcionTipoReferencia,
    [FromQuery][Description("Valor que indica el dominio del tipo de referencia")] string? dominioTipoReferencia,
    [FromQuery][Description("Valor que indica el estado del tipo de referencia")] string? estado)
    {

        var request = new TipoReferenciasRequest
        {
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            TerminoDeBusqueda = terminoDeBusqueda,
            DescripcionTipoReferencia = descripcionTipoReferencia,
            DominioTipoReferencia = dominioTipoReferencia,
            Estado = estado
        };

        var resultado = await _service.ObtenerTiposReferencias(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<TipoReferenciaDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<TipoReferenciaDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ObtenerDescripcionTipoReferencia")]
    [Endpoint("ObtenerDescripcionTipoReferencia")]
    [SwaggerOperation(
    Summary = "Permite visualizar descripciones de tipos de referencia",
    Description = "Listar todas las descripciones de tipos de referencia")]
    public async Task<ActionResult<List<TipoReferenciaDTO>>> ObtenerDescripcionTipoReferencia(int estado, int codigoTipoReferencia, string? descripcionTipoReferencia = null)
    {

        var listaDatos = await _service.ObtenerDescripcionTiposReferencias(descripcionTipoReferencia, codigoTipoReferencia, estado);
        if (listaDatos.Any())
        {
            return Ok(new ApiResponse<List<TipoReferenciaDTO>>
            {
                Success = true,
                Data = (List<TipoReferenciaDTO>)listaDatos,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<TipoReferenciaDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }



    [HttpGet("ObtenerDominioTipoReferencia")]
    [Endpoint("ObtenerDominioTipoReferencia")]
    [SwaggerOperation(
    Summary = "Permite visualizar dominios de tipos de referencia",
    Description = "Listar todos los dominios de tipos de referencia")]
    public async Task<ActionResult<List<TipoReferenciaDTO>>> ObtenerDominioTipoReferencia(int estado, int codigoTipoReferencia, string? dominioTipoReferencia = null)
    {

        var listaDatos = await _service.ObtenerDominioTiposReferencias(dominioTipoReferencia, codigoTipoReferencia, estado);
        if (listaDatos.Any())
        {
            return Ok(new ApiResponse<List<TipoReferenciaDTO>>
            {
                Success = true,
                Data = (List<TipoReferenciaDTO>)listaDatos,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<TipoReferenciaDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ObtenerReferencias")]
    [Endpoint("ObtenerReferencias")]
    [SwaggerOperation(
         Summary = "Permite visualizar las referencias",
         Description = "Listar todas las referencias según parámetros")]
    public async Task<ActionResult<List<ReferenciasDTO>>> ObtenerReferencias(int codigoTipoReferencia, int estado, int pagina, int cantidadRegistros, decimal? valorDecimal = 0,
                                                                                     string? dominioReferencia = null, string? descripcionReferencia = null, string? valorAlfanumerico = null,
                                                                                     string? descripcionLarga = null, DateTime? valorFecha = null)
    {
        var listaDatos = await _service.ObtenerReferencias(pagina, cantidadRegistros, dominioReferencia, codigoTipoReferencia, descripcionReferencia, estado, valorFecha ?? DateTime.MinValue,
                                                           valorAlfanumerico, descripcionLarga, valorDecimal);

        if (listaDatos.Any())
        {
            return Ok(new ApiResponse<List<ReferenciasDTO>>
            {
                Success = true,
                Data = (List<ReferenciasDTO>)listaDatos,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ReferenciasDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarReferencias")]
    [Endpoint("ListarReferencias")]
    [SwaggerOperation(
         Summary = "Permite visualizar las referencias",
         Description = "Listar todas las referencias según parámetros")]
    public async Task<IActionResult> ListarReferencias(
        [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? dominioReferencia,
    [FromQuery][Description("Valor que indica el valor del codigo del tipo de referencia")] int? codigoTipoReferencia,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? descripcionReferencia,
    [FromQuery][Description("Valor que indica el valor del estado del objeto de la referencia")] string? estado,
    [FromQuery][Description("Valor que indica el valor de la fecha")] DateTime? valorFecha,
    [FromQuery][Description("Valor que indica el valor alfanumerico de la referencia")] string? valorAlfanumerico,
    [FromQuery][Description("Valor que indica la descripcion larga de la referencia")] string? descripcionLarga,
    [FromQuery][Description("Valor que indica la descripcion tipo de la referencia")] string? descripcionTipoReferencia,
    [FromQuery][Description("Valor que indica el valor decimal de la referencia")] decimal? valorDecimal)
    {

        var request = new ReferenciasRequest
        {
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            TerminoDeBusqueda = terminoDeBusqueda,
            DominioReferencia = dominioReferencia,
            CodigoTipoReferencia = codigoTipoReferencia,
            DescripcionReferencia = descripcionReferencia,
            Estado = estado,
            ValorFecha = valorFecha,
            ValorAlfanumerico = valorAlfanumerico,
            DescripcionLarga = descripcionLarga,
            ValorDecimal = valorDecimal,
            DescripcionTipoReferencia= descripcionTipoReferencia,
        };

        var validationResult = new ReferenciasRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);           
        }

        var resultado = await _service.ObtenerListadoReferencias(request);

       if(resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ReferenciasDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<ReferenciasDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });

    }


    [HttpGet("ObtenerDominioReferencias")]
    [Endpoint("ObtenerDominioReferencias")]
    [SwaggerOperation(
         Summary = "Permite visualizar dominios de referencias",
         Description = "Listar todos los dominios de referencias")]
    public async Task<ActionResult<List<ReferenciasDTO>>> ObtenerDominioReferencias(int estado, int codigoReferencia, string? dominioReferencia = null)
    {

        var listaDatos = await _service.ObtenerDominioReferencias(dominioReferencia, codigoReferencia, estado);
        if (listaDatos.Any())
        {
            return Ok(new ApiResponse<List<ReferenciasDTO>>
            {
                Success = true,
                Data = (List<ReferenciasDTO>)listaDatos,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ReferenciasDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ObtenerDescripcionReferencias")]
    [Endpoint("ObtenerDescripcionReferencias")]
    [SwaggerOperation(
         Summary = "Permite visualizar descripciones de referencias",
         Description = "Listar todas las descripciones de referencias")]
    public async Task<ActionResult<List<ReferenciasDTO>>> ObtenerDescripcionReferencias(int estado, int codigoReferencia, string? descripcionReferencia = null)
    {

        var listaDatos = await _service.ObtenerDescripcionReferencias(descripcionReferencia, codigoReferencia, estado);
        if (listaDatos.Any())
        {
            return Ok(new ApiResponse<List<ReferenciasDTO>>
            {
                Success = true,
                Data = (List<ReferenciasDTO>)listaDatos,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ReferenciasDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpPost("insertarTipo")]
    [Endpoint("insertarTipo")]
    [SwaggerOperation(
     Summary = "Nos permite insertar informacion del tipo de referencia",
     Description = "Inserta información del tipo de referencia")]
    public async Task<IActionResult> InsertarTipoReferencia([FromBody] TipoReferencias tipoReferencia)
    {
        tipoReferencia.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = await _service.InsertarTipoReferencia(tipoReferencia);

        if (resultado != null && resultado.TotalRegistros != -1)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        if (resultado != null && resultado.TotalRegistros == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ya existe un tipo de referencia con el dominio ingresado.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo agregar el tipo de contrato.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpPost("insertar")]
    [Endpoint("insertar")]
    [SwaggerOperation(
     Summary = "Nos permite insertar informacion de la referencia",
     Description = "Inserta información de la referencia")]
    public async Task<IActionResult> InsertarReferencia([FromBody] Referencias referencias)
    {
        referencias.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = await _service.InsertarReferencia(referencias);

        if (resultado != null && resultado.TotalRegistros != -1)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        if (resultado != null && resultado.TotalRegistros == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ya existe una referencia con el dominio ingresado.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo agregar la referencia.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }



    [HttpPut("modificarTipo")]
    [Endpoint("modificarTipo")]
    [SwaggerOperation(
     Summary = "Nos permite modificar el tipo de referencia",
     Description = "Modifica información del tipo de referencia")]
    public async Task<IActionResult> ModificarTipoReferencia([FromBody] TipoReferencias tipoReferencia)
    {
        tipoReferencia.UsuarioModificacion = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = await _service.ModificarTipoReferencia(tipoReferencia);

        if (resultado != null && resultado.TotalRegistros != -1)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        if (resultado != null && resultado.TotalRegistros == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ya existe un tipo de referencia con el dominio ingresado.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo modificar el tipo de referencia.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpPut("modificar")]
    [Endpoint("modificar")]
    [SwaggerOperation(
     Summary = "Nos permite modificar la referencia",
     Description = "Modifica información de la referencia")]
    public async Task<IActionResult> ModificarReferencia([FromBody] Referencias referencias)
    {
        referencias.UsuarioModificacion = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = await _service.ModificarReferencia(referencias);

        if (resultado != null && resultado.TotalRegistros != -1)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
                if (resultado != null && resultado.TotalRegistros == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ya existe una referencia con el dominio ingresado.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo modificar la referencia.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }
}
