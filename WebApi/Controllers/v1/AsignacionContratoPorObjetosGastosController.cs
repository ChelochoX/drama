using Application.Services.Interfaces;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.DTOs;
using Domain.Entities;
using Domain.Entities.AsignacionContratos;
using Domain.Entities.Request;
using Domain.Entities.Request.AsignacionContratos;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Net;
using WebApi.ValidationHandlers;


namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AsignacionContratoPorObjetosGastosController : ControllerBase
{
    private readonly IAsignacionContratoPorObjetosGastosService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public AsignacionContratoPorObjetosGastosController(IAsignacionContratoPorObjetosGastosService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }


    [HttpGet("ObtenerObjetoGastoPorContrato")]
    [Endpoint("ObtenerObjetoGastoPorContrato")]
    [SwaggerOperation(
         Summary = "Permite visualizar los objetos de gasto por asignacion de contrato",
         Description = "Listar todos los objetos de gasto por asignacion de contrato")]
    public async Task<IActionResult> ObtenerObjetoGastoPorContrato(
    [FromQuery][Description("Valor que indica el codigo de anteproyecto del objeto")] int codigoAnteproyectoObjeto,
    [FromQuery][Description("Valor que indica el codigo de version")] int codigoVersion
    )
    {
        var resultado = await _service.ObtenerObjetosdeGastoPorContrato(codigoAnteproyectoObjeto, codigoVersion);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ObjetosdeGastoPorContratoAsignadoDTO>>>
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
                Data = new List<ObjetosdeGastoPorContratoAsignadoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ObtenerContrato")]
    [Endpoint("ObtenerContrato")]
    [SwaggerOperation(
         Summary = "Obtiene el contrato por su codigo.",
         Description = "Obtener contrato por su numero.")]
    public async Task<IActionResult> ObtenerContrato(
    [FromQuery][Description("Valor que indica el numero del contrato.")] string numeroContrato)
    {
        var resultado = await _service.ObtenerContrato(numeroContrato);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>>
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
                Data = new List<ContratoPorObjetosdeGastoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }
    [HttpGet("ObtenerContratoAsignado")]
    [SwaggerOperation(
         Summary = "Permite visualizar los objetos de gasto por asignacion de contrato",
         Description = "Listar todos los objetos de gasto por asignacion de contrato")]
    public async Task<IActionResult> ObtenerContratoAsignado(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? numeroContrato,    
    [FromQuery][Description("Valor que indica el valor del codigo del tipo de referencia")] string? pacPrepac,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? administradorContrato,
    [FromQuery][Description("Valor que indica el valor del estado del objeto de la referencia")] string? tipoContrato,
    [FromQuery][Description("Valor que indica el valor de la fecha")] string? montoContrato,
    [FromQuery][Description("Valor que indica el valor de la fecha")] int codigoVersion,
    [FromQuery][Description("Valor que indica el valor de la fecha")] int codigoAnteproyectoObjeto,
    [FromQuery][Description("Valor que indica el valor de descripcion contrato")] string? descripcionContrato
    )
    {
        var request = new ContratoAsignadoRequest
        {
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            TerminoDeBusqueda = terminoDeBusqueda,
            NumeroContrato = numeroContrato,
            PacPrepac = pacPrepac,
            AdministradorContrato = administradorContrato,
            TipoContrato = tipoContrato,
            MontoContrato = montoContrato,
            CodigoVersion = codigoVersion,            
            CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
            DescripcionContrato = descripcionContrato
        };

        var resultado = await _service.ObtenerContratoAsignado(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>>
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
                Data = new List<ContratoPorObjetosdeGastoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpPost("AgregarContrato")]
    [Endpoint("AgregarContrato")]
    [SwaggerOperation(
         Summary = "Permite visualizar los contratos asignados por objeto de gasto",
         Description = "Listar todos los contratos asignados por objeto de gasto")]
    public async Task<IActionResult> AgregarContrato(
        [FromBody][Description("Datos que corresponden al contrato a asignar")] ContratoPorObjetosdeGasto contrato)
    {
        contrato.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = await _service.InsertarContrato(contrato);
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
                Message = "El contrato seleccionado ya ha sido asignado.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo agregar el contrato.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpPut("ModificarContrato")]
    [Endpoint("ModificarContrato")]
    [SwaggerOperation(
         Summary = "Permite modificar contratos asignados por objeto de gasto",
         Description = "Modificar todos los contratos asignados por objeto de gasto")]
    public async Task<IActionResult> ModificarContrato(
        [FromBody][Description("Datos que corresponden al contrato a modificar")] ContratoporObjetosdeGastoRequest request)
    {
        request.UsuarioModificacion = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var filasActualizadas = await _service.ModificarContrato(request);
        if (filasActualizadas != null)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = filasActualizadas,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo modificar el contrato.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpDelete("EliminarContrato")]
    [Endpoint("EliminarContrato")]
    [SwaggerOperation(
         Summary = "Permite crear un nuevo contrato para asignar a un objeto de gasto",
         Description = "Crear un nuevo contrato para asignar a un objeto de gasto")]
    public async Task<IActionResult> EliminarContrato(
        [FromQuery][Description("Valor que indica el codigo de anteproyecto del contrato asignado al objeto")] int codigoAnteproyectoContrato,
        [FromQuery][Description("Valor que indica el codigo de la version del anteproyecto del contrato asignado al objeto")] int codigoVersion,
        [FromQuery][Description("Valor que indica el codigo de objeto anteproyecto del contrato")] int codigoAnteproyectoObjeto)
    {
        var resultado = await _service.EliminarContrato(codigoAnteproyectoContrato, codigoVersion, codigoAnteproyectoObjeto);

        if (resultado != null)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }
        else
        {
            return NotFound(new ApiResponse<object>
            {
                Success = true,
                Message = "El recurso solicitado no fue encontrado.",
                StatusCode = (int)HttpStatusCode.NotFound
            });
        }
    }
    


    [HttpGet("ListarContratos")]
    [Endpoint("ListarContratos")]
    [SwaggerOperation(
         Summary = "Permite hacer la busqueda de los contratos disponibles.",
         Description = "Listar todos los contratos para asignar.")]
    public async Task<IActionResult> ListarContratos(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? numeroContrato,
     [FromQuery][Description("Valor que indica el dominio de la referencia")] string? numeroContratacion,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? descripcionContrato,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? codigoContrato,
    [FromQuery][Description("Valor que indica el valor del codigo del tipo de referencia")] string? pacPrepac,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? administradorContrato,
    [FromQuery][Description("Valor que indica el valor del estado del objeto de la referencia")] string? tipoContrato,
    [FromQuery][Description("Valor que indica el valor de la fecha")] string? montoContrato)
    {
        var request = new ContratosRequest
        {
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            TerminoDeBusqueda = terminoDeBusqueda,
            NumeroContrato = numeroContrato,
            PacPrepac = pacPrepac,
            AdministradorContrato = administradorContrato,
            TipoContrato = tipoContrato,
            CodigoContrato = codigoContrato,
            NumeroContratacion = numeroContratacion,
            DescripcionContrato = descripcionContrato,
            MontoContrato = montoContrato
        };

        var validationResult = new ContratosRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarContratos(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>>
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
                Data = new List<ContratoPorObjetosdeGastoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }

 
    
    [HttpPost("RegistroFechaCierreResolucion")]
    [Endpoint("RegistroFechaCierreResolucion")]
    [SwaggerOperation(
            Summary = "Permite el Registro de Fecha de Cierre de Resolución",
            Description = "Registro de Fecha de Cierre de Resolución")]
    public async Task<IActionResult> RegistroFechaCierreResolucion(
           [FromBody][Description("Datos que corresponden al Registro de Fecha de Cierre de Resolución")] FechaCierreResolucion request)
    {
        request.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var resultado = _service.RegistroFechaCierreResolucion(request);
        if (resultado != null)
        {
            return Ok(new ApiResponse<Datos<int>>
            {
                Success = true,
                Message = "Registro de Fecha de Cierre de Resolución creado correctamente.",
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        {
            return BadRequest(new ApiResponse<Datos<int>>
            {
                Success = false,
                Message = "No se pudo agregar el contrato.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpGet("ListadoFechaCierreResolucion")]
    [Endpoint("ListadoFechaCierreResolucion")]
    [SwaggerOperation(
         Summary = "Permite hacer la busqueda de los registros de fecha de cierre.",
         Description = "Listar todos los los registros de fecha de cierre.")]
    public async Task<IActionResult> ListadoFechaCierreResolucion(
        [FromQuery][Description("Datos paginado")] ListarFechaCierreResolucionRequest request)
    {
        var validationResult = new ListarFechaCierreResolucionRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoFechaCierreResolucion(request);

        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ListarFechaCierreResolucion>>>
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
                Data = new List<ContratoPorObjetosdeGastoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ObtenerListadoNotificacionesUsuario")]
    [Endpoint("ObtenerListadoNotificacionesUsuario")]
    [SwaggerOperation(
       Summary = "Permite hacer la busqueda de los registros de fecha de cierre.",
       Description = "Listar todos los los registros de fecha de cierre.")]
    public async Task<IActionResult> ObtenerListadoNotificacionesUsuario([FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica el dominio de la referencia")] string? mensaje,
    [FromQuery][Description("Valor que indica el valor del codigo del tipo de referencia")] string? fechaNotificacion,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? circunscripcionOrigen,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? usuario,
    [FromQuery][Description("Valor que indica la descripcion de la referencia")] string? userName,
    [FromQuery][Description("Valor que indica el tipo de notificacion")] string? tipoNotificacion
  )
    {
        var usuarioCedula = _usuarioService.DatosUsuario.NumeroDocumento;
        var request = new ListadoNotificacionesUsuarioRequest
        {
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            TerminoDeBusqueda = terminoDeBusqueda,
            Mensaje = mensaje,
            FechaNotificacion = fechaNotificacion,
            CircunscripcionOrigen = circunscripcionOrigen,
            Usuario = usuarioCedula,
            UserName = userName,
            TipoNotificacion = tipoNotificacion

        };

        var validationResult = new ListadoNotificacionesUsuarioRequestValidator().Validate(request);


        var resultado = await _service.ObtenerListadoNotificacionesUsuario(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ListadoNotificacionesUsuarioDTO>>>
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
                Data = new List<ListadoNotificacionesUsuarioDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

       

    }


    [HttpGet("CantidadNotificacionesAbiertas")]
    [Endpoint("CantidadNotificacionesAbiertas")]
    [SwaggerOperation(
    Summary = "Nos permite obtener los totales de bienes y montos",
    Description = "Obtiene información de los totales de bienes y montos.")]
    public async Task<ActionResult<int>> CantidadNotificacionesAbiertas()
    {
        var usuario = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        var listado = await _service.CantidadNotificacionesAbiertas(usuario);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron Notificaciones abiertas.");
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

}
