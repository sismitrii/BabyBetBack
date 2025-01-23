using Application.Exceptions;
using SendGrid.Helpers.Errors.Model;

namespace BabyBetBack.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is BetException || ex is BetException || ex is UserNotFoundException)
        {
            await HandleCustomExceptionAsync(context, ex);
        }
    }

    private static Task HandleCustomExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = new
        {
            Message = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}