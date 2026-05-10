using User.Client.Components;
using User.Client.Services;
using User.Client.Services.Api.Interface;

namespace User.Client.Pages;

public partial class Address : FormComponentBase<AddressInput>
{
    protected override async Task OnInitializedAsync()
    {
        InitializeEditContext(new AddressInput());
        await RunAsync(InitializeAddressAsync, "送付先情報の取得に失敗しました。時間をおいて再度お試しください。");
    }

    private async Task InitializeAddressAsync()
    {
        var addressInput = await ResolveInitialAddressInputAsync();
        await ResetForm(addressInput);
    }

    private async Task<AddressInput> ResolveInitialAddressInputAsync()
    {
        var existingOrderAddress = await TryLoadExistingOrderAddressAsync();
        if (existingOrderAddress is not null)
        {
            return existingOrderAddress;
        }

        if (HasAddress(SelectionState.Address))
        {
            return CloneAddress(SelectionState.Address);
        }

        try
        {
            return await LoadProfileAddressAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return CloneAddress(SelectionState.Address);
        }
    }

    private async Task<AddressInput?> TryLoadExistingOrderAddressAsync()
    {
        if (SelectionState.HasLoadedExistingOrder)
        {
            return null;
        }

        try
        {
            var currentOrder = await ShareholderOrderApiClient.GetCurrentAsync();
            if (currentOrder is null)
            {
                return null;
            }

            SelectionState.SetExistingOrder(currentOrder.OrderId, currentOrder.IsExported);
            SelectionState.SetSelection(currentOrder.Items.Select(item => new SelectedRewardItem(
                item.ItemId,
                item.ItemName,
                item.ImagePath,
                item.RequiredPoints,
                item.Quantity)));

            var addressInput = new AddressInput
            {
                PostalCode = currentOrder.PostalCode,
                PhoneNumber = currentOrder.PhoneNumber ?? string.Empty,
                Address1 = currentOrder.Address1,
                Address2 = currentOrder.Address2,
                Address3 = currentOrder.Address3
            };

            SelectionState.SetAddress(CloneAddress(addressInput));
            SelectionState.MarkOrderLoaded();
            return CloneAddress(addressInput);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<AddressInput> LoadProfileAddressAsync()
    {
        var shareholderAddress = await ShareholderProfileApiClient.GetAddressAsync();
        var addressInput = new AddressInput
        {
            PostalCode = shareholderAddress.PostalCode,
            PhoneNumber = shareholderAddress.PhoneNumber ?? string.Empty,
            Address1 = shareholderAddress.Address1,
            Address2 = shareholderAddress.Address2,
            Address3 = shareholderAddress.Address3
        };

        SelectionState.SetAddress(CloneAddress(addressInput));
        return addressInput;
    }

    private void MoveBack()
    {
        NavigationManager.NavigateTo("/items");
    }

    private async Task HandleMoveToConfirmAsync()
    {
        await RunAsync(MoveToConfirmAsync);
    }

    private Task MoveToConfirmAsync()
    {
        SelectionState.SetAddress(CloneAddress(_formModel));
        NavigationManager.NavigateTo("/confirm");
        return Task.CompletedTask;
    }

    private static bool HasAddress(AddressInput address)
    {
        return !string.IsNullOrWhiteSpace(address.PostalCode)
            || !string.IsNullOrWhiteSpace(address.PhoneNumber)
            || !string.IsNullOrWhiteSpace(address.Address1)
            || !string.IsNullOrWhiteSpace(address.Address2)
            || !string.IsNullOrWhiteSpace(address.Address3);
    }

    private static AddressInput CloneAddress(AddressInput address)
    {
        return new AddressInput
        {
            PostalCode = address.PostalCode,
            PhoneNumber = address.PhoneNumber,
            Address1 = address.Address1,
            Address2 = address.Address2,
            Address3 = address.Address3
        };
    }
}
