using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using FluentValidation;
using System.Net;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebApi.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                throw new UnauthorizedAccessException();               
            }
        }       
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Errors = new List<string>()
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.AddRange(validationException.Errors.Select(e => e.ErrorMessage));
                break;

            case ReglasdeNegocioException ReglasNegocioException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.Add($"Tipo de excepción: {exception.GetType().Name}");
                response.Errors.Add($"Mensaje de la excepción: {exception.Message}");
                break;

            case KeyNotFoundException _:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Errors.Add("El dato solicitado no fue encontrado");
                break;

            case ErrorAutorizacionUsuarioException errorAutorizacionException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Errors.Add($"Tipo de excepción: {exception.GetType().Name}");
                response.Errors.Add($"Mensaje de la excepción: {exception.Message}");
                break;

            case UnauthorizedAccessException unauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Errors.Add($"Tipo de excepción: {exception.GetType().Name}");
                response.Errors.Add("Usuario no Autorizado para consumir el recurso seleccionado");
                break;

            case ParametroFaltanteCadenaConexionException parametrosConexionFaltanteException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.Add($"Tipo de excepción: {exception.GetType().Name}");
                response.Errors.Add($"Mensaje de la excepción: {exception.Message}");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Errors.Add("Ocurrió un error interno en el servidor");
                response.Errors.Add($"Tipo de excepción: {exception.GetType().Name}");
                response.Errors.Add($"Mensaje de la excepción: {exception.Message}");
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsync(jsonResponse);
    }
  
}

