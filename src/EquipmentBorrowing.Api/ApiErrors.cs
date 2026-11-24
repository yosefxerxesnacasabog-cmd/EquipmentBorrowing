using EquipmentBorrowing.Application.Interfaces;
using Microsoft.AspNetCore.Diagnostics;

namespace EquipmentBorrowing.Api;

internal sealed record ApiError(string Code, string Message);

internal static class ApiErrors
{
    internal static ApiError ForStatus(int status) => status switch
    {
        400 => new("invalid_request", "Check the request fields and their formats."),
        404 => new("not_found", "The requested resource was not found."),
        405 => new("method_not_allowed", "This HTTP method is not supported for the requested route."),
        415 => new("unsupported_media_type", "Send the request body as JSON."),
        _ => new("server_error", "The server could not complete the request.")
    };

    internal static async Task HandleExceptionAsync(HttpContext context)
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = error is BadHttpRequestException badRequest &&
            badRequest.StatusCode is 400 or 404 or 405 or 415
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError;
        var body = error is StorageException
            ? new ApiError("database_error", "The server could not load records from the database. Try again later.")
            : ForStatus(context.Response.StatusCode);
        await context.Response.WriteAsJsonAsync(body);
    }
}
