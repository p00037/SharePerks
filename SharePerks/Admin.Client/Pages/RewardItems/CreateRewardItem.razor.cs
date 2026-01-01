using Admin.Client.Components;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Entities;
using System.Net;

namespace Admin.Client.Pages.RewardItems
{
    public partial class CreateRewardItem : FormComponentBase<CreateRewardItemInput>
    {
        [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public int? ItemId { get; set; }

        private RewardItem? _createdItem;
        private RewardItem? _loadedItem;
        private bool _isEdit => ItemId.HasValue;

        private Task HandleValidSubmit() => RunAsync(
            ValidSubmit,
            _isEdit ? "更新に失敗しました。もう一度実行してください" : "登録に失敗しました。もう一度実行してください");
        private Task HandleResetForm() => RunAsync(ResetForm);

        protected override async Task OnInitializedAsync()
        {
            await Run(() =>
            {
                InitializeEditContext(new CreateRewardItemInput());
            });

            if (_isEdit)
            {
                await RunAsync(LoadItem, "優待商品の読み込みに失敗しました。");
            }
        }

        private async Task ValidSubmit()
        {
            if (_isEdit && ItemId.HasValue)
            {
                var updated = await ApiClient.UpdateAsync(ItemId.Value, _formModel);
                _createdItem = updated;
                _loadedItem = updated;
                Snackbar.Add($"優待商品『{updated.ItemName}』を更新しました。", Severity.Success);
                base.InitializeEditContext(ToInputModel(updated));
                return;
            }

            var created = await ApiClient.CreateAsync(_formModel);
            _createdItem = created;
            Snackbar.Add($"優待商品『{created.ItemName}』を登録しました。", Severity.Success);
            base.InitializeEditContext(new CreateRewardItemInput());
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

        protected override Task ResetForm()
        {
            if (_isEdit && _loadedItem is not null)
            {
                InitializeEditContext(ToInputModel(_loadedItem));
                _serverErrorMessage = null;
                return Task.CompletedTask;
            }

            return base.ResetForm();
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
