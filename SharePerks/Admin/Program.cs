using System.Collections.ObjectModel;
using System.Data;
using Admin.Client.Pages;
using Admin.Client.Services;
using Admin.Client.Services.Api;
using Admin.Client.Services.Api.Interface;
using Admin.Components;
using Admin.Components.Account;
using Admin.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

//Overlayサービス
builder.Services.AddScoped<OverlayState>();

builder.Services.AddScoped(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (httpContext is null)
        throw new InvalidOperationException("No active HttpContext.");

    var req = httpContext.Request;
    var baseUri = $"{req.Scheme}://{req.Host}/";
    return new HttpClient { BaseAddress = new Uri(baseUri) };
});

//Apiサービス
builder.Services.AddScoped<IRewardItemApiClient, RewardItemApiClient>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Serilog 構成
var sinkOptions = new MSSqlServerSinkOptions
{
    TableName = "LogsAdmin",
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
builder.Services.AddControllers();

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

app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Admin.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

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
