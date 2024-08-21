using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.ConsolidadoAnteproyectoPresupuesto;
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
public class ConsolidadoAnteproyectoPresupuestoController : ControllerBase
{
  private readonly  IConsolidadoAnteproyectoPresupuestoService _service;
    private readonly IDatosUsuarioService _usuarioService;
    public ConsolidadoAnteproyectoPresupuestoController(IConsolidadoAnteproyectoPresupuestoService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }


    [HttpGet("ListarConsolidadoAnteproyectoporObjetoGasto")]
    [Endpoint("ListarConsolidadoAnteproyectoporObjetoGasto")]
    [SwaggerOperation(
         Summary = "Permite visualizar la consolidacion de anteproyecto por objeto de gasto",
         Description = "Permitir visualizar la consolidacion de anteproyecto por objeto de gasto")]
    public async Task<IActionResult> ListarConsolidadoAnteproyectoporObjetoGasto(
    [FromQuery][Description("Valores que indican las propiedades del Consolidado de Anteproyecto de Objetos")] ObjetoGastosConsolidadoAnteproyectoRequest request)
    { 

        var validationResult = new ObjetoGastosConsolidadoAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarConsolidadoAnteproyectoporObjetoGasto(request);

        if (resultado == null || resultado.Count() == 0)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ObjetoGastosConsolidadoAnteproyectoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<ObjetoGastosConsolidadoAnteproyectoDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarBienesConsolidadoAnteproyecto")]
    [Endpoint("ListarBienesConsolidadoAnteproyecto")]
    [SwaggerOperation(
     Summary = "Permite visualizar los bienes de la consolidacion de anteproyecto de presupuesto",
     Description = "Permitir visualizar los bienes de la consolidacion de anteproyecto de presupuesto")]
    public async Task<IActionResult> ListarBienesConsolidadoAnteproyecto(
    [FromQuery][Description("Valores que indican las propiedades para obtener los bienes del Consolidado de Anteproyecto de Presupuesto")] BienesdelConsolidadoAnteproyectoRequest request)
    {

        var validationResult = new BienesConsolidadoAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarBienesdelConsolidadoAnteproyectoPresupuestario(request);

        if (resultado == null || resultado.TotalRegistros == 0)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<BienesdelConsolidadoAnteproyectoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpPut("ModificarBiendeVersionAnteproyectoBien")]
    [Endpoint("ModificarBiendeVersionAnteproyectoBien")]
    [SwaggerOperation(
    Summary = "Permite modificar el bien de la tabla de Anteproyecto Bienes",
    Description = "Permitir modificar el bien de la tabla de Anteproyecto Bienes")]
    public async Task<IActionResult> ModificarBiendeVersionAnteproyectoBien(
    [FromBody][Description("Valores que indican las propiedades paramodificar el bien de la tabla de anteproyecto bienes")] ModificarBienesVersionAnteproyectoRequest request)
    {

        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new ModificarBienesVersionAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ModificarBiendeVersionAnteproyecto(request);

        if (resultado == 0)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpDelete("EliminarBiendeVersionAnteproyectoBien")]
    [Endpoint("EliminarBiendeVersionAnteproyectoBien")]
    [SwaggerOperation(
        Summary = "Permite eliminar el bien de la tabla de Anteproyecto Bienes",
        Description = "Permitir eliminar el bien de la tabla de Anteproyecto Bienes")]
    public async Task<IActionResult> EliminarBiendeVersionAnteproyectoBien(
    [FromBody][Description("Valores que indican las propiedades para eliminar el bien de la tabla de anteproyecto bienes")] EliminarBienVersionAnteproyectoRequest request)
    {

        var validationResult = new EliminarBienVersionAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.EliminarBiendeVersionAnteproyecto(request);

        if (resultado == 0)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarObjetosGastosVersionAnteproyecto")]
    [Endpoint("ListarObjetosGastosVersionAnteproyecto")]
    [SwaggerOperation(
         Summary = "Permite visualizar los objetos de gastos de la version anteproyectos",
         Description = "Permitir visualizar los objetos de gastos de la version anteproyectos")]
    public async Task<IActionResult> ListarObjetosGastosVersionAnteproyecto(
    [FromQuery][Description("Valores que indican las propiedades para obtener los objetos de gastos de la version Anteproyecto")] ObjetodeGastoVersionAnteproyectoRequest request)
    {

        var validationResult = new ObjetodeGastoVersionAnteproyectoRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarObjetosdeGastosdeVersionesAnteproyectos(request);

        if (resultado == null || resultado.Count() == 0)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ObjetodeGastoVersionAnteproyectoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<ObjetodeGastoVersionAnteproyectoDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpDelete("GestionarEliminarObjetoGastoVersionesAnteproyecto")]
    [Endpoint("GestionarEliminarObjetoGastoVersionesAnteproyecto")]
    [SwaggerOperation(
    Summary = "Permite gestionar eliminar Objeto Gastos de Versiones anteproyecto y sus implicancias",
    Description = "Permitir gestionar eliminar Objeto Gastos de Versiones anteproyecto y sus implicancias")]
    public async Task<IActionResult> GestionarEliminarObjetoGastoVersionesAnteproyecto(
    [FromBody][Description("Valores que indican las propiedades para la gestion de eliminar objeto de gasto de versiones anteproyecto")] ObjetodeGastoVersionAnteproyectoListRequest request)
    {
        var validationResult = new ObjetodeGastoVersionAnteproyectoListRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.GestionarEliminarObjetoGastoVersionesAnteproyecto(request.Items);

        if (resultado == 0)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }
}
