using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SlotFlow.Api.Api.Extensions;
using SlotFlow.Api.Api.Validators;
using SlotFlow.Api.Application.UseCases.Resources;

namespace SlotFlow.Api.Api.Controllers;

[ApiController]
[Route("api/resources")]
public sealed class ResourcesController(
    CreateResource createResource,
    GetResources getResources,
    GetResourceById getResourceById) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateResourceRequest request,
        [FromServices] IValidator<CreateResourceRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "One or more validation errors occurred.",
                errors = validation.ToDictionary()
            });

        var result = await createResource.ExecuteAsync(
            new CreateResource.Request(
                request.Name,
                request.Description ?? string.Empty,
                request.HoldDurationMinutes,
                request.InitialSlotCount),
            ct);

        return result.ToHttpResult(dto => CreatedAtAction(
            nameof(GetById), new { id = dto.Id }, dto));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await getResources.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await getResourceById.ExecuteAsync(id, ct);
        return result.ToHttpResult(Ok);
    }
}