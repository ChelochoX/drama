using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;
using FluentValidation;
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
    public class ConsolidarAnteproyectoModificarObjetoGastoController : ControllerBase
    {
        private readonly IConsolidarAnteproyectoModificarObjetoGastoService _service;
        private readonly IDatosUsuarioService _usuarioService;

        public ConsolidarAnteproyectoModificarObjetoGastoController(IConsolidarAnteproyectoModificarObjetoGastoService service, IDatosUsuarioService usuarioService)
        {
            _service = service;
            _usuarioService = usuarioService;
        }


        [HttpGet("ListarMateriayCentroResponsabilidadporUsuario")]
        [Endpoint("ListarMateriayCentroResponsabilidadporUsuario")]
        [SwaggerOperation(
        Summary = "Permite obtener el dato de Centro de Responsabilidad y Materia a la que pertenece el usuario",
        Description = "Permitir obtener el dato de Centro de Responsabilidad y Materia a la que pertenece el usuario")]
        public async Task<IActionResult> ListarMateriayCentroResponsabilidadporUsuario()
        {
            var cedula = _usuarioService.DatosUsuario.NumeroDocumento;

            if (cedula == null || cedula.Count() == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Valor de Cedula es Obligatoria",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }

            var resultado = await _service.ObtenerCentroResponsabilidadyMateriaporUsuario(cedula);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<CentroResponsabilidadyMateriaDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<CentroResponsabilidadyMateriaDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            }); ; ;
        }


        [HttpGet("ListarCircunscripciones")]
        [Endpoint("ListarCircunscripciones")]
        [SwaggerOperation(
        Summary = "Permite listar las circunscripciones",
        Description = "Permitir listar las circunscripciones")]
        public async Task<IActionResult> ListarCircunscripciones()
        {
            var resultado = await _service.ObtenerCircunscripciones();

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<CircunscripcionesparaConsolidadoAnteproyectoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<CircunscripcionesparaConsolidadoAnteproyectoDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ListarObjetosdeGastosConsolidadoAnteproyecto")]
        [Endpoint("ListarObjetosdeGastosConsolidadoAnteproyecto")]
        [SwaggerOperation(
            Summary = "Permite listar los objetos de gastos",
            Description = "Permitir listar los objetos de gastos")]
        public async Task<IActionResult> ListarObjetosdeGastosConsolidadoAnteproyecto(
        [FromQuery][Description("Valor que indica el ejercicio de busqueda")] int ejercicio)
        {
            if (ejercicio == null || ejercicio == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Valor de Ejercicio es Obligatoria",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }

            var resultado = await _service.ObtenerObjetosGastos(ejercicio);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<ObjetosGastosConsolidacionAnteproyectoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<ObjetosGastosConsolidacionAnteproyectoDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ListarDatosparaConfiguracionPresupuestariaConsolidado")]
        [SwaggerOperation(
          Summary = "Permite listar los datos necesario para la configuracion presupuestaria del consolidado",
          Description = "Permitir listar los datos necesario para la configuracion presupuestaria del consolidado")]
        public async Task<IActionResult> ListarDatosparaConfiguracionPresupuestariaConsolidado(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener la configuracion presupuestaria del consolidado")]
        DatosparaConfiguracionPresupuestariaConsolidadoRequest request)
        {
            var validationResult = new DatosparaConfiguracionPresupuestariaRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerDatosparalaConfiguracionPresupuestaria(request);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<DatosparaConfiguracionPresupuestariaConsolidadoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<DatosparaConfiguracionPresupuestariaConsolidadoDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ListarBienesVersionesAnteproyectoConsolidado")]
        [SwaggerOperation(
         Summary = "Permite listar los datos de bienes de anteproyecto bienes del consolidado",
         Description = "Permitir listar los datos de bienes de anteproyecto bienes del consolidado")]
        public async Task<IActionResult> ListarBienesVersionesAnteproyectoConsolidado(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener los datos de versiones anteproyecto bienes")]
        BienesdeAnteproyectoBienesConsolidadoRequest request)
        {
            var validationResult = new BienesdeAnteproyectoBienesConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerBienesdeAnteproyectoObjeto(request);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<BienesdeAnteproyectoBienesConsolidadoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<IEnumerable<BienesdeAnteproyectoBienesConsolidadoDTO>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ListarBienesparaVersionesAnteproyectoConsolidado")]
        [SwaggerOperation(
        Summary = "Permite listar los datos de bienes para anteproyecto bienes del consolidado",
        Description = "Permitir listar los datos de bienes para anteproyecto bienes del consolidado")]
        public async Task<IActionResult> ListarBienesparaVersionesAnteproyectoConsolidado(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener bienes para insertar en versiones anteproyecto bienes")]
        BienesparaConsolidadoRequest request)
        {
            var validationResult = new BienesparaConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerBienesparaConsolidado(request);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<BienesparaConsolidadoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<Datos<IEnumerable<BienesparaConsolidadoDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpPost("InsertarConfiguracionPresupuestariadesdeConsolidado")]
        [SwaggerOperation(
        Summary = "Permite insertar la Configuracion presupuestaria desde la Consolidacion de Anteproyecto",
        Description = "Permitir insertar la configuracion presupuestaria desde la Consolidacion de Anteproyecto")]
        public async Task<IActionResult> InsertarConfiguracionPresupuestariadesdeConsolidado(
        [FromBody][Description("Datos que corresponden a la solicitud de insertar para la configuracion presupuestaria")]
        ConfiguracionPresupuestariaConsolidadoRequest request)
        {
            var validationResult = new ConfiguracionPresupuestariaConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var configuracionInsertada = await _service.InsertarConfiguracionPresupuestariaDesdeConsolidado(request);

            if (configuracionInsertada != null || configuracionInsertada != 0)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = configuracionInsertada,
                    StatusCode = (int)HttpStatusCode.Created
                });
            }
            else
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se pudo crear la configuracion presupuestaria desde la consolidacionde versiones de anteproyecto",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        }


        [HttpPost("InsertarObjetodeGastodesdeConsolidado")]
        [SwaggerOperation(
          Summary = "Permite insertar el objeto de gasto desde la Consolidacion de Anteproyecto",
          Description = "Permitir insertar el objeto de gasto desde la Consolidacion de Anteproyecto")]
        public async Task<IActionResult> InsertarObjetodeGastodesdeConsolidado(
        [FromBody][Description("Datos que corresponden a la solicitud de insertar en versionados de objetos de gastos en la consolidacion")]
        DatosparaVersionesAnteproyectoObjetoConsolidadoRequest request)
        {
            var validationResult = new DatosparaVersionesAnteproyectoObjetoConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.GestionarInsertarVersionadoObjetoGastoConsolidado(request);

            if (resultado != 0 && resultado != -1)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = resultado,
                    StatusCode = (int)HttpStatusCode.Created
                });
            }
            else
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se pudo crear el objeto de gasto desde la consolidacion de versiones de anteproyecto de objetos",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        }


        [HttpPost("InsertarBienesdesdeConsolidado")]
        [SwaggerOperation(
         Summary = "Permite insertar el bien desde la Consolidacion de Anteproyecto",
         Description = "Permitir insertar el bien desde la Consolidacion de Anteproyecto")]
        public async Task<IActionResult> InsertarBienesdesdeConsolidado(
        [FromBody][Description("Datos que corresponden a la solicitud de insertar en versionados de bienes en la consolidacion")]
        DatosparaVersionesAnteproyectoBienesConsolidadoRequest request)
        {
            request.UsuarioInserto = _usuarioService.DatosUsuario.NumeroDocumento;

            var validationResult = new DatosparaVersionesAnteproyectoBienesConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.GestionarInsertarVersionadoBienesConsolidado(request);

            if (resultado != null && resultado != 0 && resultado != -1)
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
                    Message = "El Bien Seleccionado ya pertenece a una configuracion",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            else
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se pudo crear el bien desde la consolidacion de versiones de anteproyecto de objetos",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        }


        [HttpGet("ValidarExistenciaObjetoGastoenConsolidado")]
        [SwaggerOperation(
         Summary = "Permite validar si existe el objeto de gasto a insertar en la pantalla de consolidado",
         Description = "Permitir validar si existe el objeto de gasto a insertar en la pantalla de consolidado")]
        public async Task<IActionResult> ValidarExistenciaObjetoGastoenConsolidado(
        [FromQuery][Description("Valores que indican las propiedades necesarias para validar la existencia de objeto de gasto en versiones en la pantalla de consolidado")]
        ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest request)
        {
            var validationResult = new ValidarExistenciaenAnteproyectoObjetoConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ValidarExistenciaenAnteproyectoObjetoConsolidado(request);

            if (resultado > 0)
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Data = resultado,
                    Message = "Ya existe un objeto de gasto para esta configuracion presupuestaria",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }

            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpPut("ModificarObjetoGastodeAnteproyectoObjetoConsolidado")]
        [SwaggerOperation(
        Summary = "Permite Modificar el objeto desde la Consolidacion de Anteproyecto",
        Description = "Permitir insertar el objeto desde la Consolidacion de Anteproyecto")]
        public async Task<IActionResult> ModificarObjetoGastodeAnteproyectoObjetoConsolidado(
        [FromBody][Description("Datos que corresponden a la solicitud de modificar en versionados anteproyecrto objeto en la consolidacion")]
        ModificarOBGConsolidadoRequest request)
        {
            var validationResult = new ModificarOBGConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ModificarAnteproyectoObjetoConsolidado(request);

            if (resultado != null || resultado != 0)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = resultado,
                    StatusCode = (int)HttpStatusCode.NoContent
                });
            }
            else
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se pudo actualizar el objeto de gasto desde la consolidacion de versiones de anteproyecto de objetos",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        }


        [HttpPut("ModificarBienesdeAnteproyectoBienesConsolidado")]
        [SwaggerOperation(
          Summary = "Permite Modificar el bien desde la Consolidacion de Anteproyecto",
          Description = "Permitir insertar el bien desde la Consolidacion de Anteproyecto")]
        public async Task<IActionResult> ModificarBienesdeAnteproyectoBienesConsolidado(
        [FromBody][Description("Datos que corresponden a la solicitud de modificar en versionados anteproyecrto bienes en la consolidacion")]
        ModificarBienesConsolidadoRequest request)
        {
            request.CedulaUsuario = _usuarioService.DatosUsuario.NumeroDocumento;

            var validationResult = new ModificarBienesConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ModificarAnteproyectoBienesConsolidado(request);

            if (resultado != null || resultado != 0)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = resultado,
                    StatusCode = (int)HttpStatusCode.NoContent
                });
            }
            else
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se pudo actualizar el bien desde la consolidacion de versiones de anteproyecto de bienes",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
        }


        [HttpGet("ListarMontosPresupuestoInicialyModificaiones")]
        [SwaggerOperation(
        Summary = "Permite listar los montos de presupuesto inicial y modificaciones",
        Description = "Permitir listar los montos de presupuesto inicial y modificaciones")]
        public async Task<IActionResult> ListarMontosPresupuestoInicialyModificaiones(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener los montos de presupuesto inicial y modificaciones")]
        PresupuestoInicialyModificacionesConsolidadoRequest request)
        {
            var validationResult = new ObtenerPresupuestoInicialyModificacionesConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerPresupuestoInicialyModificaciones(request);

            if (resultado == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new List<PresupuestoInicialyModificacionesConsolidadoDTO>(),
                    StatusCode = (int)HttpStatusCode.OK,
                    Errors = new List<string> { "La lista de elementos está vacía" }
                });
            }

            return Ok(new ApiResponse<PresupuestoInicialyModificacionesConsolidadoDTO>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpDelete("EliminarBienesdesdeConsolidadoAnteproyectoModif")]
        [SwaggerOperation(
        Summary = "Permite eliminar registro de la tabla de versiones anteproyecto bienes desde el consolidado de Versiones anteproyecto",
        Description = "Permitir eliminar registro de la tabla de versiones anteproyecto bienes desde el consolidado de Versiones anteproyecto")]
        public async Task<IActionResult> EliminarBienesdesdeConsolidadoAnteproyecto(
        [FromQuery][Description("Valor que indica el valor del codigo de la version anteproyecto")] int codigoVersionAnteproyecto,
        [FromQuery][Description("Valor que indica el valor del codigo de la version anteproyecto de objeto de gasto")] int codigoVersionAnteproyectoOBG,
        [FromQuery][Description("Valor que indica el valor del codigo de la version anteproyecto de bienes")] int codigoVersionAnteproyectoBien,
        [FromQuery][Description("Valor que indica el valor del codigo de la configuracion presupuestaria")] int codigoConfiguracionPresupuestaria,
        [FromQuery][Description("Valor que indica el valor del codigo de la materia")] int codigoMateria,
        [FromQuery][Description("Valor que indica el valor del codigo de centro responsabilidad")] int codigoCentroResponsabilidad)
        {
            if (codigoVersionAnteproyecto == 0 || codigoVersionAnteproyectoOBG == 0 || codigoVersionAnteproyectoBien == 0 || codigoConfiguracionPresupuestaria == 0
                || codigoMateria == 0 || codigoCentroResponsabilidad == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Parametro no puede tener valor cero o nulo",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }

            var resultado = await _service.EliminarBiendesdeConsolidadoModif(codigoVersionAnteproyecto, codigoVersionAnteproyectoOBG, codigoVersionAnteproyectoBien, codigoConfiguracionPresupuestaria,
                codigoMateria,codigoCentroResponsabilidad);

            if (resultado == null || resultado == 0)
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


        [HttpGet("ObtenerEjercicioActivoFechaCierre")]
        [SwaggerOperation(
          Summary = "Permite obtener el ejercicio activo para el prespuesto",
          Description = "Permitir obtener el ejercicio activo para el prespuesto")]
        public async Task<IActionResult> ObtenerEjercicioActivoFechaCierre()
        {
            var resultado = await _service.ObtenerEjercicioActivo();

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


        [HttpGet("ObtenerMontoPlanificado")]
        [SwaggerOperation(
        Summary = "Permite obtener el monto planificado",
        Description = "Permitir obtener el monto planificado")]
        public async Task<IActionResult> ObtenerMontoPlanificado(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener el monto planificado")]
        MontoPlanificadoRequest request)
        {
            var validationResult = new MontoPlanificadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerMontoPlanificado(request);

            if (resultado == null || resultado == 0)
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


        [HttpGet("ValidarsiOBGyBienestienenConfiguracionPresupuestaria")]
        [SwaggerOperation(
        Summary = "Permite validar si existe algun objeto de gasto con bienes sin configuracion presupuestaria",
        Description = "Permitir validar si existe algun objeto de gasto con bienes sin configuracion presupuestaria")]
        public async Task<IActionResult> ValidarsiOBGyBienestienenConfiguracionPresupuestaria(
       [FromQuery][Description("Valores que indican la version del anteproyecto")] int codigoVersion)
        {
            if (codigoVersion == null || codigoVersion == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Codigo Version Anteproyecto no puede ser nulo o valor cero",
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }            

            var resultado = await _service.ValidarOBGPerteneceaConfiguracion(codigoVersion);

            if (resultado == null || resultado.TotalRegistros == 0)
            {
                return Ok(new ApiResponse<Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>>
                {
                    Success = true,
                    Data = resultado,
                    StatusCode = (int)HttpStatusCode.NotFound
                });
            }

            return Ok(new ApiResponse<Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }


        [HttpGet("ObtenerPresupuestoInicialyModificaciones")]
        [SwaggerOperation(
         Summary = "Permite Obtener el valor del presupuesto inicial y modificaciones del periodo anterior",
         Description = "Permitir Obtener el valor del presupuesto inicial y modificaciones del periodo anterior")]
        public async Task<IActionResult> ObtenerPresupuestoInicialyModificaciones(
        [FromQuery][Description("Valores que indican las propiedades necesarias para obtener el presupuesto inicial y modificaciones")]
        PresupuestoInicialyModificacionesConsolidadoRequest request)
        {
            var validationResult = new ObtenerPresupuestoInicialyModificacionesConsolidadoRequestValidator().Validate(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var resultado = await _service.ObtenerPresupuestoInicialyModificaciones(request);          

            return Ok(new ApiResponse<PresupuestoInicialyModificacionesConsolidadoDTO>
            {
                Success = true,
                Data = resultado,
                StatusCode = (int)HttpStatusCode.OK
            });
        }
    }
}
