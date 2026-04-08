using System.Globalization;
using System.Text;
using Admin.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Entities;

namespace Admin.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class RewardOrdersController(IUnitOfWork unitOfWork, ILogger<RewardOrdersController> logger) : ControllerBase
{
    private const string ExportScopeAll = "all";
    private const string ExportScopeUnexported = "unexported";

    [HttpGet("export")]
    public async Task<IActionResult> ExportAsync([FromQuery] string scope = ExportScopeUnexported, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(scope, ExportScopeUnexported, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(scope, ExportScopeAll, StringComparison.OrdinalIgnoreCase))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["scope"] = ["出力対象が不正です。"]
            }));
        }

        var exportOnlyUnexported = string.Equals(scope, ExportScopeUnexported, StringComparison.OrdinalIgnoreCase);
        var orders = await unitOfWork.RewardOrders.ListForExportAsync(exportOnlyUnexported, cancellationToken);
        if (orders.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "出力対象がありません。",
                Detail = exportOnlyUnexported
                    ? "未出力の申込データがありません。"
                    : "出力対象の申込データがありません。",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var csv = BuildCsv(orders);
        if (exportOnlyUnexported)
        {
            var utcNow = DateTime.UtcNow;
            foreach (var order in orders)
            {
                order.IsExported = true;
                order.UpdatedAt = utcNow;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "申込CSVを出力しました。Scope: {Scope}, OrderCount: {OrderCount}, RowCount: {RowCount}",
            exportOnlyUnexported ? ExportScopeUnexported : ExportScopeAll,
            orders.Count,
            orders.Sum(static x => x.OrderItems.Count));

        var fileName = $"RewardOrders_Export_{DateTime.Now:yyyyMMdd}.csv";
        return File(new UTF8Encoding(true).GetBytes(csv), "text/csv; charset=utf-8", fileName);
    }

    private static string BuildCsv(IEnumerable<RewardOrder> orders)
    {
        var builder = new StringBuilder();
        builder.Append("OrderId,ShareholderNo,ShareholderName,PostalCode,Address1,Address2,Address3,PhoneNumber,ItemCode,ItemName,Quantity,RequiredPoints,SubtotalPoints,TotalOrderPoints,OrderedAt,Remarks\r\n");

        foreach (var order in orders)
        {
            if (order.Shareholder is null)
            {
                continue;
            }

            foreach (var orderItem in order.OrderItems.Where(x => x.Item is not null))
            {
                builder.Append(Escape(order.OrderId.ToString(CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(order.Shareholder.ShareholderNo)).Append(',');
                builder.Append(Escape(order.Shareholder.ShareholderName)).Append(',');
                builder.Append(Escape(order.PostalCode)).Append(',');
                builder.Append(Escape(order.Address1)).Append(',');
                builder.Append(Escape(order.Address2)).Append(',');
                builder.Append(Escape(order.Address3 ?? string.Empty)).Append(',');
                builder.Append(Escape(order.PhoneNumber ?? string.Empty)).Append(',');
                builder.Append(Escape(orderItem.Item!.ItemCode)).Append(',');
                builder.Append(Escape(orderItem.Item.ItemName)).Append(',');
                builder.Append(Escape(orderItem.Quantity.ToString(CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(orderItem.Item.RequiredPoints.ToString(CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(orderItem.SubtotalPoints.ToString(CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(order.TotalPoints.ToString(CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(order.OrderedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))).Append(',');
                builder.Append(Escape(string.Empty)).Append("\r\n");
            }
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
