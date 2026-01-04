using System.Security.Claims;
using System.Text;
using Admin.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Shared.Entities;

namespace Admin.Controllers;

[ApiController]
[Route("api/admin/shareholders")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class ShareholderImportsController : ControllerBase
{
    private static readonly string[] RequiredHeaders =
    [
        "ShareholderNo",
        "ShareholderName",
        "ShareholderNameKana",
        "PostalCode",
        "Address1",
        "Address2",
        "Address3",
        "PhoneNumber",
        "Holdings",
        "CourseCode",
        "GrantedPoints",
        "LoginId",
        "InitialPassword"
    ];

    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ShareholderImportsController> _logger;

    public ShareholderImportsController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<ShareholderImportsController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportBatch>> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(file), "CSVファイルを選択してください。");
            return ValidationProblem(ModelState);
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(file), "CSV形式のファイルを選択してください。");
            return ValidationProblem(ModelState);
        }

        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return Forbid();
        }

        _logger.LogInformation("株主CSVインポートを開始します (FileName: {FileName})", file.FileName);

        var now = DateTime.UtcNow;
        var successCount = 0;
        var errorCount = 0;
        var errorMessages = new List<string>();
        var processedShareholderNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var parser = new TextFieldParser(reader)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            ModelState.AddModelError(nameof(file), "CSVファイルが空です。");
            return ValidationProblem(ModelState);
        }

        var headers = parser.ReadFields();
        if (headers is null || headers.Length == 0)
        {
            ModelState.AddModelError(nameof(file), "CSVヘッダーの読み取りに失敗しました。");
            return ValidationProblem(ModelState);
        }

        headers[0] = headers[0].TrimStart('\uFEFF');
        var headerMap = headers
            .Select((name, index) => new { name = name.Trim(), index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        foreach (var requiredHeader in RequiredHeaders)
        {
            if (!headerMap.ContainsKey(requiredHeader))
            {
                ModelState.AddModelError(nameof(file), $"CSVヘッダーに {requiredHeader} がありません。");
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var rowNumber = 1;
        while (!parser.EndOfData)
        {
            rowNumber++;
            var fields = parser.ReadFields();
            if (fields is null)
            {
                continue;
            }

            if (fields.Length < headers.Length)
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: 列数が不足しています。");
                continue;
            }

            var shareholderNo = GetField(fields, headerMap, "ShareholderNo");
            var shareholderName = GetField(fields, headerMap, "ShareholderName");
            var shareholderNameKana = GetField(fields, headerMap, "ShareholderNameKana");
            var postalCode = GetField(fields, headerMap, "PostalCode");
            var address1 = GetField(fields, headerMap, "Address1");
            var address2 = GetField(fields, headerMap, "Address2");
            var address3 = GetField(fields, headerMap, "Address3");
            var phoneNumber = GetField(fields, headerMap, "PhoneNumber");
            var holdingsText = GetField(fields, headerMap, "Holdings");
            var courseCode = GetField(fields, headerMap, "CourseCode");
            var grantedPointsText = GetField(fields, headerMap, "GrantedPoints");
            var loginId = GetField(fields, headerMap, "LoginId");
            var initialPassword = GetField(fields, headerMap, "InitialPassword");

            if (!ValidateRequired(shareholderNo, shareholderName, postalCode, address1, address2, holdingsText, grantedPointsText, loginId, initialPassword))
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: 必須項目が不足しています。");
                continue;
            }

            if (!int.TryParse(holdingsText, out var holdings))
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: Holdings が数値ではありません。");
                continue;
            }

            if (!int.TryParse(grantedPointsText, out var grantedPoints))
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: GrantedPoints が数値ではありません。");
                continue;
            }

            if (!processedShareholderNos.Add(shareholderNo))
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: ShareholderNo が重複しています。");
                continue;
            }

            if (await _unitOfWork.Shareholders.ExistsByShareholderNoAsync(shareholderNo, cancellationToken))
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: ShareholderNo が既に登録されています。");
                continue;
            }

            if (await _userManager.FindByNameAsync(loginId) is not null)
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: LoginId が既に登録されています。");
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = loginId,
                Email = $"{shareholderNo}@example.local",
                PhoneNumber = phoneNumber
            };

            var createResult = await _userManager.CreateAsync(user, initialPassword);
            if (!createResult.Succeeded)
            {
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: ユーザー作成に失敗しました。");
                continue;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.User);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                errorCount++;
                errorMessages.Add($"{rowNumber}行目: ロール付与に失敗しました。");
                continue;
            }

            var shareholder = new Shareholder
            {
                UserId = user.Id,
                ShareholderNo = shareholderNo,
                ShareholderName = shareholderName,
                ShareholderNameKana = shareholderNameKana,
                PostalCode = postalCode,
                Address1 = address1,
                Address2 = address2,
                Address3 = address3,
                PhoneNumber = phoneNumber,
                Holdings = holdings,
                CourseCode = courseCode,
                GrantedPoints = grantedPoints,
                UsedPoints = 0,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Shareholders.AddAsync(shareholder, cancellationToken);
            successCount++;
        }

        var importBatch = new ImportBatch
        {
            ExecutedAt = now,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            FileName = file.FileName,
            AdminUserId = adminUserId,
            Remarks = errorMessages.Count == 0 ? null : string.Join(Environment.NewLine, errorMessages.Take(20))
        };

        await _unitOfWork.ImportBatches.AddAsync(importBatch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "株主CSVインポートが完了しました (FileName: {FileName}, Success: {SuccessCount}, Error: {ErrorCount})",
            file.FileName,
            successCount,
            errorCount);

        return Ok(importBatch);
    }

    private static string GetField(string[] fields, IReadOnlyDictionary<string, int> headerMap, string name)
    {
        return headerMap.TryGetValue(name, out var index) && index < fields.Length
            ? fields[index].Trim()
            : string.Empty;
    }

    private static bool ValidateRequired(params string[] values)
    {
        return values.All(value => !string.IsNullOrWhiteSpace(value));
    }
}
