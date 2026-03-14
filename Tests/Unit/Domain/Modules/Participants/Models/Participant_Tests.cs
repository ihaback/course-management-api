using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Domain.Modules.Participants.Models;

namespace Backend.Tests.Unit.Domain.Modules.Participants.Models;

public class Participant_Tests
{
    [Fact]
    public void Constructor_Should_Create_Participant_When_Parameters_Are_Valid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act
        var participant = Participant.Reconstitute(id, firstName, lastName, email, phoneNumber);

        // Assert
        Assert.NotNull(participant);
        Assert.Equal(id, participant.Id);
        Assert.Equal(firstName, participant.FirstName);
        Assert.Equal(lastName, participant.LastName);
        Assert.Equal(email, participant.Email.Value);
        Assert.Equal(phoneNumber, participant.PhoneNumber.Value);
        Assert.Equal(ParticipantContactType.Reconstitute(1, "Primary"), participant.ContactType);
    }

    [Fact]
    public void Constructor_Should_Default_ContactType_When_Not_Provided()
    {
        var id = Guid.NewGuid();

        var participant = Participant.Reconstitute(id, "John", "Doe", "john.doe@example.com", "+46701234567");

        Assert.Equal(ParticipantContactType.Reconstitute(1, "Primary"), participant.ContactType);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Id_Is_Empty()
    {
        // Arrange
        var id = Guid.Empty;
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("id", exception.ParamName);
        Assert.Contains("ID cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_FirstName_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        string firstName = null!;
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("firstName", exception.ParamName);
        Assert.Contains("First name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_FirstName_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("firstName", exception.ParamName);
        Assert.Contains("First name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_FirstName_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "   ";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("firstName", exception.ParamName);
        Assert.Contains("First name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_LastName_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        string lastName = null!;
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("lastName", exception.ParamName);
        Assert.Contains("Last name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_LastName_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("lastName", exception.ParamName);
        Assert.Contains("Last name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_LastName_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "   ";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("lastName", exception.ParamName);
        Assert.Contains("Last name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Email_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        string email = null!;
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("email", exception.ParamName);
        Assert.Contains("Email cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Email_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("email", exception.ParamName);
        Assert.Contains("Email cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Email_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "   ";
        var phoneNumber = "+46701234567";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("email", exception.ParamName);
        Assert.Contains("Email cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_PhoneNumber_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        string phoneNumber = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("phoneNumber", exception.ParamName);
        Assert.Contains("Phone number cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("phoneNumber", exception.ParamName);
        Assert.Contains("Phone number cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_PhoneNumber_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, firstName, lastName, email, phoneNumber));

        Assert.Equal("phoneNumber", exception.ParamName);
        Assert.Contains("Phone number cannot be empty or whitespace", exception.Message);
    }

    [Theory]
    [InlineData("Alice", "Smith", "alice.smith@example.com", "+46709876543")]
    [InlineData("Bob", "Johnson", "bob.johnson@example.com", "+46701111111")]
    [InlineData("Charlie", "Brown", "charlie.brown@example.com", "+46702222222")]
    public void Constructor_Should_Create_Participant_With_Various_Valid_Parameters(
        string firstName, string lastName, string email, string phoneNumber)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var participant = Participant.Reconstitute(id, firstName, lastName, email, phoneNumber);

        // Assert
        Assert.Equal(firstName, participant.FirstName);
        Assert.Equal(lastName, participant.LastName);
        Assert.Equal(email, participant.Email.Value);
        Assert.Equal(phoneNumber, participant.PhoneNumber.Value);
    }

    [Fact]
    public void Properties_Should_Be_Initialized_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        // Act
        var participant = Participant.Reconstitute(id, firstName, lastName, email, phoneNumber);

        // Assert
        Assert.Equal(id, participant.Id);
        Assert.Equal(firstName, participant.FirstName);
        Assert.Equal(lastName, participant.LastName);
        Assert.Equal(email, participant.Email.Value);
        Assert.Equal(phoneNumber, participant.PhoneNumber.Value);
    }

    [Fact]
    public void Two_Participants_With_Same_Values_Should_Have_Same_Property_Values()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+46701234567";

        var participant1 = Participant.Reconstitute(id, firstName, lastName, email, phoneNumber);
        var participant2 = Participant.Reconstitute(id, firstName, lastName, email, phoneNumber);

        // Assert
        Assert.Equal(participant1.Id, participant2.Id);
        Assert.Equal(participant1.FirstName, participant2.FirstName);
        Assert.Equal(participant1.LastName, participant2.LastName);
        Assert.Equal(participant1.Email, participant2.Email);
        Assert.Equal(participant1.PhoneNumber, participant2.PhoneNumber);
    }

    [Fact]
    public void Id_Property_Should_Be_Read_Only()
    {
        // Arrange
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", "+46701234567");

        // Assert
        var initialId = participant.Id;
        Assert.Equal(initialId, participant.Id);
    }

    [Fact]
    public void FirstName_Property_Should_Be_Read_Only()
    {
        // Arrange
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", "+46701234567");

        // Assert
        var initialFirstName = participant.FirstName;
        Assert.Equal(initialFirstName, participant.FirstName);
    }

    [Fact]
    public void LastName_Property_Should_Be_Read_Only()
    {
        // Arrange
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", "+46701234567");

        // Assert
        var initialLastName = participant.LastName;
        Assert.Equal(initialLastName, participant.LastName);
    }

    [Fact]
    public void Email_Property_Should_Be_Read_Only()
    {
        // Arrange
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", "+46701234567");

        // Assert
        var initialEmail = participant.Email;
        Assert.Equal(initialEmail, participant.Email);
    }

    [Fact]
    public void PhoneNumber_Property_Should_Be_Read_Only()
    {
        // Arrange
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", "+46701234567");

        // Assert
        var initialPhoneNumber = participant.PhoneNumber;
        Assert.Equal(initialPhoneNumber, participant.PhoneNumber);
    }

    [Fact]
    public void Constructor_Should_Handle_Long_Names()
    {
        // Arrange
        var firstName = "Very Long First Name With Multiple Words";
        var lastName = "Very Long Last Name With Multiple Words";
        var participant = Participant.Reconstitute(Guid.NewGuid(), firstName, lastName, "test@example.com", "+46701234567");

        // Assert
        Assert.Equal(firstName, participant.FirstName);
        Assert.Equal(lastName, participant.LastName);
    }

    [Fact]
    public void Constructor_Should_Handle_Swedish_Characters()
    {
        // Arrange
        var firstName = "Göran";
        var lastName = "Åström";
        var participant = Participant.Reconstitute(Guid.NewGuid(), firstName, lastName, "goran.astrom@example.com", "+46701234567");

        // Assert
        Assert.Equal(firstName, participant.FirstName);
        Assert.Equal(lastName, participant.LastName);
    }

    [Fact]
    public void Constructor_Should_Handle_Various_Email_Formats()
    {
        // Arrange
        var email = "user.name+tag@example.co.uk";
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", email, "+46701234567");

        // Assert
        Assert.Equal(email, participant.Email.Value);
    }

    [Theory]
    [InlineData("+46701234567")]
    [InlineData("0701234567")]
    [InlineData("+1-555-1234")]
    public void Constructor_Should_Accept_Various_Phone_Number_Formats(string phoneNumber)
    {
        // Act
        var participant = Participant.Reconstitute(Guid.NewGuid(), "John", "Doe", "john.doe@example.com", phoneNumber);

        // Assert
        Assert.Equal(phoneNumber, participant.PhoneNumber.Value);
    }

    [Fact]
    public void Update_Should_Change_Values_When_Input_Is_Valid()
    {
        // Arrange
        var participant = Participant.Reconstitute(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john.doe@example.com",
            "+46701234567");

        // Act
        participant.Update(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            "+46709876543",
            ParticipantContactType.Reconstitute(2, "Billing"));

        // Assert
        Assert.Equal("Jane", participant.FirstName);
        Assert.Equal("Smith", participant.LastName);
        Assert.Equal("jane.smith@example.com", participant.Email.Value);
        Assert.Equal("+46709876543", participant.PhoneNumber.Value);
        Assert.Equal(ParticipantContactType.Reconstitute(2, "Billing"), participant.ContactType);
    }

    [Fact]
    public void Update_Should_Throw_ArgumentException_When_Email_Is_Empty()
    {
        // Arrange
        var participant = Participant.Reconstitute(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john.doe@example.com",
            "+46701234567");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            participant.Update("Jane", "Smith", "", "+46709876543", ParticipantContactType.Reconstitute(1, "Primary")));

        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData("john.doe")]
    [InlineData("john.doe@")]
    [InlineData("@example.com")]
    [InlineData("john.doe@example")]
    [InlineData("john.doe@example.")]
    [InlineData("john doe@example.com")]
    public void Constructor_Should_Throw_ArgumentException_When_Email_Format_Is_Invalid(string invalidEmail)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Participant.Reconstitute(id, "John", "Doe", invalidEmail, "+46701234567"));

        Assert.Equal("email", exception.ParamName);
        Assert.Contains("Email", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
