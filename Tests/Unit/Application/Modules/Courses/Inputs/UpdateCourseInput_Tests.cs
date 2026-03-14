using Backend.Application.Common;
using Backend.Application.Modules.Courses.Inputs;

namespace Backend.Tests.Unit.Application.Modules.Courses.Inputs;

public class UpdateCourseInput_Tests
{
    [Fact]
    public void Constructor_Should_Initialize_All_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var input = new UpdateCourseInput(id, title, description, durationInDays);

        // Assert
        Assert.Equal(id, input.Id);
        Assert.Equal(title, input.Title);
        Assert.Equal(description, input.Description);
        Assert.Equal(durationInDays, input.DurationInDays);
    }

    [Fact]
    public void Constructor_Should_Accept_Empty_Guid()
    {
        // Arrange & Act
        var input = new UpdateCourseInput(Guid.Empty, "Title", "Description", 10);

        // Assert
        Assert.Equal(Guid.Empty, input.Id);
    }

    [Fact]
    public void Constructor_Should_Accept_Null_Values()
    {
        // Arrange & Act
        var input = new UpdateCourseInput(Guid.NewGuid(), null!, null!, 0);

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
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var input = new UpdateCourseInput(id, title, description, durationInDays);

        // Assert
        Assert.Equal(id, input.Id);
        Assert.Equal(title, input.Title);
        Assert.Equal(description, input.Description);
        Assert.Equal(durationInDays, input.DurationInDays);
    }

    [Fact]
    public void Two_Instances_With_Same_Values_Should_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var input1 = new UpdateCourseInput(id, "Test Course", "Test Description", 10);
        var input2 = new UpdateCourseInput(id, "Test Course", "Test Description", 10);

        // Act & Assert
        Assert.Equal(input1, input2);
        Assert.True(input1 == input2);
        Assert.False(input1 != input2);
        Assert.Equal(input1.GetHashCode(), input2.GetHashCode());
    }

    [Fact]
    public void Two_Instances_With_Different_Id_Should_Not_Be_Equal()
    {
        // Arrange
        var input1 = new UpdateCourseInput(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var input2 = new UpdateCourseInput(Guid.NewGuid(), "Test Course", "Test Description", 10);

        // Act & Assert
        Assert.NotEqual(input1, input2);
        Assert.False(input1 == input2);
        Assert.True(input1 != input2);
    }

    [Fact]
    public void Two_Instances_With_Different_Title_Should_Not_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var input1 = new UpdateCourseInput(id, "Course 1", "Test Description", 10);
        var input2 = new UpdateCourseInput(id, "Course 2", "Test Description", 10);

        // Act & Assert
        Assert.NotEqual(input1, input2);
    }

    [Fact]
    public void Two_Instances_With_Different_Description_Should_Not_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var input1 = new UpdateCourseInput(id, "Test Course", "Description 1", 10);
        var input2 = new UpdateCourseInput(id, "Test Course", "Description 2", 10);

        // Act & Assert
        Assert.NotEqual(input1, input2);
    }

    [Fact]
    public void Two_Instances_With_Different_DurationInDays_Should_Not_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var input1 = new UpdateCourseInput(id, "Test Course", "Test Description", 10);
        var input2 = new UpdateCourseInput(id, "Test Course", "Test Description", 20);

        // Act & Assert
        Assert.NotEqual(input1, input2);
    }

    [Fact]
    public void With_Expression_Should_Create_New_Instance_With_Modified_Id()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var original = new UpdateCourseInput(originalId, "Test Course", "Test Description", 10);

        // Act
        var modified = original with { Id = newId };

        // Assert
        Assert.Equal(originalId, original.Id);
        Assert.Equal(newId, modified.Id);
        Assert.Equal(original.Title, modified.Title);
    }

    [Fact]
    public void With_Expression_Should_Create_New_Instance_With_Modified_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var original = new UpdateCourseInput(id, "Original Title", "Test Description", 10);

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
        var id = Guid.NewGuid();
        var original = new UpdateCourseInput(id, "Test Course", "Original Description", 10);

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
        var id = Guid.NewGuid();
        var original = new UpdateCourseInput(id, "Test Course", "Test Description", 10);

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
        var id = Guid.NewGuid();
        var input = new UpdateCourseInput(id, "Test Course", "Test Description", 10);

        // Act
        var result = input.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(id.ToString(), result);
        Assert.Contains("Test Course", result);
        Assert.Contains("Test Description", result);
        Assert.Contains("10", result);
    }

    [Fact]
    public void Deconstruct_Should_Extract_All_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var input = new UpdateCourseInput(id, "Test Course", "Test Description", 10);

        // Act
        var (extractedId, title, description, durationInDays) = input;

        // Assert
        Assert.Equal(id, extractedId);
        Assert.Equal("Test Course", title);
        Assert.Equal("Test Description", description);
        Assert.Equal(10, durationInDays);
    }
}

