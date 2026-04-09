using Microsoft.AspNetCore.Mvc;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Errors;

namespace SlotFlow.Api.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value!);

        return result.Error.ToHttpResult();
    }

    public static IActionResult ToHttpResult(this Error error)
    {
        var body = new { code = error.Code, message = error.Message };

        return error.Code switch
        {
            var c when c.EndsWith(".NotFound") =>
                new NotFoundObjectResult(body),

            var c when c == "Reservation.NotOwnedByUser" =>
                new ObjectResult(body) { StatusCode = StatusCodes.Status403Forbidden },

            _ => new ConflictObjectResult(body)
        };
    }
}