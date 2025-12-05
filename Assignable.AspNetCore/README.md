# Assignable.AspNetCore 🎯

[![CI/CD](https://github.com/your-org/assignable/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/your-org/assignable/actions/workflows/ci-cd.yml)
[![NuGet](https://img.shields.io/nuget/v/Assignable.AspNetCore.svg)](https://www.nuget.org/packages/Assignable.AspNetCore/)

**Distinguish between "not provided" and "explicitly set to null"** in your ASP.NET Core APIs with zero friction.

Ever struggled with PATCH endpoints where you need to know if a client *omitted* a field or *intentionally set it to null*? **Assignable.AspNetCore** solves this elegantly.

## ✨ Features

- 🔍 **Three-state binding** - Absent, Null, or Value
- 🚀 **Zero boilerplate** - Just wrap your types
- ⚙️ **Fully customizable** - Configure null value representations

## 📦 Installation

```bash
dotnet add package Assignable.AspNetCore
```

## 🚀 Quick Start

### 1. Register Services

```csharp
// Program.cs

// Add query parameter support
builder.Services.AddAssignableQueryParameters();

// Add JSON body support
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AddAssignableJsonFields();
    });
```

### 2. Use in Your Controllers

```csharp
[HttpGet("users")]
public IActionResult GetUsers(
    AssignableQueryParameter<string> name,
    AssignableQueryParameter<int?> minAge,
    AssignableQueryParameter<bool?> isActive)
{
    // Check what the client actually sent
    if (name.IsAssigned)
    {
        // Client provided 'name' parameter
        // name.Value could be a string OR null (if they sent ?name=null)
    }
    else
    {
        // Client didn't include 'name' parameter at all
    }
    
    // Convert to domain model
    var domainFilter = new UserFilter
    {
        Name = name.AsAssignable(),
        MinAge = minAge.AsAssignable(),
        IsActive = isActive.AsAssignable()
    };
    
    return Ok(FilterUsers(domainFilter));
}
```

## 🎯 The Three States

| Query String | `IsAssigned` | `Value` |
|--------------|--------------|---------|
| *(omitted)* | `false` | `default` |
| `?name=null` | `true` | `null` |
| `?name=John` | `true` | `"John"` |

## ⚙️ Configuration

### Custom Null Values

By default, `"null"` and `"nil"` (case-insensitive) are treated as null. Customize this:

```csharp
builder.Services.AddAssignableQueryParameters(options =>
{
    options.NullValues = new[] { "null", "nil", "undefined", "none", "-" };
});
```

## 📚 Supported Types

Works with **any type** that ASP.NET Core can bind:

```csharp
AssignableQueryParameter<int>           // Integers
AssignableQueryParameter<string>        // Strings
AssignableQueryParameter<Guid>          // GUIDs
AssignableQueryParameter<DateTime>      // Dates
AssignableQueryParameter<MyEnum>        // Enums
AssignableQueryParameter<bool>          // Booleans (supports 1/0 too!)
AssignableQueryParameter<int?>          // Nullable types
AssignableQueryParameter<int[]>         // Arrays
AssignableQueryParameter<List<string>>  // Lists
```

## 🔗 Collections

Supports both comma-separated and repeated parameter styles:

```
?ids=1,2,3        → [1, 2, 3]
?ids=1&ids=2&ids=3 → [1, 2, 3]
?ids=null         → null (explicitly set)
?ids=             → [] (empty array)
```

## 🏗️ Domain Conversion

Seamlessly convert to your domain `Assignable<T>` type:

```csharp
public record UpdateUserRequest(
    AssignableQueryParameter<string> Name,
    AssignableQueryParameter<int?> Age);

public class UserService
{
    public void UpdateUser(int id, UpdateUserRequest request)
    {
        var update = new UserUpdate
        {
            Name = request.Name.AsAssignable(),
            Age = request.Age.AsAssignable()
        };
        
        // Now use update.Name.IsAssigned to check if it should be updated
        if (update.Name.IsAssigned)
        {
            user.Name = update.Name.Value;
        }
    }
}
```

## 💡 Pro Tips

### Minimal APIs Support

Works seamlessly with ASP.NET Core Minimal APIs:

```csharp
// Program.cs - same setup as before
builder.Services.AddAssignableQueryParameters();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AddAssignableJsonFields();
    });

// Minimal API endpoints
app.MapGet("/users", (AssignableQueryParameter<string> name, AssignableQueryParameter<int?> minAge) =>
{
    if (name.IsAssigned)
    {
        // Handle name filter
    }
    
    if (minAge.IsAssigned)
    {
        // Handle age filter
    }
    
    return Results.Ok(new { message = "Filtered users" });
});

app.MapPatch("/users/{id}", async (int id, AssignableJsonField<string?> name, AssignableJsonField<int?> age) =>
{
    var user = await GetUserAsync(id);
    
    if (name.IsAssigned)
    {
        user.Name = name.Value;
    }
    
    if (age.IsAssigned)
    {
        user.Age = age.Value;
    }
    
    await SaveUserAsync(user);
    return Results.Ok(user);
});
```

### PATCH Endpoints Made Easy

```csharp
[HttpPatch("users/{id}")]
public IActionResult PatchUser(
    int id,
    AssignableQueryParameter<string> name,
    AssignableQueryParameter<string> email)
{
    var user = GetUser(id);
    
    if (name.IsAssigned)
    {
        user.Name = name.Value;
    }
    
    if (email.IsAssigned)
    {
        user.Email = email.Value;
    }
    
    SaveUser(user);
    return Ok(user);
}
```

### JSON Request Bodies

Use `AssignableJsonField<T>` for JSON request bodies with the same three-state semantics:

```csharp
public class UpdateUserRequest
{
    public AssignableJsonField<string?> Name { get; set; }
    public AssignableJsonField<int?> Age { get; set; }
    public AssignableJsonField<string?> Email { get; set; }
}

[HttpPatch("users/{id}")]
public IActionResult PatchUser(int id, [FromBody] UpdateUserRequest request)
{
    var user = GetUser(id);
    
    if (request.Name.IsAssigned)
    {
        user.Name = request.Name.Value;
    }
    
    if (request.Age.IsAssigned)
    {
        user.Age = request.Age.Value;
    }
    
    if (request.Email.IsAssigned)
    {
        user.Email = request.Email.Value;
    }
    
    SaveUser(user);
    return Ok(user);
}
```

JSON payloads work as expected:

```json
// Only updates name (age and email are absent)
{ "name": "John" }

// Sets name to null, updates age (email is absent)
{ "name": null, "age": 30 }

// Updates all fields
{ "name": "John", "age": 30, "email": "john@example.com" }
```

## 📄 License

MIT

---

## 🤝 Contributing

We welcome contributions! This project uses:

- **GitHub Actions** for CI/CD
- **xUnit** for testing
- **Multi-targeting** (.NET 6.0+ support)

### Publishing Releases

1. Create a GitHub release with a version tag (e.g., `v1.0.1`)
2. CI/CD will automatically build, test, and publish to NuGet.org
3. Make sure `NUGET_API_KEY` secret is configured in repository settings

**Happy coding!** 🎉
