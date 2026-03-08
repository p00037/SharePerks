using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Data;
using Shared.Dtos;

namespace User.Controllers;

[ApiController]
[Route("api/shareholder/items")]
[Authorize(Roles = ApplicationRoles.User)]
public sealed class ShareholderItemsController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RewardItemSummaryDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var items = await unitOfWork.RewardItems.ListActiveAsync(cancellationToken);
        var response = items
            .Select(item => new RewardItemSummaryDto(
                item.ItemId,
                item.ItemCode,
                item.ItemName,
                item.ItemDescription,
                item.RequiredPoints,
                item.ImagePath))
            .ToList();

        return Ok(response);
    }
}
