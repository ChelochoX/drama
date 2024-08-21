using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.Anteproyecto;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.VerificacionSolicitudes;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Net;
using WebApi.ValidationHandlers;
using ParametrosRequestValidator = Domain.Entities.GenerarSolicitudporCircunscripcion.ParametrosRequestValidator;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SolicitudesporCircunscripcionController : ControllerBase
{
    private readonly IGenerarSolicitudesporCircunscripcionService _service;
    private readonly IDatosUsuarioService _usuarioService;

    public SolicitudesporCircunscripcionController(IGenerarSolicitudesporCircunscripcionService service, IDatosUsuarioService usuarioService)
    {
        _service = service;
        _usuarioService = usuarioService;
    }


    #region DESARROLLO DE LA INCIDENCIA CSJ-138

    [HttpGet("ListadoBienesPorCodigoSolicitud")]
    [Endpoint("ListadoBienesPorCodigoSolicitud")]
    [SwaggerOperation(
            Summary = "Nos permite obtener informacion de los Bienes de la la tabla Solicitud Objetos Bienes Detalle",
            Description = "Obtiene información de los bienes que pertenecen a la tabla Solicitud Objetos Bienes Detalle.")]
    public async Task<ActionResult<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCodigo(
        [FromQuery][Description("Código único de la solicitud")] int codigoSolicitud, int pagina, int cantidadRegistros)
    {
        if (codigoSolicitud == 0)
        {
            throw new ReglasdeNegocioException("El código de solicitud no puede estar vacío o tener valor igual a cero");
        }

        if (codigoSolicitud == 0)
        {
            throw new ReglasdeNegocioException("El código de usuario no puede estar vacío o tener valor igual a cero");
        }
        var listado = await _service.ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(codigoSolicitud, pagina, cantidadRegistros);


        if (listado == null || !listado.Any())
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de solicitud para el código proporcionado.");
        }



        return Ok(new ApiResponse<IEnumerable<SolicitudObjetoBienesDetalleDTO>>
        {
            Success = true,
            Data = (IEnumerable<SolicitudObjetoBienesDetalleDTO>)listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("ListadoBienesPorCriterioBusqueda")]
    [Endpoint("ListadoBienesPorCriterioBusqueda")]
    [SwaggerOperation(
      Summary = "Nos permite obtener informacion de los Bienes de la tabla Solicitud Objetos Bienes Detalle",
      Description = "Obtiene información de los bienes que pertenecen a la tabla Solicitud Objetos Bienes Detalle.")]
    public async Task<ActionResult<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusqueda(
   [FromQuery][Description("Código único de la solicitud")] int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string? descripcion, int pagina, int cantidadRegistros)
    {



        int cantidadTotalRegistros;
        var listado = await _service.ListadoBienesPorCriterioBusqueda(ejercicio - 1, codigoSolicitud, codigoSolicitudObjeto, codigo, descripcion, pagina, cantidadRegistros);

        // Parámetros del informe (si es necesario)
        var parameters = new { param1 = "valor1", param2 = "valor2" };


        if (listado == null || !listado.Any())
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de bienes para los datos proporcionados.");
        }
        else
        {
            SolicitudObjetoBienesDetalleDTO totalRegistros = listado.First();


            cantidadTotalRegistros = await _service.CantidadRegistrosBienes(0, ejercicio, codigoSolicitud, codigoSolicitudObjeto, codigo, descripcion);
        }
        var response = new ApiResponse<dynamic>
        {
            Success = true,
            Data = new
            {
                items = listado,
                totalRegistros = cantidadTotalRegistros
            },
            StatusCode = (int)HttpStatusCode.OK
        };

        return Ok(response);
    }


    [HttpGet("FiltroBienes")]
    [Endpoint("FiltroBienes")]
    [SwaggerOperation(
    Summary = "Permite Obtener aquellos Bienes por ejercicio",
    Description = "Obtener Registros de aquellos bienes por ejercicio")]
    public async Task<ActionResult<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusquedaPorFiltros(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica ej ejercicio")] int ejercicio,
    [FromQuery][Description("Valor que indica el valor del nro solicitud")] int? CodigoSolicitud,
    [FromQuery][Description("Valor que indica el valor del nro Objeto")] int? CodigoSolicitudObjeto,
    [FromQuery][Description("Valor que indica el valor de la fecha de emision")] DateTime FechaInserto,
    [FromQuery][Description("Valor que indica el valor del solicitante")] int? UsuarioInserto,
    [FromQuery][Description("Valor que indica el descripcion del bien")] string? DescripcionBienesNacional,
    [FromQuery][Description("Valor que indica el valor de fundamentacion")] string? Fundamentacion,
    [FromQuery][Description("Valor que indica el valor de la fecha de emision")] string? Cantidad,
    [FromQuery][Description("Valor que indica el valor del codigo de bien")] string? NumeroCatalogo,
    [FromQuery][Description("Valor que indica el valor del codigo de bien")] string? CodigoBienDetalle,
    [FromQuery][Description("Valor que indica el valor del solicitante")] decimal? Monto,
     [FromQuery][Description("Valor que indica el valor del solicitante")] string? CostoUnitario,
    [FromQuery][Description("Valor que indica el descripcion del bien")] string? MontoTotal)

    {
        int varEjercicio = 0;
        if (ejercicio != 0)
        {
            varEjercicio = ejercicio - 1;
        }
        var request = new SolicitudObjetoBienesDetalleRequest
        {


            CodigoSolicitud = CodigoSolicitud,
            CodigoSolicitudObjeto = CodigoSolicitudObjeto,
            Ejercicio = varEjercicio,
            UsuarioInserto = UsuarioInserto,
            TerminoDeBusqueda = terminoDeBusqueda,
            FechaInserto = FechaInserto,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            DescripcionBienesNacional = DescripcionBienesNacional,
            MontoTotal = MontoTotal,
            Monto = Monto,
            Fundamentacion = Fundamentacion,
            NumeroCatalogo = NumeroCatalogo,
            CodigoBienDetalle = CodigoBienDetalle,
            Cantidad = Cantidad,
            CostoUnitario = CostoUnitario

        };



        var resultado = await _service.ListadoBienesPorCriterioBusquedaPorFiltros(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }


    }


    [HttpGet("ListadoBienesPorCodigoDescripcion")]
    [Endpoint("ListadoBienesPorCodigoDescripcion")]
    [SwaggerOperation(
    Summary = "Permite Obtener aquellos Bienes por ejercicio",
    Description = "Obtener Registros de aquellos bienes por ejercicio")]
    public async Task<ActionResult<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCodigoDescripcion(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica ej ejercicio")] int ejercicio,
    [FromQuery][Description("Valor que indica el valor del nro solicitud")] int? CodigoSolicitud,
    [FromQuery][Description("Valor que indica el valor del nro Objeto")] int? CodigoSolicitudObjeto,
    [FromQuery][Description("Valor que indica el valor del nro Objeto")] int? CodigoObjetoGasto,
    [FromQuery][Description("Valor que indica el valor de la fecha de emision")] DateTime FechaInserto,
    [FromQuery][Description("Valor que indica el valor del solicitante")] int? UsuarioInserto,
    [FromQuery][Description("Valor que indica el descripcion del bien")] string? DescripcionBienesNacional,
    [FromQuery][Description("Valor que indica el valor de fundamentacion")] string? Fundamentacion,
    [FromQuery][Description("Valor que indica el valor de la fecha de emision")] string? Cantidad,
    [FromQuery][Description("Valor que indica el valor del codigo de bien")] string? NumeroCatalogo,
    [FromQuery][Description("Valor que indica el valor del codigo de bien")] string? CodigoBienDetalle,
    [FromQuery][Description("Valor que indica el valor del solicitante")] decimal? Monto,
    [FromQuery][Description("Valor que indica el valor del solicitante")] string? CostoUnitario,
    [FromQuery][Description("Valor que indica el descripcion del bien")] string? MontoTotal)

    {
        int varEjercicio = 0;
        if (ejercicio != 0)
        {
            varEjercicio = ejercicio - 1;
        }
        var request = new SolicitudObjetoBienesDetalleRequest
        {


            CodigoSolicitud = CodigoSolicitud,
            CodigoSolicitudObjeto = CodigoSolicitudObjeto,
            CodigoObjetoGasto = CodigoObjetoGasto,
            Ejercicio = varEjercicio,
            UsuarioInserto = UsuarioInserto,
            TerminoDeBusqueda = terminoDeBusqueda,
            FechaInserto = FechaInserto,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            DescripcionBienesNacional = DescripcionBienesNacional,
            MontoTotal = MontoTotal,
            Monto = Monto,
            Fundamentacion = Fundamentacion,
            NumeroCatalogo = NumeroCatalogo,
            CodigoBienDetalle = CodigoBienDetalle,
            Cantidad = Cantidad,
            ValorUnitario = CostoUnitario

        };



        var resultado = await _service.ListadoBienesPorCodigoDescripcion(request);
        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
        else
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }


    }


    [HttpPost("CrearBien")]
    [Endpoint("CrearBien")]
    [SwaggerOperation(
        Summary = "Nos permite Crear bienes en la BBDD del Poder Judicial, Solo Los Autorizados pueden gestionarlos",
        Description = "Creamos bienes que pertenecen a la tabla Solicitud Objetos Bienes Detalle.")]
    public async Task<ActionResult> CrearBien([FromBody] SolicitudObjetoBienesDetalleDTO bienDetalle)
    {
        bienDetalle.UsuarioInserto = int.Parse(_usuarioService.DatosUsuario.NumeroDocumento);

        bool eliminar = false;
        var filasActualizadas = await _service.CrearSolicitudObjetosBienesDetalle(bienDetalle);
        string bien = await _service.Bienprocesado(bienDetalle.NumeroCatalogo, bienDetalle.CodigoSolicitud, bienDetalle.CodigoSolicitudObjeto, eliminar);

        if (filasActualizadas == 0)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "Bien: " + bien + " no existe.",
            });
        }

        else if (filasActualizadas == -1)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = " El bien: " + bien + " seleccionado ya existe.",
            });
        }

        else if (filasActualizadas == -2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent,
                Message = "Ocurrio un error al insertar los datos en la tabla Solicitud Objetos Bienes Detalle",
            });
        }

        else
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
                Message = " Registro de bien: " + bien + " creado correctamente.",
            });
        }

    }
    [HttpDelete("EliminarBien")]
    [Endpoint("EliminarBien")]
    [SwaggerOperation(
        Summary = "Nos permite Eliminar bienes en la tabla Solicitud Objetos Bienes Detalle",
        Description = "Eliminar bienes que pertenecen a la tabla Solicitud Objetos Bienes Detalle.")]
    public async Task<ActionResult> EliminarBien([FromQuery][Description("Código único del bien")] int CodigoBienDetalle, int CodigoSolicitud, int CodigoSolicitudObjeto, int ejercicio, string descripcionBienesNacional = "")
    {
        // Validar que Parametros no sean nulo o cero
        if (CodigoSolicitud <= 0 || CodigoSolicitudObjeto <= 0 || CodigoBienDetalle == null || CodigoSolicitud == null || CodigoSolicitudObjeto == null)
        {
            throw new ReglasdeNegocioException("El Parametros de eliminación de bien no es válido");
        }
        bool eliminar = true;



        string bien = await _service.Bienprocesado(CodigoBienDetalle.ToString(), CodigoSolicitud, CodigoSolicitudObjeto, eliminar);

        var filasActualizadas = await _service.EliminarSolicitudObjetosBienesDetalle(CodigoBienDetalle, CodigoSolicitud, CodigoSolicitudObjeto, descripcionBienesNacional, ejercicio);


        if (filasActualizadas == -1)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent,
                Message = " El bien: " + bien + " no existe.",
            });
        }

        else if (filasActualizadas == -2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent,
                Message = "Ocurrio un error al insertar los datos en la tabla Solicitud Objetos Bienes Detalle",
            });
        }

        else
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
                Message = " Se eliminó correctamente el bien: " + bien + ".",
            });
        }

    }


    [HttpPut("ActualizarBienDetalle")]
    [Endpoint("ActualizarBienDetalle")]
    [SwaggerOperation(
        Summary = "Actualiza el detalle de un bien en una solicitud",
        Description = "Actualiza la información del detalle de un bien en una solicitud utilizando un DTO de detalle de bienes y un código de usuario.")]
    public async Task<ActionResult> ActualizarBienDetalle([FromBody] SolicitudObjetoBienesDetalleDTO bienDetalle)
    {
        bool eliminar = false;
        if (bienDetalle.NumeroCatalogo == "")
        {
            throw new ReglasdeNegocioException("El código de bien no puede estar vacío o tener valor igual a cero");
        }

        var filasActualizadas = await _service.ActualizarSolicitudObjetoBienesDetalle(bienDetalle);
        string bien = await _service.Bienprocesado(bienDetalle.NumeroCatalogo, bienDetalle.CodigoSolicitud, bienDetalle.CodigoSolicitudObjeto, eliminar);


        if (filasActualizadas == -1)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent,
                Message = " El bien: " + bien + " seleccionado ya existe.",
            });
        }

        else if (filasActualizadas == -2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Ocurrio un error al actualizar los datos en la tabla Solicitud Objetos Bienes Detalle",
            });
        }

        else
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.OK,
                Message = " Se actualizó correctamente El bien: " + bien + ".",
            });
        }

    }


    [HttpGet("TotalesSolicitud")]
    [Endpoint("TotalesSolicitud")]
    [SwaggerOperation(
     Summary = "Nos permite obtener los totales de bienes y montos",
     Description = "Obtiene información de los totales de bienes y montos.")]
    public async Task<ActionResult<CantidadTotalGenericaDTO>> TotalesCatidadBienesSolicitud(
    [FromQuery][Description("Código único de la solicitud")] int codigoSolicitud, int codigoSolicitudObjeto)
    {

        var listado = await _service.TotalesCatidadBienesSolicitud(codigoSolicitud, codigoSolicitudObjeto);

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de bienes para los datos proporcionados.");
        }

        return Ok(new ApiResponse<CantidadTotalGenericaDTO>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }


    [HttpGet("EjercicioSolicitud")]
    [Endpoint("EjercicioSolicitud")]
    [SwaggerOperation(
   Summary = "Nos permite obtener el ejercicio activo",
   Description = "Obtiene información de el ejercicio activo.")]
    public async Task<ActionResult<int>> EjercicioSolicitud()
    {

        var listado = await _service.EjercicioSolicitud();

        if (listado == null)
        {

            throw new ReglasdeNegocioException("No se encontraron detalles de bienes para los datos proporcionados.");
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = listado,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

    #endregion


    [HttpGet("ListarSolicitudesconEstadosAbiertos")]
    [Endpoint("ListarSolicitudesconEstadosAbiertos")]
    [SwaggerOperation(
        Summary = "Permite Obtener aquellas solicitudes de Bienes por cada Circunscripcion con estados abiertos",
        Description = "Obtener Registros de aquellas solcitudes con estados abiertos por cada circunscipcion")]
    public async Task<IActionResult> ListarSolicitudesEstadosAbiertos(
    [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que indica el valor del nro solicitud")] int? numeroSolicitud,
    [FromQuery][Description("Valor que indica el valor del poi")] string? poi,
    [FromQuery][Description("Valor que indica el valor de la circunscripcion")] string? circunscripcion,
    [FromQuery][Description("Valor que indica el valor del centro de responsabilidad")] string? centroResponsabilidad,
    [FromQuery][Description("Valor que indica el valor de la materia juridica")] string? materiaJuridica,
    [FromQuery][Description("Valor que indica el valor de la fecha de emision")] DateTime? fechaEmision,
    [FromQuery][Description("Valor que indica el valor del solicitante")] string? usuarioSolicitante,
    [FromQuery][Description("Valor que indica el nombre Completo del solicitante")] string? usuarioNombreCompleto,
    [FromQuery][Description("Valor que indica el codigo de la solicitud")] int codigoSolicitud,
    [FromQuery][Description("Valor que indica el codigo de la solicitud")] string? descripcionEstado)
    {
        var request = new SolicitudBienesCircunscripcionRequest
        {
            NumeroSolicitud = numeroSolicitud,
            POI = poi,
            Circunscripcion = circunscripcion,
            CentroResponsabilidad = centroResponsabilidad,
            MateriaJuridica = materiaJuridica,
            FechaEmision = fechaEmision,
            UsuarioSolicitante = usuarioSolicitante,
            UsuarioNombreCompleto = usuarioNombreCompleto,
            TerminoDeBusqueda = terminoDeBusqueda,
            CodigoSolicitud = codigoSolicitud,
            CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento,
            descripcionEstado = descripcionEstado,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new ParametrosRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarSolicitudesconEstadosAbiertos(request);


        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>>
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
                Data = new List<SolicitudCircunscripcionesEstadoAbierto>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarCentroResponsabilidadporUsuario")]
    [Endpoint("ListarCentroResponsabilidadporUsuario")]
    [SwaggerOperation(
    Summary = "Permite obtener el centro de responsabilidad que corresponde al usuario por circunscripcion",
    Description = "Permitir obtener el centro de responsabilidad que corresponde al usuario por circunscripcion")]
    public async Task<IActionResult> ListarCentroResponsabilidadporUsuario()
    {
        var request = new UsuarioCircunscipcionRequest2
        {
            CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento
        };

        var validationResult = new UsuarioCircunscipcionRequest2Validator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerCentroResponsabilidadporUsuario(request);

        if (resultado != null)
        {
            return Ok(new ApiResponse<UsuarioCircunscripcion>
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
                Data = new List<UsuarioCircunscripcion>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarMateriasporUsuarioyCentroResponsabilidad")]
    [Endpoint("ListarMateriasporUsuarioyCentroResponsabilidad")]
    [SwaggerOperation(
    Summary = "Permite obtener la materia que corresponde al usuario por centro de responsabilidad",
    Description = "Permitir obtener la materia que corresponde al usuario por centro de responsabilidad")]
    public async Task<IActionResult> ListarMateriasporUsuarioyCentroResponsabilidad(
          [FromQuery][Description("Valor que indica el codigo centro responsabilidad del usuario")] int codigoCentroResponsabilidad)
    {

        var request = new UsuarioCircunscipcionRequest2
        {
            CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento,
            CodigoCentroResponsabilidad = codigoCentroResponsabilidad
        };

        var validationResult = new UsuarioCircunscipcionRequest2Validator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerMateriaporUsuarioyCentroResponsabilidad(request);

        if (resultado != null)
        {
            return Ok(new ApiResponse<UsuarioCircunscripcion>
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
                Data = new List<UsuarioCircunscripcion>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarUsuarioCentroResponsabilidadyMateria")]
    [Endpoint("ListarUsuarioCentroResponsabilidadyMateria")]
    [SwaggerOperation(
    Summary = "Permite obtener el centro de responsabilidad y materia que corresponde al usuario por circunscripcion",
    Description = "Permitir obtener el centro de responsabilidad y materia que corresponde al usuario por circunscripcion")]
    public async Task<IActionResult> ObtenerCentroResponsabilidadyMateriaporUsuario()
    {
        var cedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        if (cedulaUsuario == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = true,
                Message = "Numero de Cedula no puede estar vacio",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var resultado = await _service.ObtenerCentrodeResponsabilidadyMateriaJuridica(cedulaUsuario);

        if (resultado != null)
        {
            return Ok(new ApiResponse<CentroResponsabilidadyMateria>
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
                Data = new List<CentroResponsabilidadyMateria>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarObjetosdeGastos")]
    [Endpoint("ListarObjetosdeGastos")]
    [SwaggerOperation(
     Summary = "Nos permite listar Objeto de gasto",
     Description = "Obtiene una lista de objetos de gasto")]
    public async Task<IActionResult> ObtenerObjetosGasto(
    [FromQuery][Description("Valor que indica el numero del objeto de gasto")] string? NumeroObjetoGasto,
    [FromQuery][Description("Valor que indica la descripcion del objeto de gasto")] string? DescripcionObjetoGasto,
    [FromQuery][Description("Valor que indica la expresion de busqueda para el filtro generico")] string? TerminoDeBusqueda,
    [FromQuery][Description("Valor que indica el valor del ejercicio actual")] int ejercicio,
    [FromQuery][Description("Valor que indica el numero de pagina a mostrar")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros)
    {
        var request = new ObjetosdeGastosRequest
        {
            NumeroObjetoGasto = NumeroObjetoGasto,
            DescripcionObjetoGasto = DescripcionObjetoGasto,
            TerminoDeBusqueda = TerminoDeBusqueda,
            Ejercicio = ejercicio,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros
        };

        var validationResult = new ListarObjetosdeGastosRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ObtenerObjetosGasto(request);

        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<ObjetodeGastosDTO>>>
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
                Data = new List<ObjetodeGastosDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ListarObjetosdeGastosporNroSolicitud")]
    [Endpoint("ListarObjetosdeGastosporNroSolicitud")]
    [SwaggerOperation(
        Summary = "Permite visualizar solicitudes de bienes con estado abierto para su aprobacion o rechazo",
        Description = "Listar todas las solicitudes recibidas con sus respectivos detalles de Objeto de gasto")]
    public async Task<IActionResult> ListarObjetosdeGastosporNroSolicitud(
        [FromQuery][Description("Expresion que se utilizara como termino de busqueda")] string? terminoDeBusqueda,
        [FromQuery][Description("Valor que indica el estado del objeto de gasto")] string? objetoGastos,
        [FromQuery][Description("Valor que indica el estado del objeto de gasto")] string? costoUnitario,
        [FromQuery][Description("Valor que indica el estado del objeto de gasto")] string? cantidadTotal,
        [FromQuery][Description("Valor que indica el estado del objeto de gasto")] string? estado,
        [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
        [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
        [FromQuery][Description("Valor que indica el codigo de la solicitud")] int codigoSolicitud,
        [FromQuery][Description("Valor que indica el codigo del objeto de gasto")] int? codigoOBG)
    {

        var request = new ListaObjetoGastosdeSolicitudRequest
        {
            ObjetoDeGasto = objetoGastos,
            CostoUnitario = costoUnitario,
            CantidadTotal = cantidadTotal,
            Estado = estado,
            TerminoDeBusqueda = terminoDeBusqueda,
            CodigoSolicitud = codigoSolicitud,
            Pagina = pagina,
            CantidadRegistros = cantidadRegistros,
            CodigoObjetoGasto = codigoOBG
        };

        var validationResult = new ObjetoGastodeSolicitudRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListarObjetodeGastosporCodigoSolicitud(request);


        if (resultado != null && resultado.Items.Any())
        {
            return Ok(new ApiResponse<Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>>
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
                Data = new List<DetalleObjetoGastoporNroSolicitudDTO>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }



    [HttpPost("InsertarCabeceraSolicitud")]
    [Endpoint("InsertarCabeceraSolicitud")]
    [SwaggerOperation(
        Summary = "Permite insertar la Cabecera de la Solicitud Bienes por Circunscripcion",
        Description = "Permitir insertar la cabecera de la solicitud por circunscripcion")]
    public async Task<IActionResult> InsertarSolicitudBienCircunscripcion(
    [FromBody][Description("Datos que corresponden a la Solicitud y Objetos por circunscripcion")]
    SolicitudCabeceraporCircunscripcionRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new SolicitudBienesCircunscripcionValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var solicitudInsertada = await _service.InsertarCabeceraSolicitudCircunscripcion(request);

        if (solicitudInsertada != null)
        {
            return Ok(new ApiResponse<Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>>
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
                Message = "No se pudo insertar la solicitud.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpPost("InsertarObjetodeGastosporNroSolicitud")]
    [Endpoint("InsertarObjetodeGastosporNroSolicitud")]
    [SwaggerOperation(
      Summary = "Permite insertar los objetos de gastos que pertenecen a una solicitu por Circunscripcion",
      Description = "Permitir insertar los Objetos de Gastos por el nro de la Solicityud que corresponda a una Circunscripcion")]
    public async Task<IActionResult> InsertarObjetosdeGastosporelNrodeSolicitud(
    [FromBody][Description("Datos que corresponden a la Solicitud y Objetos por circunscripcion")] ObjetosdeGastospornroSolicitudRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new ObjetosdeGastospornroSolicitudRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.InsertarObjetosdeGastosPorNrodeSolicitud(request);

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
        if (resultado.TotalRegistros == -1)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "El Objeto Seleccionado ya existe.",
                StatusCode = (int)HttpStatusCode.Conflict
            });
        }

        else
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No se pudo insertar la solicitud.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }
    }


    [HttpPut("ModificarSolicitudBienesCircunscripcion")]
    [Endpoint("ModificarSolicitudBienesCircunscripcion")]
    [SwaggerOperation(
        Summary = "Nos permite modificaar informacion de las solicitudes de bienes por circunscripcion",
        Description = "Modifica información de las solicitudes de bienes por circunscripcion")]
    public async Task<IActionResult> ModificarSolicitudBienCircunscripcion([FromBody] ModificarSolicitudRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var filasActualizadas = await _service.ModificarSolicitudBienCircunscripcion(request);

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


    [HttpPut("AnularSolicitudyObjetosGastos")]
    [Endpoint("AnularSolicitudyObjetosGastos")]
    [SwaggerOperation(
      Summary = "Nos permite modificaar el estado de la Solicitud o solicitudes y los objetos de gastos que pertenezcan a esas solicitudes",
      Description = "Modifica modificar el estado de la Solicitud o solicitudes y los objetos de gastos que pertenezcan a esas solicitudes")]
    public async Task<IActionResult> AnularSolicitudysusObjetosGastos(
    [FromBody][Description("Datos que correspondan a los datos de las solicitudes y el estado que asumira")] ActualizarEstadoSolicitudesRequest request)
    {
        request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

        var validationResult = new ActualizarEstadoSolicitudesRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var filasActualizadas = await _service.ActualizarEstadoSolicitudesysusObjetosGastos(request);

        if (filasActualizadas > 0)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = filasActualizadas,
                StatusCode = (int)HttpStatusCode.NoContent
            });
        }
        else if (filasActualizadas == 0)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Se cancela el proceso de anulación, existen objetos de gastos aprobados.",
                StatusCode = (int)HttpStatusCode.NotFound
            });
        }

        return BadRequest(new ApiResponse<string>
        {
            Success = false,
            Message = "Error inesperado al procesar la solicitud.",
            StatusCode = (int)HttpStatusCode.BadRequest
        });
    }


    [HttpDelete("EliminarObjetodeGastoPorNroSolicitud")]
    [Endpoint("EliminarObjetodeGastoPorNroSolicitud")]
    [SwaggerOperation(
    Summary = "Permite eliminar un objeto de gasto mediante el nro de la solicitud",
    Description = "Permitir eliminar un registro de objeto de gasto, mediando el codigo propio del objeto de gasto y el nro de solicitud al que pertenece")]
    public async Task<IActionResult> EliminarObjetodeGastoporNroSolicitud(
        [FromQuery][Description("Valor que indica el numero del objeto de gasto")] int codigoSolicitudObjeto,
        [FromQuery][Description("Valor que indica la descripcion del objeto de gasto")] int codigoSolicitud)
    {

        var resultado = await _service.EliminarObjetodeGasto(codigoSolicitudObjeto, codigoSolicitud);

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


    [HttpPut("ActualizarEstadoObjetoGasto")]
    [Endpoint("ActualizarEstadoObjetoGasto")]
    [SwaggerOperation(
       Summary = "Permite Actualizar el estado del objeto de gasto",
       Description = "Permitir actualizar el estado del objeto de gasto, mediante el codigo del objeto de gasto y codigo solicitiud")]
    public async Task<IActionResult> ActualizarEstadoObjetoGasto(
        [FromQuery][Description("Código único de solicitud bienes circunscripcion")] int codigoSolicitud,
        [FromQuery][Description("Codigo Unico del Objeto de Gasto")] int codigoObjetoGasto,
        [FromQuery][Description("Valor que indica el estado del objeto de gasto")] int estado)
    {

        var filasActualizadas = await _service.ActualizarEstadoObjetodeGasto(codigoSolicitud, codigoObjetoGasto, estado);
        if (filasActualizadas == 2)
        {
            return Ok(new ApiResponse<int>
            {
                Success = false,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = " El Objeto de gasto debe poseer al menos un bien para ser Aprobado.",
            });
        }
        else
        if (filasActualizadas == 3)
        {
            return Ok(new ApiResponse<int>
            {
                Success = false,
                Data = (int)filasActualizadas,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = " Los valores de Cantidad y Monto del bien no pueden ser cero.",
            });
        }
        else
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


    [HttpGet("ListarObjetosGastosyBienesporNroSolicitud")]
    [Endpoint("ListarObjetosGastosyBienesporNroSolicitud")]
    [SwaggerOperation(
        Summary = "Permite visualizar el Objeto de Gasto y los bienes que lo corresponden, por el Nro de Solicitud",
        Description = "Permitir visualizar el Objeto de Gasto Seleccionado con sus respectivos bienes por el nro de solicitud")]
    public async Task<IActionResult> VisualizarlosElObjetodeGastoySusBienesporNroSolicitud(
    [FromQuery][Description("Código único de solicitud bienes circunscripcion")] int codigoSolicitud,
    [FromQuery][Description("Valor que indica el codigo del objeto de gasto")] int codigoSolicitudObjeto,
    [FromQuery][Description("Valor que indica el inicio desde donde se obtendra el registro")] int pagina,
    [FromQuery][Description("Valor que indica la cantidad de registros a recuperar")] int cantidadRegistros,
    [FromQuery][Description("Valor que identifica el estado del estado de la solicitud")] int? estado)
    {
        estado ??= 1;
        var resultado = await _service.VisualizarObjetosdeGastosyBienesporNroSolicitud(codigoSolicitud, pagina, cantidadRegistros, codigoSolicitudObjeto, estado);

        if (resultado != null)
        {
            return Ok(new ApiResponse<Datos<ObjetosdeGastosyBienesporNroSolicitud>>
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
                Data = new List<ObjetosdeGastosyBienesporNroSolicitud>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });


        }
    }


    [HttpPost("ImportarDocumento")]
    [Endpoint("ImportarDocumento")]
    public async Task<IActionResult> ImportarDocumento(IFormFile archivo)
    {
        var ciUsuarioSesion = _usuarioService.DatosUsuario.NumeroDocumento;

        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Favor adjuntar el archivo para procesar",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var extension = Path.GetExtension(archivo.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "El archivo adjunto no es un archivo Excel válido.",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var (registros, errores, erroresValidacion) = await _service.ImportarDocumento(archivo, ciUsuarioSesion);

        if (errores != null && errores.Any())
        {
            return BadRequest(new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "El archivo adjunto contiene columnas vacias,o caracteres no permitidos,favor verficar y vuelva intentar",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Data = errores
            });
        }

        if (erroresValidacion != null && erroresValidacion.Any())
        {
            return BadRequest(new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "El archivo adjunto contiene datos inconsistentes,favor verficar y vuelva intentar",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Data = erroresValidacion
            });
        }

        return Ok(new { Registros = registros, Errores = errores, ErroresValidacion = erroresValidacion });
    }


    [HttpGet("ListadoVerificacionSolicitudes")]
    [Endpoint("ListadoVerificacionSolicitudes")]
    [SwaggerOperation(
      Summary = "Permite Listar las Solicitudes con estado abierto para su verificacion",
      Description = "Permitir Listar las Solicitudes con estado abierto para su verificacion")]
    public async Task<IActionResult> ListadoVerificacionSolicitudes(
    [FromQuery][Description("Datos necesarios para obtener la solicitud para la verficacion")] VerificacionSolicitudesRequest request)
    {

        var validationResult = new VerificacionSolicitudesRequestValidator().Validate(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var resultado = await _service.ListadoVerificacionSolicitudes(request);

        if (resultado != null)
        {
            return Ok(new ApiResponse<Datos<IEnumerable<VerificacionSolicitudes>>>
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
                Data = new List<VerificacionSolicitudes>(),
                StatusCode = (int)HttpStatusCode.OK,
                Errors = new List<string> { "La lista de elementos está vacía" }
            });
        }
    }


    [HttpGet("ValidarVersionAbierto")]
    [Endpoint("ValidarVersionAbierto")]
    [SwaggerOperation(
   Summary = "Permite validar si la version actual esta abierta",
   Description = "Permitir validar si la version actual esta abierta")]
    public async Task<IActionResult> ValidarVersionAbierto(
    [FromQuery][Description("Valor que indica el ejercicio actual")] int ejercicio)

    {
        if (ejercicio == null || ejercicio == 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = true,
                Message = "Ejercicio no puede estar vacio o cero",
                StatusCode = (int)HttpStatusCode.BadRequest
            });
        }

        var resultado = await _service.ValidarVersionAbierta(ejercicio);

        if (resultado != null)
        {
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
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

}


