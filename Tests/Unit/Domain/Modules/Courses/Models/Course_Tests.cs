using Backend.Domain.Modules.Courses.Models;

namespace Backend.Tests.Unit.Domain.Modules.Courses.Models;

public class Course_Tests
{
    #region Constructor - Valid Cases

    [Fact]
    public void Constructor_Should_Create_Course_When_All_Parameters_Are_Valid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.NotNull(course);
        Assert.Equal(id, course.Id);
        Assert.Equal(title, course.Title);
        Assert.Equal(description, course.Description);
        Assert.Equal(durationInDays, course.DurationInDays);
    }

    [Theory]
    [InlineData("C# Fundamentals", "Learn C# programming", 5)]
    [InlineData("Advanced .NET", "Master .NET Core", 15)]
    [InlineData("Database Design", "SQL Server and EF Core", 20)]
    [InlineData("A", "B", 1)]
    [InlineData("Very Long Course Title With Many Words", "Very Long Description With Many Words", 365)]
    public void Constructor_Should_Create_Course_With_Various_Valid_Inputs(
        string title, string description, int durationInDays)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.NotNull(course);
        Assert.Equal(id, course.Id);
        Assert.Equal(title, course.Title);
        Assert.Equal(description, course.Description);
        Assert.Equal(durationInDays, course.DurationInDays);
    }

    [Fact]
    public void Constructor_Should_Create_Course_With_Minimum_Valid_Duration()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 1;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal(1, course.DurationInDays);
    }

    [Fact]
    public void Constructor_Should_Create_Course_With_Large_Duration()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = int.MaxValue;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal(int.MaxValue, course.DurationInDays);
    }

    #endregion

    #region Constructor - Invalid Id

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Id_Is_Empty()
    {
        // Arrange
        var id = Guid.Empty;
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("id", exception.ParamName);
        Assert.Contains("Course id cannot be empty", exception.Message);
    }

    #endregion

    #region Constructor - Invalid Title

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Title_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        string title = null!;
        var description = "Test Description";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("title", exception.ParamName);
        Assert.Contains("Course title cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Title_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "";
        var description = "Test Description";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("title", exception.ParamName);
        Assert.Contains("Course title cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Title_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "   ";
        var description = "Test Description";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("title", exception.ParamName);
        Assert.Contains("Course title cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_Should_Throw_ArgumentException_When_Title_Is_Only_Whitespace_Characters(string title)
    {
        // Arrange
        var id = Guid.NewGuid();
        var description = "Test Description";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("title", exception.ParamName);
    }

    #endregion

    #region Constructor - Invalid Description

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Description_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        string description = null!;
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("description", exception.ParamName);
        Assert.Contains("Course description cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Description_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("description", exception.ParamName);
        Assert.Contains("Course description cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Description_Is_Whitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "   ";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("description", exception.ParamName);
        Assert.Contains("Course description cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_Should_Throw_ArgumentException_When_Description_Is_Only_Whitespace_Characters(string description)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var durationInDays = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));

        Assert.Equal("description", exception.ParamName);
    }

    #endregion

    #region Constructor - Invalid Duration

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_DurationInDays_Is_Zero()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 0;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_DurationInDays_Is_Negative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = -1;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_DurationInDays_Is_Various_Negative_Values(int durationInDays)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Course.Reconstitute(id, title, description, durationInDays));
    }

    #endregion

    #region Trimming Behavior

    [Fact]
    public void Constructor_Should_Trim_Leading_Whitespace_From_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "   Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Course", course.Title);
    }

    [Fact]
    public void Constructor_Should_Trim_Trailing_Whitespace_From_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course   ";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Course", course.Title);
    }

    [Fact]
    public void Constructor_Should_Trim_Leading_And_Trailing_Whitespace_From_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "   Test Course   ";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Course", course.Title);
    }

    [Fact]
    public void Constructor_Should_Trim_Leading_Whitespace_From_Description()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "   Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Description", course.Description);
    }

    [Fact]
    public void Constructor_Should_Trim_Trailing_Whitespace_From_Description()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description   ";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Description", course.Description);
    }

    [Fact]
    public void Constructor_Should_Trim_Leading_And_Trailing_Whitespace_From_Description()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "   Test Description   ";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Description", course.Description);
    }

    [Fact]
    public void Constructor_Should_Trim_Both_Title_And_Description()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "   Test Course   ";
        var description = "   Test Description   ";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test Course", course.Title);
        Assert.Equal("Test Description", course.Description);
    }

    [Fact]
    public void Constructor_Should_Preserve_Internal_Whitespace_In_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "  Test   Course  With   Spaces  ";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test   Course  With   Spaces", course.Title);
    }

    [Fact]
    public void Constructor_Should_Preserve_Internal_Whitespace_In_Description()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "  Test   Description  With   Spaces  ";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Test   Description  With   Spaces", course.Description);
    }

    #endregion

    #region Property Immutability

    [Fact]
    public void Properties_Should_Be_Read_Only()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert - Properties should have getters but no setters
        Assert.Equal(id, course.Id);
        Assert.Equal(title, course.Title);
        Assert.Equal(description, course.Description);
        Assert.Equal(durationInDays, course.DurationInDays);

        // Verify properties are read-only by checking the property info
        var idProperty = typeof(Course).GetProperty(nameof(Course.Id));
        var titleProperty = typeof(Course).GetProperty(nameof(Course.Title));
        var descriptionProperty = typeof(Course).GetProperty(nameof(Course.Description));
        var durationProperty = typeof(Course).GetProperty(nameof(Course.DurationInDays));

        Assert.NotNull(idProperty);
        Assert.NotNull(titleProperty);
        Assert.NotNull(descriptionProperty);
        Assert.NotNull(durationProperty);

        Assert.Null(idProperty.SetMethod);
        Assert.Null(titleProperty.GetSetMethod());
        Assert.Null(descriptionProperty.GetSetMethod());
        Assert.Null(durationProperty.GetSetMethod());
    }

    [Fact]
    public void Two_Courses_With_Same_Id_Should_Have_Equal_Id()
    {
        // Arrange
        var id = Guid.NewGuid();
        var course1 = Course.Reconstitute(id, "Course 1", "Description 1", 10);
        var course2 = Course.Reconstitute(id, "Course 2", "Description 2", 20);

        // Act & Assert
        Assert.Equal(course1.Id, course2.Id);
    }

    [Fact]
    public void Two_Courses_With_Different_Ids_Should_Have_Different_Ids()
    {
        // Arrange
        var course1 = Course.Reconstitute(Guid.NewGuid(), "Course 1", "Description 1", 10);
        var course2 = Course.Reconstitute(Guid.NewGuid(), "Course 1", "Description 1", 10);

        // Act & Assert
        Assert.NotEqual(course1.Id, course2.Id);
    }

    #endregion

    #region Special Characters and Unicode

    [Fact]
    public void Constructor_Should_Accept_Title_With_Special_Characters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "C# & .NET: Advanced Topics!";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("C# & .NET: Advanced Topics!", course.Title);
    }

    [Fact]
    public void Constructor_Should_Accept_Description_With_Special_Characters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Course";
        var description = "Learn C#, ASP.NET & Entity Framework! 100% coverage.";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Learn C#, ASP.NET & Entity Framework! 100% coverage.", course.Description);
    }

    [Fact]
    public void Constructor_Should_Accept_Unicode_Characters_In_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "C# Programmering 编程 プログラミング";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("C# Programmering 编程 プログラミング", course.Title);
    }

    [Fact]
    public void Constructor_Should_Accept_Emojis_In_Title()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Advanced C# 🚀";
        var description = "Test Description";
        var durationInDays = 10;

        // Act
        var course = Course.Reconstitute(id, title, description, durationInDays);

        // Assert
        Assert.Equal("Advanced C# 🚀", course.Title);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_Should_Change_Values_When_Input_Is_Valid()
    {
        var course = Course.Reconstitute(Guid.NewGuid(), "Old", "Old desc", 5);

        course.Update("New Title", "New Description", 10);

        Assert.Equal("New Title", course.Title);
        Assert.Equal("New Description", course.Description);
        Assert.Equal(10, course.DurationInDays);
    }

    [Fact]
    public void Update_Should_Throw_ArgumentException_When_Title_Is_Whitespace()
    {
        var course = Course.Reconstitute(Guid.NewGuid(), "Old", "Old desc", 5);

        var ex = Assert.Throws<ArgumentException>(() => course.Update("   ", "New Description", 10));
        Assert.Equal("title", ex.ParamName);
    }

    #endregion
}
