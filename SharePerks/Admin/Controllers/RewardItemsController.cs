using Admin.Client.Models;
using Admin.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Entities;

namespace Admin.Controllers;

[ApiController]
[Route("api/admin/items")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class RewardItemsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RewardItemsController> _logger;

    public RewardItemsController(IUnitOfWork unitOfWork, ILogger<RewardItemsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RewardItem>> GetById(int id)
    {
        var item = await _unitOfWork.RewardItems.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<RewardItem>> Create([FromBody] CreateRewardItemInput request)
    {
        if (await _unitOfWork.RewardItems.ExistsByItemCodeAsync(request.ItemCode))
        {
            ModelState.AddModelError(nameof(request.ItemCode), "同じ商品コードが既に登録されています。");
            return ValidationProblem(ModelState);
        }

        var now = DateTime.UtcNow;
        var entity = new RewardItem
        {
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            ItemDescription = request.ItemDescription,
            RequiredPoints = request.RequiredPoints,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.RewardItems.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("優待商品を登録しました (ItemId: {ItemId}, ItemCode: {ItemCode})", entity.ItemId, entity.ItemCode);

        return CreatedAtAction(nameof(GetById), new { id = entity.ItemId }, entity);
    }
}
