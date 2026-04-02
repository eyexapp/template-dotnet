using FluentAssertions;
using MyApp.Domain.ValueObjects;

namespace MyApp.UnitTests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("test.user@domain.org")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        var result = Email.Create(email);
        result.Value.Should().Be(email.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrow(string? email)
    {
        var act = () => Email.Create(email!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@missing.com")]
    [InlineData("missing@")]
    public void Create_WithInvalidFormat_ShouldThrow(string email)
    {
        var act = () => Email.Create(email);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid email*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("USER@EXAMPLE.COM");
        email1.Should().Be(email2);
    }
}
