using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Shareholder.Client.Services;
using Shareholder.Client.Services.Api;
using Shareholder.Client.Services.Api.Interface;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<RewardSelectionState>();
builder.Services.AddScoped<IShareholderOrderApiClient, ShareholderOrderApiClient>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
