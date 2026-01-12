using Admin.Client.Components;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems
{
    public partial class EditRewardItem : FormComponentBase<CreateRewardItemInput>
    {
        [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public int? ItemId { get; set; }

        private RewardItem? _createdItem;
        private RewardItem? _loadedItem;
        private IBrowserFile? _imageFile;
        private string? _selectedImageName;

        private Task HandleValidSubmit() => RunAsync(ValidSubmit, "更新に失敗しました。もう一度実行してください");
        private Task HandleResetForm() => RunAsync(ResetForm);

        protected override async Task OnInitializedAsync()
        {
            await RunAsync(LoadItem, "初期処理に失敗しました。");
        }

        private async Task LoadItem()
        {
            if (!ItemId.HasValue)
            {
                return;
            }

            var item = await ApiClient.GetByIdAsync(ItemId.Value);
            _loadedItem = item;
            base.InitializeEditContext(ToInputModel(item));
        }

        private async Task ValidSubmit()
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

        private Task ResetForm()
        {
            if (_loadedItem == null)
            {
                return Task.CompletedTask;
            }

            ResetImageSelection();
            return base.ResetForm(ToInputModel(_loadedItem));
        }

        private void HandleImageFileChange(InputFileChangeEventArgs args)
        {
            _imageFile = args.File;
            _selectedImageName = _imageFile?.Name;
        }

        private void ResetImageSelection()
        {
            _imageFile = null;
            _selectedImageName = null;
        }

        private static CreateRewardItemInput ToInputModel(RewardItem item)
        {
            return new CreateRewardItemInput
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
