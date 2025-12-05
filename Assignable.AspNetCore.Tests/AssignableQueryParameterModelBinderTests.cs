using Assignable.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Assignable.AspNetCore.Tests;

public class AssignableQueryParameterModelBinderTests
{
    private readonly AssignableQueryParameterOptions _defaultOptions = new();

    #region Scalar Tests

    [Fact]
    public async Task BindScalar_WhenValueNotPresent_ReturnsAbsent()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", ValueProviderResult.None);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int>>(context.Result.Model);
        Assert.False(result.IsAssigned);
    }

    [Fact]
    public async Task BindScalar_WhenValueIsEmpty_ReturnsAbsent()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", new ValueProviderResult(""));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int>>(context.Result.Model);
        Assert.False(result.IsAssigned);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("NULL")]
    [InlineData("Null")]
    [InlineData("nil")]
    [InlineData("NIL")]
    public async Task BindScalar_WhenValueIsNullString_ReturnsAssignedWithDefault(string nullValue)
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int?>(_defaultOptions);
        var context = CreateBindingContext<int?>("myParam", new ValueProviderResult(nullValue));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int?>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task BindScalar_WhenValueIsInteger_ReturnsAssignedWithValue()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", new ValueProviderResult("42"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task BindScalar_WhenValueIsString_ReturnsAssignedWithValue()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<string>(_defaultOptions);
        var context = CreateBindingContext<string>("myParam", new ValueProviderResult("hello"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<string>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public async Task BindScalar_WhenValueIsGuid_ReturnsAssignedWithValue()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var binder = new AssignableQueryParameterModelBinder<Guid>(_defaultOptions);
        var context = CreateBindingContext<Guid>("myParam", new ValueProviderResult(expectedGuid.ToString()));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<Guid>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(expectedGuid, result.Value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    public async Task BindScalar_WhenValueIsBoolean_ReturnsAssignedWithValue(string input, bool expected)
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<bool>(_defaultOptions);
        var context = CreateBindingContext<bool>("myParam", new ValueProviderResult(input));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<bool>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task BindScalar_WhenValueIsInvalid_ReturnsFailed()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", new ValueProviderResult("not-a-number"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.False(context.Result.IsModelSet);
        Assert.True(context.ModelState.ContainsKey("myParam"));
    }

    #endregion

    #region Enumerable Tests

    [Fact]
    public async Task BindEnumerable_WhenValueIsNullString_ReturnsAssignedWithDefault()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int[]>(_defaultOptions);
        var context = CreateBindingContext<int[]>("myParam", new ValueProviderResult("null"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int[]>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task BindEnumerable_WhenCommaSeparatedValues_ReturnsArray()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int[]>(_defaultOptions);
        var context = CreateBindingContext<int[]>("myParam", new ValueProviderResult("1,2,3"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int[]>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value);
    }

    [Fact]
    public async Task BindEnumerable_WhenMultipleQueryParams_ReturnsArray()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int[]>(_defaultOptions);
        var context = CreateBindingContext<int[]>("myParam", new ValueProviderResult(new StringValues(new[] { "1", "2", "3" })));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int[]>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value);
    }

    [Fact]
    public async Task BindEnumerable_WhenEmptyValues_ReturnsEmptyCollection()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int[]>(_defaultOptions);
        var context = CreateBindingContext<int[]>("myParam", new ValueProviderResult(""));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int[]>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task BindEnumerable_WhenListType_ReturnsList()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<List<int>>(_defaultOptions);
        var context = CreateBindingContext<List<int>>("myParam", new ValueProviderResult("1,2,3"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<List<int>>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal(new List<int> { 1, 2, 3 }, result.Value);
    }

    #endregion

    #region Custom Null Values Tests

    [Fact]
    public async Task BindScalar_WithCustomNullValues_RecognizesCustomValue()
    {
        // Arrange
        var options = new AssignableQueryParameterOptions
        {
            NullValues = new[] { "undefined", "none" }
        };
        var binder = new AssignableQueryParameterModelBinder<int?>(_defaultOptions);
        var binderCustom = new AssignableQueryParameterModelBinder<int?>(options);
        var context = CreateBindingContext<int?>("myParam", new ValueProviderResult("undefined"));

        // Act
        await binderCustom.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<int?>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task BindScalar_WithCustomNullValues_DoesNotRecognizeDefaultNull()
    {
        // Arrange
        var options = new AssignableQueryParameterOptions
        {
            NullValues = new[] { "undefined", "none" }
        };
        var binder = new AssignableQueryParameterModelBinder<string>(options);
        var context = CreateBindingContext<string>("myParam", new ValueProviderResult("null"));

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var result = Assert.IsType<AssignableQueryParameter<string>>(context.Result.Model);
        Assert.True(result.IsAssigned);
        Assert.Equal("null", result.Value); // "null" is treated as a regular string value
    }

    #endregion

    #region AsAssignable Tests

    [Fact]
    public async Task AsAssignable_WhenAbsent_ReturnsAbsentAssignable()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", ValueProviderResult.None);

        // Act
        await binder.BindModelAsync(context);
        var result = Assert.IsType<AssignableQueryParameter<int>>(context.Result.Model);
        var domain = result.AsAssignable();

        // Assert
        Assert.False(domain.IsAssigned);
    }

    [Fact]
    public async Task AsAssignable_WhenAssigned_ReturnsAssignedAssignable()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<int>(_defaultOptions);
        var context = CreateBindingContext<int>("myParam", new ValueProviderResult("42"));

        // Act
        await binder.BindModelAsync(context);
        var result = Assert.IsType<AssignableQueryParameter<int>>(context.Result.Model);
        var domain = result.AsAssignable();

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Equal(42, domain.Value);
    }

    [Fact]
    public async Task AsAssignable_WithSelector_WhenAbsent_ReturnsAbsentAssignable()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<string>(_defaultOptions);
        var context = CreateBindingContext<string>("myParam", ValueProviderResult.None);

        // Act
        await binder.BindModelAsync(context);
        var result = Assert.IsType<AssignableQueryParameter<string>>(context.Result.Model);
        var domain = result.AsAssignable(s => s?.ToUpper());

        // Assert
        Assert.False(domain.IsAssigned);
    }

    [Fact]
    public async Task AsAssignable_WithSelector_WhenAssigned_ReturnsProjectedValue()
    {
        // Arrange
        var binder = new AssignableQueryParameterModelBinder<string>(_defaultOptions);
        var context = CreateBindingContext<string>("myParam", new ValueProviderResult("hello"));

        // Act
        await binder.BindModelAsync(context);
        var result = Assert.IsType<AssignableQueryParameter<string>>(context.Result.Model);
        var domain = result.AsAssignable(s => s?.ToUpper());

        // Assert
        Assert.True(domain.IsAssigned);
        Assert.Equal("HELLO", domain.Value);
    }

    #endregion

    private static ModelBindingContext CreateBindingContext<T>(string modelName, ValueProviderResult valueProviderResult)
    {
        var valueProvider = new TestValueProvider(modelName, valueProviderResult);
        var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(AssignableQueryParameter<T>));

        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ModelMetadata = modelMetadata,
            ValueProvider = valueProvider
        };

        return bindingContext;
    }

    private class TestValueProvider : IValueProvider
    {
        private readonly string _key;
        private readonly ValueProviderResult _result;

        public TestValueProvider(string key, ValueProviderResult result)
        {
            _key = key;
            _result = result;
        }

        public bool ContainsPrefix(string prefix) => prefix == _key;

        public ValueProviderResult GetValue(string key)
        {
            return key == _key ? _result : ValueProviderResult.None;
        }
    }
}
