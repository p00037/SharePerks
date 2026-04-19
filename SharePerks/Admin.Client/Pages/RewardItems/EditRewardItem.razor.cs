using Admin.Client.Components;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems
{
    public partial class EditRewardItem : FormComponentBase<RewardItemInput>
    {
        [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public int? ItemId { get; set; }

        private RewardItem? _createdItem;
        private RewardItem? _loadedItem;
        private IBrowserFile? _imageFile;
        private string? _selectedImageName;

        protected override async Task OnInitializedAsync()
        {
            await RunAsync(LoadItemAsync, "初期処理に失敗しました。");
        }

        private async Task LoadItemAsync()
        {
            if (!ItemId.HasValue)
            {
                return;
            }

            var item = await ApiClient.GetByIdAsync(ItemId.Value);
            _loadedItem = item;
            base.InitializeEditContext(ToInputModel(item));
        }

        private async Task HandleValidSubmitAsync() => await RunAsync(ValidSubmitAsync, "更新に失敗しました。もう一度実行してください");

        private async Task ValidSubmitAsync()
        {
            if (!ItemId.HasValue)
            {
                return;
            }

            var updated = await ApiClient.UpdateAsync(ItemId.Value, _formModel, _imageFile);
            _createdItem = updated;
            _loadedItem = updated;
            Snackbar.Add($"優待商品『{updated.ItemName}』を更新しました。", Severity.Success);
            base.InitializeEditContext(ToInputModel(updated));
            ResetImageSelection();
            return;
        }

        private async Task HandleResetFormAsync() => await RunAsync(ResetForm);

        private Task ResetForm()
        {
            if (_loadedItem == null)
            {
                return Task.CompletedTask;
            }

            ResetImageSelection();
            return base.ResetForm(ToInputModel(_loadedItem));
        }

        private async Task HandleImageFileChange(InputFileChangeEventArgs args)
        {
            await RunAsync(() =>
            {
                _imageFile = args.File;
                _selectedImageName = _imageFile?.Name;
                return Task.CompletedTask;
            });
        }

        private void ResetImageSelection()
        {
            _imageFile = null;
            _selectedImageName = null;
        }

        private static RewardItemInput ToInputModel(RewardItem item)
        {
            return new RewardItemInput
            {
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemDescription = item.ItemDescription,
                RequiredPoints = item.RequiredPoints,
                DisplayOrder = item.DisplayOrder,
                IsActive = item.IsActive
            };
        }
    }
}
