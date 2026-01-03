using System.Net;
using Admin.Client.Components;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Entities;
using static MudBlazor.CategoryTypes;

namespace Admin.Client.Pages.RewardItems
{
    public partial class CreateRewardItem : FormComponentBase<CreateRewardItemInput>
    {
        [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        private RewardItem? _createdItem;

        private Task HandleValidSubmit() => RunAsync(ValidSubmit, "登録に失敗しました。もう一度実行してください");
        private Task HandleResetForm() => RunAsync(ResetForm);

        protected override async Task OnInitializedAsync()
        {
            await RunAsync(NewItem,"初期処理に失敗しました。");
        }

        private Task NewItem()
        {
            base.InitializeEditContext(new CreateRewardItemInput());
            return Task.CompletedTask;
        }

        private async Task ValidSubmit()
        {
            var created = await ApiClient.CreateAsync(_formModel);
            _createdItem = created;
            Snackbar.Add($"優待商品『{created.ItemName}』を登録しました。", Severity.Success);
            base.InitializeEditContext(new CreateRewardItemInput());
        }

        protected Task ResetForm()
        {
            return base.ResetForm(new CreateRewardItemInput());
        }
    }
}
