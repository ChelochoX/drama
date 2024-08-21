using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Request;
using Domain.Entities.VersionadoAnteproyecto;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;

namespace WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class VersionadoAnteproyectoController : ControllerBase
    {
        private readonly IVersionadoAnteproyectoService _service;

        public VersionadoAnteproyectoController(IVersionadoAnteproyectoService service)
        {
            _service = service;
        }

        [HttpGet("ListarVersionadoAnteproyecto")]
        [SwaggerOperation(
         Summary = "Permite Listar el versionado de anteproyecto",
         Description = "Permitir  Listar el versionado de anteproyecto")]
        public async Task<IActionResult> ListarVersionadoAnteproyecto(
        [FromQuery][Description("Valores que indican las propiedades para obtener el versionado de anteproyecto")] VersionadoAnteproyectoRequest request)
        {

            var validationResult = new VersionadoAnteproyectoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ListarVersionadodeAnteproyecto(request);

            if (resultado == null && resultado.Items.Any())
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<VersionadoAnteproyectoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<Datos<IEnumerable<VersionadoAnteproyectoDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ListarVersionesdelAnteproyecto")]
        [SwaggerOperation(
        Summary = "Permite Listar las versiones que existan en el anteproyecto de versionado para el ejercicio actual",
        Description = "Permitir  Listar las versiones que existan en el anteproyecto de versionado para el ejercicio actual")]
        public async Task<IActionResult> ListarVersionesdelAnteproyecto(
       [FromQuery][Description("Valor que indica el ejercicio actual del anteproyecto")] int ejercicio)
        {

            if (ejercicio == 0) 
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El parametro ejercicio es obligatorio",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        

            var resultado = await _service.ObtenerVersionesporEjercicio(ejercicio);

            if (resultado == null || resultado.Count() == 0)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<VersionesAnteproyectoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<VersionesAnteproyectoDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }

    }
}
