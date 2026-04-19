using Microsoft.AspNetCore.Components.Forms;
using Shared.Dtos;
using User.Client.Services.Api.Exceptions;

namespace User.Client.Pages
{
    public partial class Confirm
    {
        private bool isAgreed;

        private bool CanSubmit => SelectionState.SelectedItems.Count > 0
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.PostalCode)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.Address1)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.Address2)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.PhoneNumber)
                                  && !SelectionState.IsExported
                                  && isAgreed;

        protected override async Task OnInitializedAsync()
        {
            await RunAsync(InitializedAsync, "初期処理に失敗しました。");
        }

        private Task InitializedAsync()
        {
            if (SelectionState.SelectedItems.Count == 0)
            {
                NavigationManager.NavigateTo("/items");
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(SelectionState.Address.PostalCode)
                || string.IsNullOrWhiteSpace(SelectionState.Address.Address1)
                || string.IsNullOrWhiteSpace(SelectionState.Address.Address2)
                || string.IsNullOrWhiteSpace(SelectionState.Address.PhoneNumber))
            {
                NavigationManager.NavigateTo("/address");
            }

            return Task.CompletedTask;
        }

        private async Task HandleMoveToAddressAsync()
        {
            await RunAsync(() =>
            {
                NavigationManager.NavigateTo("/address");
                return Task.CompletedTask;
            });
        }

        private async Task HandleSubmitOrderAsync()
        {
            await RunAsync(SubmitOrderAsync, "申し込み登録に失敗しました。");
        }

        private async Task SubmitOrderAsync()
        {
            if (!CanSubmit)
            {
                return;
            }

            _serverErrorMessage = null;

            var request = new CreateRewardOrderRequestDto(
                SelectionState.Address.PostalCode,
                SelectionState.Address.Address1,
                SelectionState.Address.Address2,
                SelectionState.Address.Address3,
                SelectionState.Address.PhoneNumber,
                SelectionState.SelectedItems.Select(x => new CreateRewardOrderItemRequestDto(x.ItemId, x.Quantity)).ToList());

            var result = await OrderApiClient.CreateAsync(request);
            var wasUpdate = SelectionState.HasExistingOrder;
            SelectionState.Clear();
            NavigationManager.NavigateTo($"/complete?orderId={result.OrderId}&updated={wasUpdate}");
            return;
        }
    }
}
