using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using User.Data;

namespace User.Controllers;

[ApiController]
[Route("api/shareholder/profile")]
[Authorize(Roles = ApplicationRoles.User)]
public sealed class ShareholderProfileController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("address")]
    public async Task<ActionResult<ShareholderAddressDto>> GetAddressAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var shareholder = await unitOfWork.Shareholders.GetByUserIdAsync(userId, cancellationToken);
        if (shareholder is null)
        {
            return NotFound();
        }

        return Ok(new ShareholderAddressDto(
            shareholder.PostalCode,
            shareholder.Address1,
            shareholder.Address2,
            shareholder.Address3,
            shareholder.PhoneNumber));
    }
}
