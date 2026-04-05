using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using User.Client.Services;
using User.Client.Services.Api;
using User.Client.Services.Api.Interface;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddMudServices();

// Overlayサービス
builder.Services.AddScoped<OverlayState>();

// Apiサービス
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<RewardSelectionState>();
builder.Services.AddScoped<IShareholderOrderApiClient, ShareholderOrderApiClient>();
builder.Services.AddScoped<IShareholderProfileApiClient, ShareholderProfileApiClient>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
