using System.Globalization;
using System.Net.Http.Headers;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Entities;

namespace Admin.Client.Services.Api;

public class RewardItemApiClient: ApiClientBase, IRewardItemApiClient
{
    private const long MaxImageSize = 5 * 1024 * 1024;

    public RewardItemApiClient(HttpClient httpClient):base(httpClient)
    {
    }

    public Task<List<RewardItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<RewardItem>>(
            "api/admin/items",
            failedMessage: "優待商品一覧の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public Task<RewardItem> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetAsync<RewardItem>(
            $"api/admin/items/{id}",
            failedMessage: "優待商品の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public async Task<RewardItem> CreateAsync(
        CreateRewardItemInput input,
        IBrowserFile? imageFile = null,
        CancellationToken cancellationToken = default)
    {
        using var content = BuildMultipartContent(input, imageFile);
        return await PostMultipartAsync<RewardItem>(
            "api/admin/items",
            content,
            validationMessage: "入力内容を確認してください。",
            failedMessage: "優待商品の登録に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public async Task<RewardItem> UpdateAsync(
        int id,
        CreateRewardItemInput input,
        IBrowserFile? imageFile = null,
        CancellationToken cancellationToken = default)
    {
        using var content = BuildMultipartContent(input, imageFile);
        return await PutMultipartAsync<RewardItem>(
            $"api/admin/items/{id}",
            content,
            validationMessage: "入力内容を確認してください。",
            failedMessage: "優待商品の更新に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(
            $"api/admin/items/{id}",
            failedMessage: "優待商品の削除に失敗しました。",
            cancellationToken: cancellationToken);
    }

    private static MultipartFormDataContent BuildMultipartContent(CreateRewardItemInput input, IBrowserFile? imageFile)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(input.ItemCode), nameof(input.ItemCode));
        content.Add(new StringContent(input.ItemName), nameof(input.ItemName));
        content.Add(new StringContent(input.ItemDescription ?? string.Empty), nameof(input.ItemDescription));
        content.Add(new StringContent(input.RequiredPoints.ToString(CultureInfo.InvariantCulture)), nameof(input.RequiredPoints));
        content.Add(new StringContent(input.DisplayOrder.ToString(CultureInfo.InvariantCulture)), nameof(input.DisplayOrder));
        content.Add(new StringContent(input.IsActive.ToString()), nameof(input.IsActive));

        if (imageFile is not null)
        {
            var stream = imageFile.OpenReadStream(MaxImageSize);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
            content.Add(fileContent, "ImageFile", imageFile.Name);
        }

        return content;
    }
}
