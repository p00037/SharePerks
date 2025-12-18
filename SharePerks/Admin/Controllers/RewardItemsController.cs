using Admin.Data;
using Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Admin.Controllers;

[ApiController]
[Route("api/admin/items")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class RewardItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RewardItemsController> _logger;

    public RewardItemsController(ApplicationDbContext context, ILogger<RewardItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RewardItem>> GetById(int id)
    {
        var item = await _context.RewardItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<RewardItem>> Create([FromBody] CreateRewardItemRequest request)
    {
        if (await _context.RewardItems.AnyAsync(x => x.ItemCode == request.ItemCode))
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

        _context.RewardItems.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("優待商品を登録しました (ItemId: {ItemId}, ItemCode: {ItemCode})", entity.ItemId, entity.ItemCode);

        return CreatedAtAction(nameof(GetById), new { id = entity.ItemId }, entity);
    }
}
