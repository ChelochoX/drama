using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;
using WebApi.ValidationHandlers;

namespace WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ConectarCircunscripcionController : ControllerBase
    {
        private readonly IConectarCircunscripcionService _service;

        public ConectarCircunscripcionController(IConectarCircunscripcionService service)
        {
            _service = service;
        }


        [HttpGet("ListarDatosCircunscripcion")]
        [Endpoint("ListarDatosCircunscripcion")]
        [SwaggerOperation(
           Summary = "Permite Obtener los datos de la Circunscrion para la conexion",
           Description = "Permitir Obtener los datos de la Circunscrion para la conexion")]
        public async Task<IActionResult> ListarDatosCircunscripcion(
        [FromQuery][Description("Valor que indica el codigo de la circunscripcion")] int codigoCircunscripcion)
        {
            if (codigoCircunscripcion == null || codigoCircunscripcion == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Valor de Codigo Circunscripcion es Obligatoria",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }

            var resultado = await _service.ObtenerDatosparaConectarCircunscripcion(codigoCircunscripcion);

            if (resultado == null)
            {
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Data = new List<string>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
    }
}
