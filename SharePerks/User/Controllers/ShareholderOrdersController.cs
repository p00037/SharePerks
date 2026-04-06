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
    [HttpGet]
    public async Task<ActionResult<ShareholderOrderDto>> GetAsync(CancellationToken cancellationToken)
    {
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

        var rewardOrder = await unitOfWork.RewardOrders.GetByShareholderIdAsync(shareholder.ShareholderId, cancellationToken);
        if (rewardOrder is null)
        {
            return NotFound();
        }

        return Ok(ToDto(rewardOrder));
    }

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

        var shareholder = await unitOfWork.Shareholders.GetByUserIdForUpdateAsync(userId, cancellationToken);
        if (shareholder is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "株主情報が見つかりません。",
                Detail = "株主情報が未登録、または無効化されています。",
                Status = StatusCodes.Status404NotFound
            });
        }

        var existingOrder = await unitOfWork.RewardOrders.GetByShareholderIdForUpdateAsync(shareholder.ShareholderId, cancellationToken);
        if (existingOrder?.IsExported == true)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Order"] = ["申込内容は既に出力済みのため更新できません。管理者へお問い合わせください。"]
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
        var existingTotalPoints = existingOrder?.TotalPoints ?? 0;
        var availablePoints = shareholder.GrantedPoints - shareholder.UsedPoints + existingTotalPoints;
        if (totalPoints > availablePoints)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Items"] = ["使用可能ポイントを超えています。商品と数量を見直してください。"]
            }));
        }

        var utcNow = DateTime.UtcNow;
        RewardOrder rewardOrder;
        if (existingOrder is null)
        {
            rewardOrder = new RewardOrder
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
        }
        else
        {
            rewardOrder = existingOrder;
            rewardOrder.PostalCode = request.PostalCode;
            rewardOrder.Address1 = request.Address1;
            rewardOrder.Address2 = request.Address2;
            rewardOrder.Address3 = request.Address3;
            rewardOrder.PhoneNumber = request.PhoneNumber;
            rewardOrder.TotalPoints = totalPoints;
            rewardOrder.OrderedAt = utcNow;
            rewardOrder.UpdatedAt = utcNow;
            unitOfWork.RewardOrders.ReplaceItems(rewardOrder, orderItems);
        }

        shareholder.UsedPoints = totalPoints;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            existingOrder is null
                ? "申込登録が完了しました。OrderId: {OrderId}, ShareholderId: {ShareholderId}"
                : "申込更新が完了しました。OrderId: {OrderId}, ShareholderId: {ShareholderId}",
            rewardOrder.OrderId,
            shareholder.ShareholderId);

        return Ok(new CreateRewardOrderResponseDto(rewardOrder.OrderId));
    }

    private static ShareholderOrderDto ToDto(RewardOrder rewardOrder)
    {
        return new ShareholderOrderDto(
            rewardOrder.OrderId,
            rewardOrder.PostalCode,
            rewardOrder.Address1,
            rewardOrder.Address2,
            rewardOrder.Address3,
            rewardOrder.PhoneNumber,
            rewardOrder.TotalPoints,
            rewardOrder.IsExported,
            rewardOrder.OrderItems
                .Where(x => x.Item is not null)
                .Select(x => new ShareholderOrderItemDto(
                    x.ItemId,
                    x.Item!.ItemCode,
                    x.Item.ItemName,
                    x.Item.ItemDescription,
                    x.Item.ImagePath,
                    x.Item.RequiredPoints,
                    x.Quantity))
                .ToList());
    }
}
