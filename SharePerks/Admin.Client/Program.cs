using System.Net.Http;
using Admin.Client.Services;
using Admin.Client.Services.Api;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Apiサービス
builder.Services.AddScoped<IRewardItemApiClient, RewardItemApiClient>();

//Overlayサービス
builder.Services.AddScoped<OverlayState>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
