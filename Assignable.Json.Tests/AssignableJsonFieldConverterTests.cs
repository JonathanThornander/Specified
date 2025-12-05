using System.Text.Json;
using Assignable.Json;

namespace Assignable.Json.Tests;

public class AssignableJsonFieldConverterTests
{
    private readonly JsonSerializerOptions _defaultOptions;

    public AssignableJsonFieldConverterTests()
    {
        _defaultOptions = new JsonSerializerOptions();
        _defaultOptions.AddAssignableJsonFields();
    }

    #region Deserialization Tests

    [Fact]
    public void Deserialize_WhenPropertyNotPresent_ReturnsAbsent()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Name.IsAssigned);
        Assert.False(result.Age.IsAssigned);
    }

    [Fact]
    public void Deserialize_WhenPropertyIsNull_ReturnsAssignedWithNull()
    {
        // Arrange
        var json = """{"Name": null, "Age": null}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Name.IsAssigned);
        Assert.Null(result.Name.Value);
        Assert.True(result.Age.IsAssigned);
        Assert.Null(result.Age.Value);
    }

    [Fact]
    public void Deserialize_WhenPropertyHasValue_ReturnsAssignedWithValue()
    {
        // Arrange
        var json = """{"Name": "John", "Age": 30}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Name.IsAssigned);
        Assert.Equal("John", result.Name.Value);
        Assert.True(result.Age.IsAssigned);
        Assert.Equal(30, result.Age.Value);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("NULL")]
    [InlineData("Null")]
    [InlineData("nil")]
    [InlineData("NIL")]
    public void Deserialize_WhenStringValueIsNullKeyword_ReturnsAssignedWithNull(string nullValue)
    {
        // Arrange
        var json = $$"""{"Name": "{{nullValue}}"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Name.IsAssigned);
        Assert.Null(result.Name.Value);
    }

    [Fact]
    public void Deserialize_WhenNestedObject_ReturnsAssignedWithValue()
    {
        // Arrange
        var json = """{"Address": {"Street": "123 Main St", "City": "Boston"}}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModelWithAddress>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Address.IsAssigned);
        Assert.NotNull(result.Address.Value);
        Assert.Equal("123 Main St", result.Address.Value.Street);
        Assert.Equal("Boston", result.Address.Value.City);
    }

    [Fact]
    public void Deserialize_WhenArray_ReturnsAssignedWithValue()
    {
        // Arrange
        var json = """{"Tags": ["tag1", "tag2", "tag3"]}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModelWithArray>(json, _defaultOptions);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tags.IsAssigned);
        Assert.Equal(new[] { "tag1", "tag2", "tag3" }, result.Tags.Value);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void Serialize_WhenAbsent_WritesNull()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var json = JsonSerializer.Serialize(model, _defaultOptions);

        // Assert
        Assert.Contains("\"Name\":null", json);
        Assert.Contains("\"Age\":null", json);
    }

    [Fact]
    public void Serialize_WhenAssignedWithNull_WritesNull()
    {
        // Arrange
        var model = new TestModel
        {
            Name = new AssignableJsonField<string?>(null),
            Age = new AssignableJsonField<int?>(null)
        };

        // Act
        var json = JsonSerializer.Serialize(model, _defaultOptions);

        // Assert
        Assert.Contains("\"Name\":null", json);
        Assert.Contains("\"Age\":null", json);
    }

    [Fact]
    public void Serialize_WhenAssignedWithValue_WritesValue()
    {
        // Arrange
        var model = new TestModel
        {
            Name = new AssignableJsonField<string?>("John"),
            Age = new AssignableJsonField<int?>(30)
        };

        // Act
        var json = JsonSerializer.Serialize(model, _defaultOptions);

        // Assert
        Assert.Contains("\"Name\":\"John\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    #endregion

    #region Custom Null Values Tests

    [Fact]
    public void Deserialize_WithCustomNullValues_RecognizesCustomValue()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.AddAssignableJsonFields(opts =>
        {
            opts.NullValues = new[] { "undefined", "none" };
        });
        var json = """{"Name": "undefined"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Name.IsAssigned);
        Assert.Null(result.Name.Value);
    }

    [Fact]
    public void Deserialize_WithCustomNullValues_DoesNotRecognizeDefaultNull()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.AddAssignableJsonFields(opts =>
        {
            opts.NullValues = new[] { "undefined", "none" };
        });
        var json = """{"Name": "null"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Name.IsAssigned);
        Assert.Equal("null", result.Name.Value); // "null" is treated as a regular string value
    }

    #endregion

    #region AsAssignable Tests

    [Fact]
    public void AsAssignable_WhenAbsent_ReturnsAbsentAssignable()
    {
        // Arrange
        var field = AssignableJsonField<int>.Absent;

        // Act
        var domain = field.AsAssignable();

        // Assert
        Assert.False(domain.IsAssigned);
    }

    [Fact]
    public void AsAssignable_WhenAssignedWithValue_ReturnsAssignedAssignable()
    {
        // Arrange
        var field = new AssignableJsonField<int>(42);

        // Act
        var domain = field.AsAssignable();

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Equal(42, domain.Value);
    }

    [Fact]
    public void AsAssignable_WhenAssignedWithNull_ReturnsAssignedAssignableWithNull()
    {
        // Arrange
        var field = new AssignableJsonField<int?>(null);

        // Act
        var domain = field.AsAssignable();

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Null(domain.Value);
    }

    [Fact]
    public void AsAssignable_WithSelector_WhenAbsent_ReturnsAbsentAssignable()
    {
        // Arrange
        var field = AssignableJsonField<string>.Absent;

        // Act
        var domain = field.AsAssignable(s => s?.ToUpper());

        // Assert
        Assert.False(domain.IsAssigned);
    }

    [Fact]
    public void AsAssignable_WithSelector_WhenAssignedWithValue_ReturnsProjectedValue()
    {
        // Arrange
        var field = new AssignableJsonField<string>("hello");

        // Act
        var domain = field.AsAssignable(s => s?.ToUpper());

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Equal("HELLO", domain.Value);
    }

    [Fact]
    public void AsAssignable_WithSelector_WhenAssignedWithNull_ReturnsProjectedNull()
    {
        // Arrange
        var field = new AssignableJsonField<string?>(null);

        // Act
        var domain = field.AsAssignable(s => s?.ToUpper() ?? "DEFAULT");

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Equal("DEFAULT", domain.Value);
    }

    #endregion

    #region Test Models

    private class TestModel
    {
        public AssignableJsonField<string?> Name { get; set; }
        public AssignableJsonField<int?> Age { get; set; }
    }

    private class TestModelWithAddress
    {
        public AssignableJsonField<Address?> Address { get; set; }
    }

    private class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    private class TestModelWithArray
    {
        public AssignableJsonField<string[]?> Tags { get; set; }
    }

    #endregion
}
