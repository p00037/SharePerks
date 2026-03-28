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
    /// The List action should return an OkObjectResult with an empty list when the repository returns an empty list.
    /// Conditions: repository returns an empty List&lt;RewardItem&gt;.
    /// Expected: OkObjectResult returned and the Value is an empty List&lt;RewardItem&gt;.
    /// </summary>
    [TestMethod]
    public async Task List_RepositoryReturnsEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        var items = new List<RewardItem>();

        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(items);

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
        Assert.IsNotNull(returned, "Expected an empty list instance, not null");
        Assert.AreEqual(0, returned.Count, "Expected the returned list to be empty");
        repoMock.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// The List action should return an OkObjectResult with a null Value when the repository returns null.
    /// Conditions: repository returns null (unexpected but possible).
    /// Expected: OkObjectResult returned and Value is null.
    /// </summary>
    [TestMethod]
    public async Task List_RepositoryReturnsNull_ReturnsOkWithNullValue()
    {
        // Arrange
        var repoMock = new Mock<IRewardItemRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync((List<RewardItem>?)null);

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
        Assert.IsNull(ok?.Value, "Expected OkObjectResult.Value to be null when repository returns null");
        repoMock.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}