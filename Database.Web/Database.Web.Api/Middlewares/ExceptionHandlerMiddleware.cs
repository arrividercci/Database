using Newtonsoft.Json;

namespace Database.Web.Api.Middlewares
{
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                // log the error
                logger.LogCritical(exception, "error during executing {Context}", context.Request.Path.Value);
                HttpResponse response = context.Response;
                response.ContentType = "application/json";

                // get the response code and message
                (int status, string message) = GetResponse(exception);
                response.StatusCode = status;
                await response.WriteAsync(JsonConvert.SerializeObject(new { Error = message }));
            }
        }

        private (int status, string message) GetResponse(Exception exception)
        {
            switch (exception)
            {
                case ArgumentException argumentException:
                    return (StatusCodes.Status400BadRequest, argumentException.Message);
                default:
                    return (StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
