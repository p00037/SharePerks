using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Data;
using Shared.Dtos;
using Shared.Entities;

namespace User.Controllers;

[ApiController]
[Route("api/shareholder/order")]
[Authorize(Roles = ApplicationRoles.User)]
public sealed class ShareholderOrdersController(IUnitOfWork unitOfWork, ILogger<ShareholderOrdersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateRewardOrderResponseDto>> PostAsync(
        [FromBody] CreateRewardOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Items"] = ["商品を1件以上選択してください。"]
            }));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var shareholder = await unitOfWork.Shareholders.GetByUserIdAsync(userId, cancellationToken);
        if (shareholder is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "株主情報が見つかりません。",
                Detail = "株主情報が未登録、または無効化されています。",
                Status = StatusCodes.Status404NotFound
            });
        }

        var duplicated = await unitOfWork.RewardOrders.ExistsByShareholderIdAsync(shareholder.ShareholderId, cancellationToken);
        if (duplicated)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Order"] = ["すでに申し込みが完了しています。再申し込みはできません。"]
            }));
        }

        var requestedItemIds = request.Items.Select(x => x.ItemId).Distinct().ToList();
        var activeItems = await unitOfWork.RewardItems.ListActiveAsync(cancellationToken);
        var itemMap = activeItems
            .Where(x => requestedItemIds.Contains(x.ItemId))
            .ToDictionary(x => x.ItemId);

        var orderItems = new List<RewardOrderItem>();
        foreach (var requestedItem in request.Items)
        {
            if (requestedItem.Quantity <= 0)
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["Items"] = ["数量は1以上で入力してください。"]
                }));
            }

            if (!itemMap.TryGetValue(requestedItem.ItemId, out var rewardItem))
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["Items"] = ["選択された商品が存在しないか、現在は申し込みできません。"]
                }));
            }

            orderItems.Add(new RewardOrderItem
            {
                ItemId = rewardItem.ItemId,
                Quantity = requestedItem.Quantity,
                SubtotalPoints = rewardItem.RequiredPoints * requestedItem.Quantity
            });
        }

        var totalPoints = orderItems.Sum(x => x.SubtotalPoints);
        var availablePoints = shareholder.GrantedPoints - shareholder.UsedPoints;
        if (totalPoints > availablePoints)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Items"] = ["使用可能ポイントを超えています。商品と数量を見直してください。"]
            }));
        }

        var utcNow = DateTime.UtcNow;
        var rewardOrder = new RewardOrder
        {
            ShareholderId = shareholder.ShareholderId,
            PostalCode = request.PostalCode,
            Address1 = request.Address1,
            Address2 = request.Address2,
            Address3 = request.Address3,
            PhoneNumber = request.PhoneNumber,
            TotalPoints = totalPoints,
            OrderedAt = utcNow,
            IsCancelled = false,
            IsExported = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            OrderItems = orderItems
        };

        unitOfWork.RewardOrders.Add(rewardOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("申込登録が完了しました。OrderId: {OrderId}, ShareholderId: {ShareholderId}", rewardOrder.OrderId, shareholder.ShareholderId);

        return Ok(new CreateRewardOrderResponseDto(rewardOrder.OrderId));
    }
}
