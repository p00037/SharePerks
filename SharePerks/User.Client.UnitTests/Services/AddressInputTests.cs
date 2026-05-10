using System.ComponentModel.DataAnnotations;
using User.Client.Services;

namespace User.Client.UnitTests.Services;

[TestClass]
public sealed class AddressInputTests
{
    [TestMethod]
    public void Validate_WhenRequiredFieldsAreEmpty_ReturnsValidationErrors()
    {
        var input = new AddressInput();
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            input,
            new ValidationContext(input),
            results,
            validateAllProperties: true);

        Assert.IsFalse(isValid);
        CollectionAssert.AreEquivalent(
            new[] { nameof(AddressInput.PostalCode), nameof(AddressInput.PhoneNumber), nameof(AddressInput.Address1), nameof(AddressInput.Address2) },
            results.SelectMany(result => result.MemberNames).ToArray());
    }
}
