using Microsoft.AspNetCore.Components.Forms;
using Shared.Dtos;
using User.Client.Services.Api.Exceptions;

namespace User.Client.Pages
{
    public partial class Confirm
    {
        private bool isAgreed;
        private bool isSubmitting;

        private bool CanSubmit => SelectionState.SelectedItems.Count > 0
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.PostalCode)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.Address1)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.Address2)
                                  && !string.IsNullOrWhiteSpace(SelectionState.Address.PhoneNumber)
                                  && isAgreed;

        protected override void OnInitialized()
        {
            if (SelectionState.SelectedItems.Count == 0)
            {
                NavigationManager.NavigateTo("/items");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectionState.Address.PostalCode)
                || string.IsNullOrWhiteSpace(SelectionState.Address.Address1)
                || string.IsNullOrWhiteSpace(SelectionState.Address.Address2)
                || string.IsNullOrWhiteSpace(SelectionState.Address.PhoneNumber))
            {
                NavigationManager.NavigateTo("/address");
            }
        }

        private void MoveToAddress()
        {
            NavigationManager.NavigateTo("/address");
        }

        private async Task SubmitOrderAsync()
        {
            await RunAsync(async () =>
            {
                if (!CanSubmit)
                {
                    return;
                }

                isSubmitting = true;
                _serverErrorMessage = null;

                var request = new CreateRewardOrderRequestDto(
                    SelectionState.Address.PostalCode,
                    SelectionState.Address.Address1,
                    SelectionState.Address.Address2,
                    SelectionState.Address.Address3,
                    SelectionState.Address.PhoneNumber,
                    SelectionState.SelectedItems.Select(x => new CreateRewardOrderItemRequestDto(x.ItemId, x.Quantity)).ToList());

                var result = await OrderApiClient.CreateAsync(request);
                SelectionState.Clear();
                NavigationManager.NavigateTo($"/complete?orderId={result.OrderId}");
                return;
            }, "申し込み登録に失敗しました。");
        }
    }
}
