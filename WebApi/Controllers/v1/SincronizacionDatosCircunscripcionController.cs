using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Request;
using Domain.Entities.SincronizacionDatosCircunscripcion;
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
public class SincronizacionDatosCircunscripcionController : ControllerBase
{
    private readonly ISincronizacionDatosCircunscripcionService _service;

    public SincronizacionDatosCircunscripcionController(ISincronizacionDatosCircunscripcionService service)
    {
        _service = service;
    }

    [HttpGet("ListarTareasporLoteporCircunscripcion")]
    [Endpoint("ListarTareasporLoteporCircunscripcion")]
    [SwaggerOperation(
    Summary = "Permite listar las tareas de sincronizacion de datos por las circunscripciones",
    Description = "Permitir listar las tareas de sincronizacion de datos por las circunscripciones")]
    public async Task<IActionResult> ListarTareasporLoteporCircunscripcion(
    [FromQuery][Description("Valores que indican las propiedades necesarias para obtener los datos de versiones anteproyecto bienes")]
    SincronizacionDatosRequest request)
    {
        var validationResult = new SincronizacionDatosRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerDatosOrganismoFinanciador(request);

        if (resultado == null || resultado.TotalRegistros == 0)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<SincronizacionDatosDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });         
        }
        else
        {

             return Ok(new ApiResponse<Datos<IEnumerable<SincronizacionDatosDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
    }
}
