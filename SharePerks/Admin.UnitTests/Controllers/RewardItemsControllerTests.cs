#nullable enable
using Admin;
using Admin.Client;
using Admin.Client.Models;
using Admin.Controllers;
using Admin.Data;
using Admin.Data.Repositories;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos;
using Shared.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Admin.Controllers.UnitTests;

[TestClass]
public partial class RewardItemsControllerTests
{
    /// <summary>
    /// List はリポジトリが空リストを返した場合に、空リストを含む OkObjectResult を返す。
    /// </summary>
    [TestMethod]
    public async Task List_RepositoryReturnsEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        var items = new List<RewardItem>();

        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(items);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();

        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);

        // Act
        ActionResult<List<RewardItem>> actionResult = await controller.List();

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
        var ok = actionResult.Result as OkObjectResult;
        var returned = ok?.Value as List<RewardItem>;
        Assert.IsNotNull(returned, "空リストでも null ではなくリストインスタンスが返ること");
        Assert.IsEmpty(returned, "返却されたリストが空であること");
        repoMock.Verify(r => r.ListAsync(default), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// List はリポジトリが複数件のデータを返した場合に、そのまま OkObjectResult として返す。
    /// </summary>
    [TestMethod]
    public async Task List_RepositoryReturnsItems_ReturnsOkWithItems()
    {
        // Arrange
        var items = new List<RewardItem>
        {
            new()
            {
                ItemId = 1,
                ItemCode = "ITEM001",
                ItemName = "特典A",
                ItemDescription = "説明A",
                RequiredPoints = 100,
                DisplayOrder = 1,
                IsActive = true
            },
            new()
            {
                ItemId = 2,
                ItemCode = "ITEM002",
                ItemName = "特典B",
                ItemDescription = "説明B",
                RequiredPoints = 200,
                DisplayOrder = 2,
                IsActive = false
            }
        };

        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(items);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();

        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);

        // Act
        ActionResult<List<RewardItem>> actionResult = await controller.List();

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
        var ok = actionResult.Result as OkObjectResult;
        var returned = ok?.Value as List<RewardItem>;
        Assert.IsNotNull(returned, "複数件データがそのまま返ること");
        CollectionAssert.AreEqual(items, returned, "リポジトリの返却値が加工されず返ること");
        repoMock.Verify(r => r.ListAsync(default), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BulkUpdateOrderPoints_ValidRows_UpdatesChangedFieldsAndSavesOnce()
    {
        // Arrange
        var item1 = new RewardItem
        {
            ItemId = 1,
            ItemCode = "ITEM001",
            ItemName = "特典A",
            ItemDescription = "説明A",
            RequiredPoints = 100,
            DisplayOrder = 1,
            IsActive = true
        };
        var item2 = new RewardItem
        {
            ItemId = 2,
            ItemCode = "ITEM002",
            ItemName = "特典B",
            ItemDescription = "説明B",
            RequiredPoints = 200,
            DisplayOrder = 2,
            IsActive = true
        };

        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(item1);
        repoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(item2);
        repoMock.Setup(r => r.Update(item1));
        repoMock.Setup(r => r.Update(item2));
        repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(new List<RewardItem> { item1, item2 });

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(2);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();

        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);
        var request = new RewardItemBulkUpdateRequestDto(new[]
        {
            new RewardItemBulkUpdateRowDto(1, 150, 10),
            new RewardItemBulkUpdateRowDto(2, 250, 20)
        });

        // Act
        var actionResult = await controller.BulkUpdateOrderPoints(request);

        // Assert
        Assert.AreEqual(150, item1.RequiredPoints);
        Assert.AreEqual(10, item1.DisplayOrder);
        Assert.AreEqual(250, item2.RequiredPoints);
        Assert.AreEqual(20, item2.DisplayOrder);
        Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        repoMock.Verify(r => r.GetByIdAsync(1, default), Times.Once);
        repoMock.Verify(r => r.GetByIdAsync(2, default), Times.Once);
        repoMock.Verify(r => r.Update(item1), Times.Once);
        repoMock.Verify(r => r.Update(item2), Times.Once);
        repoMock.Verify(r => r.ListAsync(default), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BulkUpdateOrderPoints_InvalidValues_ReturnsValidationProblem()
    {
        // Arrange
        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();
        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);
        var request = new RewardItemBulkUpdateRequestDto(new[]
        {
            new RewardItemBulkUpdateRowDto(1, 0, -1)
        });

        // Act
        var actionResult = await controller.BulkUpdateOrderPoints(request);

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(ObjectResult));
        var objectResult = (ObjectResult)actionResult.Result!;
        Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BulkUpdateOrderPoints_NullItems_ReturnsValidationProblem()
    {
        // Arrange
        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();
        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);
        var request = new RewardItemBulkUpdateRequestDto(null!);

        // Act
        var actionResult = await controller.BulkUpdateOrderPoints(request);

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(ObjectResult));
        var objectResult = (ObjectResult)actionResult.Result!;
        Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BulkUpdateOrderPoints_DuplicateItemIds_ReturnsValidationProblem()
    {
        // Arrange
        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();
        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);
        var request = new RewardItemBulkUpdateRequestDto(new[]
        {
            new RewardItemBulkUpdateRowDto(1, 100, 1),
            new RewardItemBulkUpdateRowDto(1, 200, 2)
        });

        // Act
        var actionResult = await controller.BulkUpdateOrderPoints(request);

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(ObjectResult));
        var objectResult = (ObjectResult)actionResult.Result!;
        Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BulkUpdateOrderPoints_MissingItem_ReturnsValidationProblem()
    {
        // Arrange
        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((RewardItem?)null);

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uowMock.SetupGet(u => u.RewardItems).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<RewardItemsController>>();
        var envMock = new Mock<IWebHostEnvironment>();
        var controller = new RewardItemsController(uowMock.Object, loggerMock.Object, envMock.Object);
        var request = new RewardItemBulkUpdateRequestDto(new[]
        {
            new RewardItemBulkUpdateRowDto(99, 100, 1)
        });

        // Act
        var actionResult = await controller.BulkUpdateOrderPoints(request);

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(ObjectResult));
        var objectResult = (ObjectResult)actionResult.Result!;
        Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        repoMock.Verify(r => r.GetByIdAsync(99, default), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }

}
