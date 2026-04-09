using System.Net;
using System.Text.Json;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.Api.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Domain exception: {Code} — {Message}", ex.Error.Code, ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Error.Code, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context,
                HttpStatusCode.InternalServerError,
                "Internal.Error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context, HttpStatusCode status, string code, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(
            new { code, message },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(body);
    }
}