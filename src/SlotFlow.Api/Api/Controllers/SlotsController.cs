using Microsoft.AspNetCore.Mvc;
using SlotFlow.Api.Api.Extensions;
using SlotFlow.Api.Application.UseCases.Slots;

namespace SlotFlow.Api.Api.Controllers;

[ApiController]
[Route("api/resources/{resourceId:guid}/slots")]
public sealed class SlotsController(GetSlotsByResource getSlotsByResource) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByResource(Guid resourceId, CancellationToken ct)
    {
        var result = await getSlotsByResource.ExecuteAsync(resourceId, ct);
        return result.ToHttpResult(Ok);
    }
}