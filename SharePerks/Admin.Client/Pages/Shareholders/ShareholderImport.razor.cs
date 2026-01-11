using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Shared.Entities;

namespace Admin.Client.Pages.Shareholders;

public partial class ShareholderImport
{
    private const long MaxFileSize = 20 * 1024 * 1024;
    private const string MaxFileSizeDescription = "20MB";

    [Inject] public IShareholderImportApiClient ApiClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private ImportBatch? _importResult;
    private string? _selectedFileName;
    private long? _selectedFileSize;
    private int _fileInputKey;

    protected override async Task OnInitializedAsync()
    {
        await RunAsync(() =>
        {
            InitializeEditContext(new ShareholderImportInput());
            return Task.CompletedTask;
        }, "初期化に失敗しました。");
    }

    private async Task HandleFileChanged(InputFileChangeEventArgs args)
    {
        await RunAsync(() =>
        {
            _serverErrorMessage = null;
            _importResult = null;

            var file = args.File;
            if (!file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _serverErrorMessage = "CSVファイルを選択してください。";
                ClearSelectedFile();
                return Task.CompletedTask;
            }

            if (file.Size > MaxFileSize)
            {
                _serverErrorMessage = $"ファイルサイズが上限({MaxFileSizeDescription})を超えています。";
                ClearSelectedFile();
                return Task.CompletedTask;
            }

            _formModel.File = file;
            _selectedFileName = file.Name;
            _selectedFileSize = file.Size;
            _editContext?.NotifyFieldChanged(new FieldIdentifier(_formModel, nameof(ShareholderImportInput.File)));
            return Task.CompletedTask;
        }, "ファイル選択時にエラーが発生しました");

    }

    private async Task HandleValidSubmit()
    {
        await RunAsync(async () =>
        {
            if (_formModel.File is null)
            {
                _serverErrorMessage = "CSVファイルを選択してください。";
                return;
            }

            _importResult = await ApiClient.ImportAsync(_formModel.File);
            Snackbar.Add("株主CSVインポートが完了しました。", Severity.Success);
        }, "株主CSVインポートに失敗しました。");
    }

    private async Task HandleResetForm()
    {
        await RunAsync(async () =>
        {
            _importResult = null;
            _serverErrorMessage = null;
            ClearSelectedFile();
            await ResetForm(new ShareholderImportInput());
        }, "クリア処理が失敗しました。");
    }

    private void ClearSelectedFile()
    {
        _formModel.File = null;
        _selectedFileName = null;
        _selectedFileSize = null;
        _fileInputKey++;
    }

    private static string FormatFileSize(long? size)
    {
        if (size is null)
        {
            return "0B";
        }

        if (size < 1024)
        {
            return $"{size}B";
        }

        if (size < 1024 * 1024)
        {
            return $"{size / 1024d:0.0}KB";
        }

        return $"{size / 1024d / 1024d:0.0}MB";
    }
}
