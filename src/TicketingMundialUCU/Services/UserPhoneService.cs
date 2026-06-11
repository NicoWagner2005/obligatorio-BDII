using System.ComponentModel.DataAnnotations;
using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public sealed class UserPhoneService(IUserPhoneRepository repository)
{
    private const int MaxPhoneNumberLength = 20;

    public Task<IEnumerable<string>> GetPhoneNumbersAsync(string userId) =>
        repository.GetByUserIdAsync(userId);

    public async Task UpdatePhoneNumbersAsync(
        string userId,
        IEnumerable<string?> phoneNumbers)
    {
        ArgumentNullException.ThrowIfNull(phoneNumbers);

        var normalizedPhoneNumbers = phoneNumbers
            .Where(phoneNumber => !string.IsNullOrWhiteSpace(phoneNumber))
            .Select(phoneNumber => phoneNumber!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Validate(normalizedPhoneNumbers);

        await repository.ReplaceAllAsync(userId, normalizedPhoneNumbers);
    }

    private static void Validate(IEnumerable<string> phoneNumbers)
    {
        var phoneAttribute = new PhoneAttribute();

        foreach (var phoneNumber in phoneNumbers)
        {
            if (phoneNumber.Length > MaxPhoneNumberLength)
            {
                throw new InvalidOperationException(
                    "The phone number cannot exceed 20 characters.");
            }

            if (!phoneAttribute.IsValid(phoneNumber))
            {
                throw new InvalidOperationException("The phone number is not valid.");
            }
        }
    }
}
