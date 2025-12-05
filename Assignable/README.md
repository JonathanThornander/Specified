# Assignable

Core library providing `Assignable<T>` - a type that distinguishes between absent, null, and present values.

## Usage

```csharp
// Create an assigned value
var assigned = new Assignable<string>("hello");
assigned.IsAssigned; // true
assigned.Value; // "hello"

// Create an absent value
var absent = Assignable<string>.Absent();
absent.IsAssigned; // false

// Implicit conversion
Assignable<int> number = 42; // IsAssigned = true, Value = 42
```

## License

MIT