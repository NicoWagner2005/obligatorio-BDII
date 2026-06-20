using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class UserPhoneServiceTests
{
    private readonly IUserPhoneDao _dao =
        Substitute.For<IUserPhoneDao>();
    private readonly UserPhoneService _service;

    public UserPhoneServiceTests()
    {
        _service = new UserPhoneService(_dao);
    }

    [Fact]
    public async Task GetPhoneNumbers_delegates_to_dao()
    {
        var expected = new[] { "+598 91 234 567", "2900 0000" };
        _dao.GetByUserIdAsync("user-1").Returns(expected);

        var result = await _service.GetPhoneNumbersAsync("user-1");

        Assert.Equal(expected, result);
        await _dao.Received(1).GetByUserIdAsync("user-1");
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

        await _dao.Received(1).ReplaceAllAsync(
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
        await _dao.DidNotReceive().ReplaceAllAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string?>>());
    }

    [Fact]
    public async Task UpdatePhoneNumbers_with_invalid_number_rejects_update()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdatePhoneNumbersAsync("user-1", ["not-a-phone"]));

        Assert.Equal("The phone number is not valid.", exception.Message);
        await _dao.DidNotReceive().ReplaceAllAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string?>>());
    }
}
