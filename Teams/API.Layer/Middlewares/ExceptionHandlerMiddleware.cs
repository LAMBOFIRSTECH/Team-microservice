using Newtonsoft.Json;

namespace Teams.API.Layer.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate _next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HandlerException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var error = new
            {
                title = ex.Title,
                message = ex.Message,
                reason = ex.Reason,
            };

            var json = JsonConvert.SerializeObject(error);
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var json = JsonConvert.SerializeObject(
                new
                {
                    title = "Internal Server Error",
                    message = ex.Message,
                    reason = "UnhandledException",
                }
            );
            await context.Response.WriteAsync(json);
        }
    }
}
