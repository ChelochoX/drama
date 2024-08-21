using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.PlanificacionFinanciera;
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
public class PlanificacionFinancieraController : ControllerBase
{
    private readonly IPlanificacionFinancieraService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public PlanificacionFinancieraController(IPlanificacionFinancieraService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }


    [HttpGet("ListarVersionesAnteproyectos")]
    [Endpoint("ListarVersionesAnteproyectos")]
    [SwaggerOperation(
       Summary = "Permite listar las versiones de los anteproyectos realizados",
       Description = "Permitir listar las versiones de los anteproyectos realizados")]
    public async Task<IActionResult> ListarVersionesAnteproyectos(
     [FromQuery][Description("Valor que indica el numero de la version")] int? numeroVersion,
     [FromQuery][Description("Valor que indica el ejercicio de la actividad")] int? ejercicio,
     [FromQuery][Description("Valor que indica la descripcion estado del anteproyecto")] string? descripcionEstado,
     [FromQuery][Description("Valor que identifica el objeto a buscar en el filtro generico")] string? TerminoDeBusqueda,
     [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
     [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
     [FromQuery][Description("Valor que indica el valor de la fecha de emision")] DateTime? fecha)
    {
        var request = new VersionesAnteproyectoRequest
        {
            NumeroVersion = numeroVersion,
            Ejercicio = ejercicio,
            DescripcionEstado = descripcionEstado,
            TerminoDeBusqueda = TerminoDeBusqueda,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            Fecha = fecha
        };

        var validationResult = new VersionesAnteproyectoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoVersionesanteproyectos(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<VersionesAnteproyectoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<VersionesAnteproyectoDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ObtenerDiasFechaCierre")]
    [Endpoint("ObtenerDiasFechaCierre")]
    [SwaggerOperation(
      Summary = "Permite obtener los dias restantes antes de la fecha de cierre de anteproyecto",
      Description = "Permitir obtener los dias restantes antes de la fecha de cierre de anteproyecto")]
    public async Task<IActionResult> ObtenerDiasFechaCierre()
    {

        var resultado = await _service.ObtenerDiasRestantesCierre();

        if (resultado == null)
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



    [HttpGet("ListarPlanificacionFinanciera")]
    [Endpoint("ListarPlanificacionFinanciera")]
    [SwaggerOperation(
      Summary = "Permite listar la planificacion financiera",
      Description = "Permitir listar la planificacion financiera")]
    public async Task<IActionResult> ListarPlanificacionFinanciera(
    [FromQuery][Description("Valor que indica el ejercicio de la actividad")] int? ejercicio,
    [FromQuery][Description("Valor que indica el numero del objeto")] int? numeroObjetoGasto,
    [FromQuery][Description("Valor que indica el nro de organismo financiador")] int? numeroOrgFinanciador,
    [FromQuery][Description("Valor que indica el nro de fuente financiador")] int? numeroFuenteFinanciador,
    [FromQuery][Description("Valor que indica el nro del departamento")] int? numeroDpto,
    [FromQuery][Description("Descripcion de la fundamentacion")] string? fundamentacion,
    [FromQuery][Description("Valor que identifica el monto del presupuesto aprobado")] string? presupuestoAprobado,
    [FromQuery][Description("Valor que identifica el monto de diferencia")] string? diferencia,
    [FromQuery][Description("Valor que identifica el monto total")] string? totalMensual,
   
    [FromQuery][Description("Valor que identifica el objeto a buscar en el filtro generico")] string? TerminoDeBusqueda,
     [FromQuery][Description("Valor que indica el valor del codigo de la version")] int codigoVersion,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new PlanificacionFinancieraRequest
        {
            Ejercicio = ejercicio,
            NumeroObjetoGasto = numeroObjetoGasto,
            NumeroOrgFinanciador = numeroOrgFinanciador,
            NumeroFuenteFinanciador = numeroFuenteFinanciador,
            NumeroDpto = numeroDpto,
            Fundamentacion = fundamentacion,
            PresupuestoAprobado = presupuestoAprobado,
            TerminoDeBusqueda = TerminoDeBusqueda,
            CodigoVersion = codigoVersion,
            Pagina = pagina,
            Diferencia = diferencia,
            
            TotalMensual = totalMensual,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new PlanificacionFinancieraValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoPlanificacionFinanciera(request);

        if (resultado == null) 
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<PlanificacionFinancieraDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<PlanificacionFinancieraDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarPlanificacionFinancieraporObjetoGasto")]
    [Endpoint("ListarPlanificacionFinancieraporObjetoGasto")]
    [SwaggerOperation(
    Summary = "Permite listar la planificacion financiera",
    Description = "Permitir listar la planificacion financiera")]
    public async Task<IActionResult> ListarPlanificacionFinancieraporObjetoGasto(
  [FromQuery][Description("Valor que indica el codigo de gasto")] int codigoObjetoGasto,
  [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion,
  [FromQuery][Description("Valor que indica el codigo de la fuente financiador")] int codigoFuenteFinanciador,
  [FromQuery][Description("Valor que indica el codigo organismo financiador")] int codigoOrganismoFinanciador)
    {
        var request = new PlanificacionFinancieraporObjetoGastoRequest
        {
            CodigoObjetoGasto = codigoObjetoGasto,
            CodigoVersion = codigoVersion,
            CodigoFuenteFinanciador = codigoFuenteFinanciador,
            CodigoOrganismoFinanciador = codigoOrganismoFinanciador
        };

        var validationResult = new PlanificacionFinancieraporOBGValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoPlanificacionFinancieraporObjetoGasto(request);

        if (resultado == null )
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<PlanificacionFinancieraporObjetoGastoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<PlanificacionFinancieraporObjetoGastoDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarPlanificacionMensual")]
    [Endpoint("ListarPlanificacionMensual")]
    [SwaggerOperation(
    Summary = "Permite listar la planificacion financiera mensual",
    Description = "Permitir listar la planificacion financiera mensual")]
    public async Task<IActionResult> ListarPlanificacionMensual(
        [FromQuery][Description("Valor que indica el codigo del anteproyecto objeto")] int codigoAnteproyectoObjeto,
        [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion)
    {
        var request = new PlanificacionMensualRequest
        {
            CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
            CodigoVersion = codigoVersion
        };

        var validationResult = new PlanificacioMensualValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoPlanificacionMensual(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<PlanificacionMensualDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<PlanificacionMensualDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ObtenerCodigoAnteproyectoObjeto")]
    [Endpoint("ObtenerCodigoAnteproyectoObjeto")]
    [SwaggerOperation(
        Summary = "Permite obtener el codigo del anteproyecto objeto",
        Description = "Permitir obtener el codigo del anteproyecto objeto")]
    public async Task<IActionResult> ObtenerCodigoAnteproyectoObjeto(
      [FromQuery][Description("Valor que indica el codigo del anteproyecto objeto")] int codigoCentroResponsabilidad,
      [FromQuery][Description("Valor que indica el codigo de la version")] int codigoMateria,
      [FromQuery][Description("Valor que indica el codigo del anteproyecto objeto")] int codigoObjetoGasto,
      [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion,
      [FromQuery][Description("Valor que indica el codigo del anteproyecto objeto")] int codigoFF,
      [FromQuery][Description("Valor que indica el codigo de la version")] int codigoOG)
    {
        var request = new CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest
        {
            CodigoCentroResponsabilidad = codigoCentroResponsabilidad,
            CodigoMateria = codigoMateria,
            CodigoObjetoGasto = codigoObjetoGasto,
            CodigoVersion = codigoVersion,
            CodigoFF = codigoFF,
            CodigoOG = codigoOG
        };

        var validationResult = new CodigoAnteproyectoOBJparaPlanificacionFinancieraValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerCodigoAnteproyectoObjeto(request);

        if (resultado == null)
        {
            throw new KeyNotFoundException();
        }

        return Ok(new ApiResponse<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarMesesparaPlanificacionMensual")]
    [Endpoint("ListarMesesparaPlanificacionMensual")]
    [SwaggerOperation(
        Summary = "Permite listar los meses para la planificacion financiera mensual",
        Description = "Permitir listar los meses para la planificacion financiera mensual")]
    public async Task<IActionResult> ListarMesesparaPlanificacionMensual(
    [FromQuery][Description("Valor que indica el codigo del anteproyecto objeto")] int codigoAnteproyectoObjeto,
    [FromQuery][Description("Valor que indica el codigo de la version")] int codigoVersion)
    {
        var request = new MesesparaPlanificacionFinancieraRequest
        {
            CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
            CodigoVersion = codigoVersion
        };

        var validationResult = new MesesparaPlanificacionFinancieraRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerMesesParalaPlanificacionFinanciera(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<MesesparaPlanificacionFinancieraDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<MesesparaPlanificacionFinancieraDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpPost("InsertarPlanificacionFinanciera")]
    [Endpoint("InsertarPlanificacionFinanciera")]
    [SwaggerOperation(
     Summary = "Permite realizar la insercion de las propiedades que corresponden a la planificacion Financiera",
     Description = "Permitir realizar la insercion de las propiedades que corresponden a la planificacion Financiera")]
    public async Task<IActionResult> InsertarPlanificacionFinanciera(
    [FromBody][Description("Datos que corresponden a las propiedades para la planificacion financiera")]
       GestionarInsertarPlanificacionFinancieraRequest request)
    {
        request.UsuarioInserto = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new GestionarPlanificacionFinancieraRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.GestionarInsertarPlanificacionFinanciera(request);

        if (resultado > 0 && resultado != -1 && resultado != -2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else if (resultado == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "El monto ingresado excede el monto total planificado para el objeto de gasto",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        else if (resultado == -2)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "La cantidad de meses sobrepasa o ya alcanzó la cantidad de eventos permitidos para el presupuesto del objeto de gasto.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ocurrio un error al intentar insertar la planificacion financiera",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

    }


    [HttpPut("EditarPlanificacionFinanciera")]
    [Endpoint("EditarPlanificacionFinanciera")]
    [SwaggerOperation(
     Summary = "Permite realizar la edicion de las propiedades que corresponden a la planificacion Financiera",
     Description = "Permitir realizar la edicion de las propiedades que corresponden a la planificacion Financiera")]
    public async Task<IActionResult> EditarPlanificacionFinanciera(
    [FromBody][Description("Datos que corresponden a las propiedades para la planificacion financiera")]
       GestionarEditartarPlanificacionFinancieraRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new GestionarEditartarPlanificacionFinancieraRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.GestionarEditarPlanificacionFinanciera(request);

        if (resultado > 0 && resultado != -1 && resultado != -2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }
        else if (resultado == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "El monto ingresado excede el monto total planificado para el objeto de gasto",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }      
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Ocurrio un error al intentar editar la planificacion financiera",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

    }


    [HttpGet("ListarPlanificacionMensualporItem")]
    [Endpoint("ListarPlanificacionMensualporItem")]
    [SwaggerOperation(
       Summary = "Permite obtener el codigo del anteproyecto objeto",
       Description = "Permitir obtener el codigo del anteproyecto objeto")]
    public async Task<IActionResult> ListarPlanificacionMensualporItem(
     [FromQuery][Description("Valor que indica el codigo del anteproyecto planificacion Financiera")] PlanificacionMensualporItemRequest request)
    {      
        var validationResult = new PlanificacionMensualporItemRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoPlanificacionMensualporItem(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<PlanificacionMensualDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<PlanificacionMensualDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpDelete("EliminarPlanificacionFinanciera")]
    [Endpoint("EliminarPlanificacionFinanciera")]
    [SwaggerOperation(
     Summary = "Permite eliminar la planificacion financiera",
     Description = "Permitir eliminar la planificacion financiera")]
    public async Task<IActionResult> EliminarPlanificacionFinanciera(
    [FromQuery][Description("Valor que indica el codigo del anteproyecto planificacion Financiera")] EliminarPlanificacionFinancieraRequest request)
    {
        var validationResult = new EliminarPlanificacionFinancieraRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.EliminarPlanificacionFinanciera(request);

        if (resultado == null)
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
