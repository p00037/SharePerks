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

}
