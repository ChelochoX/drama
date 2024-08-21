using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.Request;
using Domain.Entities.Request.ConfiguracionPresupuestaria;
using Domain.Entities.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;
using WebApi.ValidationHandlers;
using ValidationException = FluentValidation.ValidationException;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConfiguracionPresupuestariaController : ControllerBase
{

    private readonly IConfiguracionPresupuestariaService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public ConfiguracionPresupuestariaController(IConfiguracionPresupuestariaService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }



    [HttpGet("ListarConfiguracionPresupuestariaporObjetosGastos")]
    [Endpoint("ListarConfiguracionPresupuestariaporObjetosGastos")]
    [SwaggerOperation(
    Summary = "Permite visualizar la configuracion presupuestaria de los objetos de gastos de las solicitudes",
    Description = "Permitir visualizar la configuracion de la estructura presupuestaria de los objetos de gastos de las solicitudes")]
    public async Task<IActionResult> ListarConfiguracionPresupuestariaObjetoGastos(
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] string? poi,
    [FromQuery][Description("Valor que indica el centro de responsabilidad")] string? centroResponsabilidad,
    [FromQuery][Description("Valor que indica la descripcion del objeto de gasto")] string? objetoGasto,
    [FromQuery][Description("Valor que indica el centro de responsabilidad")] string? descripcionMateria,
    [FromQuery][Description("Valor que indica la descripcion del objeto de gasto")] string? departamento,
    [FromQuery][Description("Valor que identifica el objeto a buscar en el filtro generico")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que identifica la cantidad a buscar en el filtro generico")] string? cantidad,
    [FromQuery][Description("Valor que identifica el monto total a buscar en el filtro generico")] string? montoTotal,
    [FromQuery][Description("Valor que identifica el periodo de la configuracion presupuestaria")] int parametroPoi,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new ConfiguracionPresupuestariaPorObjetosGastosRequest
        {
            Poi = poi,
            CentroResponsabilidad = centroResponsabilidad,
            ObjetoGasto = objetoGasto,
            TerminoDeBusqueda = terminoDeBusqueda,
            ParametroPoi = parametroPoi,
            Pagina = pagina,
            DescripcionMateria = descripcionMateria,
            Departamento = departamento,
            CantidadRegistros = cantidadRegistros,
            Cantidad = cantidad,
            MontoTotal = montoTotal
        };

        var validationResult = new ConfiguracionPresupuestariaValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerConfiguracionPresupuestariaObjetosGastos(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<ConfiguracionPresupuestariaPorObjetosGastosDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }



    [HttpPost("InsertarCabeceraConfiguracionPresupuestaria")]
    [Endpoint("InsertarCabeceraConfiguracionPresupuestaria")]
    [SwaggerOperation(
    Summary = "Permite insertar la Configuracion presupuestaria",
    Description = "Permitir insertar la configuracion presupuestaria")]
    public async Task<IActionResult> InsertarCabeceraparaConfiguracionPresupuestaria(
    [FromBody][Description("Datos que corresponden a la solicitud de insertar para la configuracion presupuestaria")]
    DatosparaCabeceraConfiguracionPresupuestaria2Request request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var solicitudInsertada = await _service.InsertarCabeceraparaConfiguracionPresupuestaria(request);

        if (solicitudInsertada != null)
        {
            return Ok(new ApiResponse<Datos<DatosdeConfiguracionPresupuestariaDTO>>
            {
                Success = true,
                Data = solicitudInsertada,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo crear la configuracion presupuestaria",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpGet("ListarDatosparaConfiguracionPresupuestaria")]
    [Endpoint("ListarDatosparaConfiguracionPresupuestaria")]
    [SwaggerOperation(
    Summary = "Permite visualizar la configuracion presupuestaria de los objetos de gastos de las solicitudes",
    Description = "Permitir visualizar la configuracion de la estructura presupuestaria de los objetos de gastos de las solicitudes")]
    public async Task<IActionResult> ObtenerDatosparalaConfiguracionPresupuestaria(      
    [FromQuery][Description("Propiedades necesarios para obtener los datos para la configuracion presupuestaria")] DatosparaConfiguracionPresupuestariaRequest request)
    {       
        var validationResult = new DatosparaConfiguracionPresupuestariaValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerDatosparaInsertarConfiguracionPresupuestaria(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<DatosparaConfiguracionPresupuestariaDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<DatosparaConfiguracionPresupuestariaDTO>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarDatosOrganismoFinanciadores")]
    [Endpoint("ListarDatosOrganismoFinanciadores")]
    [SwaggerOperation(
    Summary = "Permite Obtener los datos de la tabla de organismos financiadores",
    Description = "Permitir obtener datos de los organismos financiadores")]
    public async Task<IActionResult> ObtenerDatosdeOrganismosFinanciadores(
    [FromQuery][Description("Valor que indica el valor del codigo organismo financiador")] int? codigoOF,
    [FromQuery][Description("Valor que indica el valor del numero organismo financiador")] string? numeroOF,
    [FromQuery][Description("Valor que indica la descripcion del organismo financiadoor")] string? descripcionOF,
    [FromQuery][Description("Valor que indica el termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el nro de pagina")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new OrganismoFinanciadorRequest
        {
            CodigoOrganismoFinanciador = codigoOF,
            NumeroOrganismoFinanciador = numeroOF,
            DescripcionOrganismoFinanciador = descripcionOF,
            TerminoDeBusqueda = terminoDeBusqueda,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new OrganismoFinanciadorValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerDatosOrganismoFinanciador(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<OrganismoFinanciadorDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<OrganismoFinanciadorDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarDatosFuenteFinanciacion")]
    [Endpoint("ListarDatosFuenteFinanciacion")]
    [SwaggerOperation(
    Summary = "Permite Obtener los datos de la tabla de Fuentes de financiacion",
    Description = "Permitir obtener datos de las fuentes de financiacion")]
    public async Task<IActionResult> ObtenerDatosdeFuentesdeFinanciacion(
    [FromQuery][Description("Valor que indica el valor del codigo de la f de gasto")] int? codigoFF,
    [FromQuery][Description("Valor que indica el valor del codigo del objeto de gasto")] string? numeroFF,
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] string? descripcionFF,
    [FromQuery][Description("Valor que indica el termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el nro de pagina")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new FuenteFinanciamientoRequest
        {
            CodigoFuenteFinanciamiento = codigoFF,
            NumeroFuenteFinanciamiento = numeroFF,
            DescripcionFuenteFinanciamiento = descripcionFF,
            TerminoDeBusqueda = terminoDeBusqueda,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new FuenteFinanciamientorValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerDatosFuenteFinanciamiento(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<FuenteFinanciamientoDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<FuenteFinanciamientoDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpDelete("EliminarObjetoGastoVersionAnteproyectoObjetos")]
    [Endpoint("EliminarObjetoGastoVersionAnteproyectoObjetos")]
    [SwaggerOperation(
    Summary = "Permite eliminar registro de la tabla de versiones anteproyecto objetos con los bienes que corresponden a este Objeto de Gasto",
    Description = "Permitir eliminar registro de la tabla de versiones anteproyecto de objetos on los bienes que corresponden a este Objeto de Gasto")]
    public async Task<IActionResult> EliminarObjetodeTablaVersionAnteproyectoObjetos(
    [FromQuery][Description("Valor que indica el valor del codigo de la f de gasto")] int parametro_CodigoAnteProyectoObjeto,
    [FromQuery][Description("Valor que indica el valor del codigo del objeto de gasto")] int parametro_CodigoVersion,
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] int parametro_Config_Presupuestaria,
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] int codigoCentroResponsabilidad,
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] int codigoMateria)
    {
        var request = new VerionesAnteproyectoObjetosEliminarRequest
        {
            Parametro_CodigoAnteProyectoObjeto = parametro_CodigoAnteProyectoObjeto,
            Parametro_CodigoVersion = parametro_CodigoVersion,
            Parametro_Config_Presupuestaria = parametro_Config_Presupuestaria,
            CodigoCentroResponsabilidad = codigoCentroResponsabilidad,
            CodigoMateria = codigoMateria
        };

        var validationResult = new EliminarVersionesAnteproyectoObjetoValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.EliminarObjetodeAnteproyectoObjetoyBienes(request);

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


    [HttpPut("ModificarFuenteFinanciamientoyBienes")]
    [Endpoint("ModificarFuenteFinanciamientoyBienes")]
    [SwaggerOperation(
      Summary = "Permite actualizar fuente de financiamiento de la version anteproyecto de los objetos de gastos",
      Description = "Permitir actualizar las fuentes de financiamiento de la tabla de version anteproyecto de objetos de gastos")]
    public async Task<IActionResult> ModificarFuenteFinanciamientoyBienes(
    [FromBody][Description("Datos que corresponden a la fuente de financiamiento de objeto de gastos y bienes")]
    ModificarFuenteFinanciamientoyBienesRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new FuenteFinanciamientoyBienesValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var filasActualizadas = await _service.ModificarFuenteFinanciamientoyBienes(request);

        if (filasActualizadas > 0)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }

        throw new KeyNotFoundException();
    }


    [HttpGet("ListarBienesparaFuenteFinanciamiento")]
    [Endpoint("ListarBienesparaFuenteFinanciamiento")]
    [SwaggerOperation(
    Summary = "Permite visualizar los bienes que pertenecen a una fuente de financiamiento",
    Description = "Permitir visualizar los bienes de una fuente de financiamiento")]
    public async Task<IActionResult> ListarBienesparaFuenteFinanciamiento(
    [FromQuery][Description("Valor que indica el valor del numero de Bien")] int? numeroBien,
    [FromQuery][Description("Valor que indica la descripcion del bien")] string? descripcionBien,
    [FromQuery][Description("Valor que indica la cantidad del bien")] string? cantidad,
    [FromQuery][Description("Valor que indica la costo unitario del bien")] string? costoUnitario,
    [FromQuery][Description("Valor que indica la monto total del bien")] string? montoTotal,
    [FromQuery][Description("Valor que indica la expresion de busqueda")] string? terminodeBusqueda,
    [FromQuery][Description("Valor que indica el valor del codigo de la version anteproyecto")] int codigoversion,
    [FromQuery][Description("Valor que indica el valor del codigo de version anteproyecto objeto")] int codigoVersionOBG,
    [FromQuery][Description("Valor que indica el valor del codigo del objeto de gasto")] int CodigoObjetoGasto,
    [FromQuery][Description("Valor que indica el valor del codigo del centro de responsabilidad")] int codigoCentroResponsabilidad,
    [FromQuery][Description("Valor que indica el valor del codigo del codigo materia")] int codigoMateria,
    [FromQuery][Description("Valor que indica el periodo de la solicitud")] int Ejercicio,
    [FromQuery][Description("Valor que indica el nro de pagina")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new BienesparaConfiguracionPresupuestariaRequest
        {
            NumeroBien = numeroBien,
            DescripcionBien = descripcionBien,
            Cantidad = cantidad,
            CostoUnitario = costoUnitario,
            MontoTotal = montoTotal,
            TerminoDeBusqueda = terminodeBusqueda,
            CodigoVersion = codigoversion,
            CodigoVersionOBG = codigoVersionOBG,
            CodigoObjetoGasto = CodigoObjetoGasto,
            CodigoMateria = codigoMateria,
            CodigoCentroResponsabilidad = codigoCentroResponsabilidad,
            Ejercicio = Ejercicio,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new BienesparaConfiguracionPresupuestariaValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }    

        var resultado = await _service.ObtenerBienesparaFuenteFinanciamiento(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<BienesparaConfiguracionPresupuestariaDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpPost("InsertarFuentesdeFinanciamientoConsusBienes")]
    [Endpoint("InsertarFuentesdeFinanciamientoConsusBienes")]
    [SwaggerOperation(
    Summary = "Permite realizar la configuracion de las fuentes de finaciamiento con sus respectivos bienes para la Configuracion presupuestaria",
    Description = "Permitir realizar la configuracion de las fuentes de finaciamiento con sus respectivos bienes para la Configuracion presupuestaria")]
    public async Task<IActionResult> InsertarFuentesdeFinanciamientoConsusBienes(
    [FromBody][Description("Datos que corresponden a las fuentes de financiamiento con los datos de bienes que corresponden " +
    "a esa fuente de financiamiento para la configuracion presupuestaria")]
    DatosparaAnteproyectoObjetosyBienesRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var financiamientoInsertado = await _service.GestionarFuentesdeFinanciamientoConsusBienes(request);

        if (financiamientoInsertado > 0)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = financiamientoInsertado,
                StatusCode = (int)HttpStatusCode.Created
            });
        }
        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo crear el Financiamiento con sus bienes para la configuracion presupuestaria",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpGet("ListarFinanciamientoAnteproyectoObjetos")]
    [Endpoint("ListarFinanciamientoAnteproyectoObjetos")]
    [SwaggerOperation(
        Summary = "Permite Obtener los datos de la tabla de Versiones Anteproyecto Objetos de Gastos",
        Description = "Permitir obtener datos de tabla de Versiones Anteproyecto Objetos de Gastos")]
    public async Task<IActionResult> ObtenerDatosdeFinanciamientodeVersionesAnteproyectoObjetos(
    [FromQuery][Description("Valor que indica el valor del codigo de la version")] int codigoVersion,
    [FromQuery][Description("Valor que indica el valor del codigo de la configuracion presupuestaria")] int codigoConfiguracionPresupuestaria)
    {
        var request = new FinanciamientoVersionAnteproyectoObjetosRequest
        {
            CodigoVersionAnteproyectos = codigoVersion,
            CodigoConfiguracionPresupuestaria = codigoConfiguracionPresupuestaria
        };

        var resultado = await _service.ObtenerDatosVersionesAnteproyectosObjetos(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<FinanciamientoVersionAnteproyectoObjetosResponseDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<FinanciamientoVersionAnteproyectoObjetosResponseDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListarDatosdelaSolicitudporObjetoGasto")]
    [Endpoint("ListarDatosdelaSolicitudporObjetoGasto")]
    [SwaggerOperation(
    Summary = "Permite Obtener los datos de la Solicitud por los Objetos de Gastos",
    Description = "Permitir obtener datos de la Solicitud por los Objetos de Gastos")]
    public async Task<IActionResult> ObtenerDatosdelaSolicitudporObjetodeGasto(
    [FromQuery][Description("Valor para obtener los dato de las solicitudes por objeto de gasto")] DatosdeSolicitudporObjetoGastoRequest request)
    {  

        var validationResult = new DatosparaSolicitudporOBGValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerDatosdelaSolicitud(request);

        if (resultado == null)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new List<DatosdeSolicitudporObjetoGastoResponseDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }

        return Ok(new ApiResponse<IEnumerable<DatosdeSolicitudporObjetoGastoResponseDTO>>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

    [HttpGet("VerificarExisteVersiondeAnteproyectoCerrado")]
    [SwaggerOperation(
        Summary = "Verifica si existe una versión abierta del Anteproyecto.",
        Description = "Retorna 1 o 0 dependiendo si existe o no una versión abierta.")]
    public async Task<IActionResult> VerificarExisteVersiondeAnteproyectoCerrado(
        [FromQuery][Description("Valor que indica el valor del ejercicio")] int ejercicio)
    {

        var resultado = await _service.VerificarExisteVersiondeAnteproyectoCerrado(ejercicio);

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = resultado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

}
