using SSF.Interop.SIIFNacion.Application.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace SSF.Interop.SIIFNacion.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            // Desempaquetar la excepción real si viene envuelta
            var realEx = exception is AggregateException agg ? agg.InnerException ?? exception : exception;

            string errorMessage = realEx.Message;

            // Para errores HTTP de SIIF, propagar el mensaje con el status code de SIIF
            if (realEx is System.Net.Http.HttpRequestException httpEx)
            {
                errorMessage = httpEx.Message;
                // Si SIIF devolvió 4xx mapeamos a 502 Bad Gateway (error de upstream)
                statusCode = HttpStatusCode.BadGateway;
            }

            string result = JsonConvert.SerializeObject(new ErrorDeatils
            {
                ErrorMessage = errorMessage,
                ErrorType = "Failure"
            });

            switch (realEx)
            {
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(validationException.Errors);
                    break;
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                default:
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(result);
        }
    }

    public class ErrorDeatils
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }
}