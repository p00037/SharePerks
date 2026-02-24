using System.Collections.ObjectModel;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Shareholder.Client.Pages;
using Shareholder.Components;
using Shareholder.Components.Account;
using Shareholder.Data;
using Shared.Dtos;
using Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Serilog 構成
var sinkOptions = new MSSqlServerSinkOptions
{
    TableName = "LogsShareholder",
    AutoCreateSqlTable = true // 開発時のみ。運用時は false にして手動作成推奨。
};

var columnOptions = new ColumnOptions
{
    Store = new Collection<StandardColumn>
    {
        StandardColumn.TimeStamp,
        StandardColumn.Level,
        StandardColumn.Message,
        StandardColumn.Exception,
        StandardColumn.Properties
    },
    TimeStamp = { DataType = SqlDbType.DateTimeOffset }
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: sinkOptions,
        columnOptions: columnOptions,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await EnsureRolesAsync(roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Shareholder.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/api/shareholder/items", async (IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
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

        return Results.Ok(response);
    })
    .RequireAuthorization(new AuthorizeAttribute { Roles = ApplicationRoles.User });

app.MapPost("/api/shareholder/order", async (
        CreateRewardOrderRequestDto request,
        ClaimsPrincipal user,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
    {
        if (request.Items.Count == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Items"] = ["商品を1件以上選択してください。"]
            });
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var shareholder = await unitOfWork.Shareholders.GetByUserIdAsync(userId, cancellationToken);
        if (shareholder is null)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "株主情報が見つかりません。",
                Detail = "株主情報が未登録、または無効化されています。",
                Status = StatusCodes.Status404NotFound
            });
        }

        var duplicated = await unitOfWork.RewardOrders.ExistsByShareholderIdAsync(shareholder.ShareholderId, cancellationToken);
        if (duplicated)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Order"] = ["すでに申し込みが完了しています。再申し込みはできません。"]
            });
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
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Items"] = ["数量は1以上で入力してください。"]
                });
            }

            if (!itemMap.TryGetValue(requestedItem.ItemId, out var rewardItem))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Items"] = ["選択された商品が存在しないか、現在は申し込みできません。"]
                });
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
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Items"] = ["使用可能ポイントを超えています。商品と数量を見直してください。"]
            });
        }

        shareholder.PostalCode = request.PostalCode;
        shareholder.Address1 = request.Address1;
        shareholder.Address2 = request.Address2;
        shareholder.Address3 = request.Address3;
        shareholder.PhoneNumber = request.PhoneNumber;
        shareholder.UsedPoints += totalPoints;
        shareholder.UpdatedAt = DateTime.UtcNow;

        var rewardOrder = new RewardOrder
        {
            ShareholderId = shareholder.ShareholderId,
            TotalPoints = totalPoints,
            OrderedAt = DateTime.UtcNow,
            IsCancelled = false,
            IsExported = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        unitOfWork.Shareholders.Update(shareholder);
        unitOfWork.RewardOrders.Add(rewardOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(new CreateRewardOrderResponseDto(rewardOrder.OrderId));
    })
    .RequireAuthorization(new AuthorizeAttribute { Roles = ApplicationRoles.User });

try
{
    Log.Information("アプリケーションを起動します");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "アプリケーションの起動中に致命的なエラーが発生しました");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Admin/User ロールが存在しない場合に作成して、アプリに必要なロールを初期化する。
/// </summary>
static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
{
    foreach (var roleName in new[] { ApplicationRoles.Admin, ApplicationRoles.User })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
