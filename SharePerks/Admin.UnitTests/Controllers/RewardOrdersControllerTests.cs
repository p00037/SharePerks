#nullable enable
using Admin.Controllers;
using Admin.Data;
using Admin.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Entities;

namespace Admin.Controllers.UnitTests;

[TestClass]
public sealed class RewardOrdersControllerTests
{
    [TestMethod]
    public async Task ExportAsync_UnexportedScope_ReturnsCsvAndMarksOrdersExported()
    {
        var orders = new List<RewardOrder>
        {
            new()
            {
                OrderId = 1,
                ShareholderId = 10,
                PostalCode = "100-0001",
                Address1 = "Tokyo",
                Address2 = "Chiyoda",
                Address3 = "1-1-1",
                PhoneNumber = "03-0000-0000",
                TotalPoints = 500,
                OrderedAt = new DateTime(2026, 4, 7, 9, 30, 0, DateTimeKind.Utc),
                IsCancelled = false,
                IsExported = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Shareholder = new Shareholder
                {
                    UserId = "user-1",
                    ShareholderNo = "SH0001",
                    ShareholderName = "Shareholder Taro",
                    PostalCode = "100-0001",
                    Address1 = "Tokyo",
                    Address2 = "Chiyoda",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                OrderItems = new List<RewardOrderItem>
                {
                    new()
                    {
                        ItemId = 100,
                        Quantity = 1,
                        SubtotalPoints = 500,
                        Item = new RewardItem
                        {
                            ItemCode = "ITEM001",
                            ItemName = "Reward A",
                            RequiredPoints = 500,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    }
                }
            }
        };

        var repoMock = new Mock<IRewardOrderRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.ListForExportAsync(true, default)).ReturnsAsync(orders);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(x => x.RewardOrders).Returns(repoMock.Object);
        uowMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var loggerMock = new Mock<ILogger<RewardOrdersController>>();
        var controller = new RewardOrdersController(uowMock.Object, loggerMock.Object);

        var result = await controller.ExportAsync("unexported");

        Assert.IsInstanceOfType(result, typeof(FileContentResult));
        Assert.IsTrue(orders[0].IsExported);
        repoMock.Verify(r => r.ListForExportAsync(true, default), Times.Once);
        uowMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
