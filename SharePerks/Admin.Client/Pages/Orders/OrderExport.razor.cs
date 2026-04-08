using Admin.Client.Services;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Admin.Client.Pages.Orders;

public partial class OrderExport
{
    private const string ExportScopeAll = "all";
    private const string ExportScopeUnexported = "unexported";

    [Inject] public IRewardOrderExportApiClient ApiClient { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public OverlayState Overlay { get; set; } = default!;

    private string _selectedScope = ExportScopeUnexported;
    private string? _serverErrorMessage;

    private async Task HandleExportAsync()
    {
        if (!OperatingSystem.IsBrowser() || Overlay.Visible)
        {
            return;
        }

        try
        {
            Overlay.Show();
            _serverErrorMessage = null;

            var result = await ApiClient.ExportAsync(_selectedScope);
            var base64 = Convert.ToBase64String(result.Content);
            await JsRuntime.InvokeVoidAsync("sharePerks.downloadFileFromBase64", result.FileName, result.ContentType, base64);

            Snackbar.Add(
                _selectedScope == ExportScopeUnexported
                    ? "未出力の申込CSVを出力しました。"
                    : "申込CSVを全件出力しました。",
                Severity.Success);
        }
        catch (Exception ex)
        {
            _serverErrorMessage = ex.Message;
        }
        finally
        {
            Overlay.Hide();
            await InvokeAsync(StateHasChanged);
        }
    }
}
