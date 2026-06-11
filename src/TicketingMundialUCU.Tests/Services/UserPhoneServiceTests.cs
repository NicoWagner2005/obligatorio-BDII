using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class UserPhoneServiceTests
{
    private readonly IUserPhoneRepository _repository =
        Substitute.For<IUserPhoneRepository>();
    private readonly UserPhoneService _service;

    public UserPhoneServiceTests()
    {
        _service = new UserPhoneService(_repository);
    }

    [Fact]
    public async Task GetPhoneNumbers_delegates_to_repository()
    {
        var expected = new[] { "+598 91 234 567", "2900 0000" };
        _repository.GetByUserIdAsync("user-1").Returns(expected);

        var result = await _service.GetPhoneNumbersAsync("user-1");

        Assert.Equal(expected, result);
        await _repository.Received(1).GetByUserIdAsync("user-1");
    }

    [Fact]
    public async Task UpdatePhoneNumbers_normalizes_and_removes_duplicates()
    {
        var phoneNumbers = new string?[]
        {
            " +598 91 234 567 ",
            "",
            null,
            "+598 91 234 567",
            "2900 0000"
        };

        await _service.UpdatePhoneNumbersAsync("user-1", phoneNumbers);

        await _repository.Received(1).ReplaceAllAsync(
            "user-1",
            Arg.Is<IEnumerable<string?>>(saved =>
                saved.SequenceEqual(new[] { "+598 91 234 567", "2900 0000" })));
    }

    [Fact]
    public async Task UpdatePhoneNumbers_with_too_long_number_rejects_update()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdatePhoneNumbersAsync("user-1", [new string('1', 21)]));

        Assert.Equal(
            "The phone number cannot exceed 20 characters.",
            exception.Message);
        await _repository.DidNotReceive().ReplaceAllAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string?>>());
    }

    [Fact]
    public async Task UpdatePhoneNumbers_with_invalid_number_rejects_update()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdatePhoneNumbersAsync("user-1", ["not-a-phone"]));

        Assert.Equal("The phone number is not valid.", exception.Message);
        await _repository.DidNotReceive().ReplaceAllAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string?>>());
    }
}
