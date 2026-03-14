using Backend.Application.Common;
using Backend.Application.Modules.Courses.Inputs;

namespace Backend.Tests.Unit.Application.Modules.Courses.Inputs;

public class CreateCourseInput_Tests
{
    [Fact]
    public void Constructor_Should_Initialize_All_Properties()
    {
        // Arrange
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var input = new CreateCourseInput(title, description, durationInDays);

        // Assert
        Assert.Equal(title, input.Title);
        Assert.Equal(description, input.Description);
        Assert.Equal(durationInDays, input.DurationInDays);
    }

    [Fact]
    public void Constructor_Should_Accept_Null_Values()
    {
        // Arrange & Act
        var input = new CreateCourseInput(null!, null!, 0);

        // Assert
        Assert.Null(input.Title);
        Assert.Null(input.Description);
        Assert.Equal(0, input.DurationInDays);
    }

    [Theory]
    [InlineData("Course 1", "Description 1", 5)]
    [InlineData("Course 2", "Description 2", 10)]
    [InlineData("", "", 0)]
    [InlineData("   ", "   ", -1)]
    public void Constructor_Should_Accept_Various_Values(string title, string description, int durationInDays)
    {
        // Act
        var input = new CreateCourseInput(title, description, durationInDays);

        // Assert
        Assert.Equal(title, input.Title);
        Assert.Equal(description, input.Description);
        Assert.Equal(durationInDays, input.DurationInDays);
    }

    [Fact]
    public void Two_Instances_With_Same_Values_Should_Be_Equal()
    {
        // Arrange
        var input1 = new CreateCourseInput("Test Course", "Test Description", 10);
        var input2 = new CreateCourseInput("Test Course", "Test Description", 10);

        // Act & Assert
        Assert.Equal(input1, input2);
        Assert.True(input1 == input2);
        Assert.False(input1 != input2);
        Assert.Equal(input1.GetHashCode(), input2.GetHashCode());
    }

    [Fact]
    public void Two_Instances_With_Different_Title_Should_Not_Be_Equal()
    {
        // Arrange
        var input1 = new CreateCourseInput("Course 1", "Test Description", 10);
        var input2 = new CreateCourseInput("Course 2", "Test Description", 10);

        // Act & Assert
        Assert.NotEqual(input1, input2);
        Assert.False(input1 == input2);
        Assert.True(input1 != input2);
    }

    [Fact]
    public void Two_Instances_With_Different_Description_Should_Not_Be_Equal()
    {
        // Arrange
        var input1 = new CreateCourseInput("Test Course", "Description 1", 10);
        var input2 = new CreateCourseInput("Test Course", "Description 2", 10);

        // Act & Assert
        Assert.NotEqual(input1, input2);
    }

    [Fact]
    public void Two_Instances_With_Different_DurationInDays_Should_Not_Be_Equal()
    {
        // Arrange
        var input1 = new CreateCourseInput("Test Course", "Test Description", 10);
        var input2 = new CreateCourseInput("Test Course", "Test Description", 20);

        // Act & Assert
        Assert.NotEqual(input1, input2);
    }

    [Fact]
    public void With_Expression_Should_Create_New_Instance_With_Modified_Title()
    {
        // Arrange
        var original = new CreateCourseInput("Original Title", "Test Description", 10);

        // Act
        var modified = original with { Title = "New Title" };

        // Assert
        Assert.Equal("Original Title", original.Title);
        Assert.Equal("New Title", modified.Title);
        Assert.Equal(original.Description, modified.Description);
        Assert.Equal(original.DurationInDays, modified.DurationInDays);
    }

    [Fact]
    public void With_Expression_Should_Create_New_Instance_With_Modified_Description()
    {
        // Arrange
        var original = new CreateCourseInput("Test Course", "Original Description", 10);

        // Act
        var modified = original with { Description = "New Description" };

        // Assert
        Assert.Equal("Original Description", original.Description);
        Assert.Equal("New Description", modified.Description);
    }

    [Fact]
    public void With_Expression_Should_Create_New_Instance_With_Modified_DurationInDays()
    {
        // Arrange
        var original = new CreateCourseInput("Test Course", "Test Description", 10);

        // Act
        var modified = original with { DurationInDays = 20 };

        // Assert
        Assert.Equal(10, original.DurationInDays);
        Assert.Equal(20, modified.DurationInDays);
    }

    [Fact]
    public void ToString_Should_Return_String_Representation()
    {
        // Arrange
        var input = new CreateCourseInput("Test Course", "Test Description", 10);

        // Act
        var result = input.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Test Course", result);
        Assert.Contains("Test Description", result);
        Assert.Contains("10", result);
    }

    [Fact]
    public void Deconstruct_Should_Extract_All_Properties()
    {
        // Arrange
        var input = new CreateCourseInput("Test Course", "Test Description", 10);

        // Act
        var (title, description, durationInDays) = input;

        // Assert
        Assert.Equal("Test Course", title);
        Assert.Equal("Test Description", description);
        Assert.Equal(10, durationInDays);
    }
}

