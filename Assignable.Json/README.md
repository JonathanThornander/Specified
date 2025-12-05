# Assignable.Json

JSON serialization support for `Assignable<T>` types using System.Text.Json.

## Usage

```csharp
using Assignable.Json;

// Configure JsonSerializerOptions
var options = new JsonSerializerOptions();
options.AddAssignableJsonFields();

// Use AssignableJsonField<T> in your models
public class MyModel
{
    public AssignableJsonField<string?> Name { get; set; }
    public AssignableJsonField<int?> Age { get; set; }
}

// Serialization preserves the three states
var model = new MyModel
{
    Name = new AssignableJsonField<string?>("John"), // assigned
    Age = AssignableJsonField<int>.Absent // absent
};

string json = JsonSerializer.Serialize(model, options);
// {"Name":"John"} - Age is not included
```

## License

MIT