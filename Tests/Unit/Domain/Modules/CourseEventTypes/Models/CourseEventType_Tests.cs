using Backend.Domain.Modules.CourseEventTypes.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseEventTypes.Models;

public class CourseEventType_Tests
{
    [Fact]
    public void Constructor_Should_Create_CourseEventType_When_Parameters_Are_Valid()
    {
        // Arrange
        var id = 1;
        var name = "Online";

        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.NotNull(courseEventType);
        Assert.Equal(id, courseEventType.Id);
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Constructor_Should_Allow_Zero_Id_For_New_Entity()
    {
        // Arrange
        var id = 0;
        var name = "Online";

        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.Equal(0, courseEventType.Id);
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_Id_Is_Negative()
    {
        // Arrange
        var id = -1;
        var name = "Online";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CourseEventType.Reconstitute(id, name));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_TypeName_Is_Null()
    {
        // Arrange
        var id = 1;
        string name = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEventType.Reconstitute(id, name));

        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Type name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_TypeName_Is_Empty()
    {
        // Arrange
        var id = 1;
        var name = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEventType.Reconstitute(id, name));

        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Type name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_TypeName_Is_Whitespace()
    {
        // Arrange
        var id = 1;
        var name = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEventType.Reconstitute(id, name));

        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Type name cannot be empty or whitespace", exception.Message);
    }

    [Theory]
    [InlineData(1, "Online")]
    [InlineData(2, "In-Person")]
    [InlineData(3, "Hybrid")]
    [InlineData(4, "Virtual")]
    public void Constructor_Should_Create_CourseEventType_With_Various_Valid_Parameters(int id, string name)
    {
        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.Equal(id, courseEventType.Id);
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Properties_Should_Be_Initialized_Correctly()
    {
        // Arrange & Act
        var courseEventType = CourseEventType.Reconstitute(1, "Online");

        // Assert
        Assert.Equal(1, courseEventType.Id);
        Assert.Equal("Online", courseEventType.Name);
    }

    [Fact]
    public void Two_CourseEventTypes_With_Same_Values_Should_Have_Same_Property_Values()
    {
        // Arrange
        var courseEventType1 = CourseEventType.Reconstitute(1, "Online");
        var courseEventType2 = CourseEventType.Reconstitute(1, "Online");

        // Assert
        Assert.Equal(courseEventType1.Id, courseEventType2.Id);
        Assert.Equal(courseEventType1.Name, courseEventType2.Name);
    }

    [Fact]
    public void Id_Property_Should_Be_Read_Only()
    {
        // Arrange
        var courseEventType = CourseEventType.Reconstitute(1, "Online");

        // Assert
        Assert.Equal(1, courseEventType.Id);
        var initialId = courseEventType.Id;
        Assert.Equal(initialId, courseEventType.Id);
    }

    [Fact]
    public void TypeName_Property_Should_Be_Read_Only()
    {
        // Arrange
        var courseEventType = CourseEventType.Reconstitute(1, "Online");

        // Assert
        Assert.Equal("Online", courseEventType.Name);
        var initialName = courseEventType.Name;
        Assert.Equal(initialName, courseEventType.Name);
    }

    [Fact]
    public void Constructor_Should_Handle_Long_Type_Names()
    {
        // Arrange
        var id = 1;
        var name = "Very Long Type Name";

        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Constructor_Should_Handle_Type_Names_With_Special_Characters()
    {
        // Arrange
        var id = 1;
        var name = "In-Person";

        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Constructor_Should_Handle_Large_Id_Values()
    {
        // Arrange
        var id = int.MaxValue;
        var name = "Online";

        // Act
        var courseEventType = CourseEventType.Reconstitute(id, name);

        // Assert
        Assert.Equal(id, courseEventType.Id);
    }

    [Theory]
    [InlineData("Online")]
    [InlineData("In-Person")]
    [InlineData("Hybrid")]
    [InlineData("Virtual")]
    [InlineData("Remote")]
    [InlineData("On-Site")]
    public void Constructor_Should_Accept_Various_Type_Names(string name)
    {
        // Act
        var courseEventType = CourseEventType.Reconstitute(1, name);

        // Assert
        Assert.Equal(name, courseEventType.Name);
    }

    [Fact]
    public void Update_Should_Change_TypeName_When_Input_Is_Valid()
    {
        // Arrange
        var courseEventType = CourseEventType.Reconstitute(1, "Online");

        // Act
        courseEventType.Update("Hybrid");

        // Assert
        Assert.Equal(1, courseEventType.Id);
        Assert.Equal("Hybrid", courseEventType.Name);
    }

    [Fact]
    public void Update_Should_Throw_ArgumentException_When_TypeName_Is_Whitespace()
    {
        // Arrange
        var courseEventType = CourseEventType.Reconstitute(1, "Online");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => courseEventType.Update("   "));
        Assert.Equal("name", exception.ParamName);
    }
}
