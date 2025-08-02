# MockQueryable

Extensions for mocking [Entity Framework Core](https://github.com/dotnet/efcore) async queries like `ToListAsync`, `FirstOrDefaultAsync`, and more using popular mocking libraries such as **Moq**, **NSubstitute**, and **FakeItEasy** — all without hitting the database.

👉 [Support the project](https://github.com/sponsors/romantitov) or
☕ [Buy me a coffee](https://buymeacoffee.com/romant)

---

## 📦 NuGet Packages

| Package | Downloads | Latest Version | Install via Package Manager |
|--------|-----------|----------------|------------------------------|
| [MockQueryable.Core](https://www.nuget.org/packages/MockQueryable.Core/) | ![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Core.svg) | ![Version](https://img.shields.io/nuget/v/MockQueryable.Core.svg) | `Install-Package MockQueryable.Core` |
| [MockQueryable.EntityFrameworkCore](https://www.nuget.org/packages/MockQueryable.EntityFrameworkCore/) | ![Downloads](https://img.shields.io/nuget/dt/MockQueryable.EntityFrameworkCore.svg) | ![Version](https://img.shields.io/nuget/v/MockQueryable.EntityFrameworkCore.svg) | `Install-Package MockQueryable.EntityFrameworkCore` |
| [MockQueryable.Moq](https://www.nuget.org/packages/MockQueryable.Moq/) | ![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Moq.svg) | ![Version](https://img.shields.io/nuget/v/MockQueryable.Moq.svg) | `Install-Package MockQueryable.Moq` |
| [MockQueryable.NSubstitute](https://www.nuget.org/packages/MockQueryable.NSubstitute/) | ![Downloads](https://img.shields.io/nuget/dt/MockQueryable.NSubstitute.svg) | ![Version](https://img.shields.io/nuget/v/MockQueryable.NSubstitute.svg) | `Install-Package MockQueryable.NSubstitute` |
| [MockQueryable.FakeItEasy](https://www.nuget.org/packages/MockQueryable.FakeItEasy/) | ![Downloads](https://img.shields.io/nuget/dt/MockQueryable.FakeItEasy.svg) | ![Version](https://img.shields.io/nuget/v/MockQueryable.FakeItEasy.svg) | `Install-Package MockQueryable.FakeItEasy` |

---

## ✅ Build & Status

![.NET Core](https://github.com/romantitov/MockQueryable/workflows/.NET%20Core/badge.svg)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/ggdbipcyyfb4av9e?svg=true)](https://ci.appveyor.com/project/handybudget/mockqueryable)
[![License](https://img.shields.io/github/license/romantitov/MockQueryable.svg)](https://github.com/romantitov/MockQueryable/blob/master/LICENSE)

---

## ⭐ GitHub Stats

![Stars](https://img.shields.io/github/stars/romantitov/MockQueryable)
![Contributors](https://img.shields.io/github/contributors/romantitov/MockQueryable)
![Last Commit](https://img.shields.io/github/last-commit/romantitov/MockQueryable)
![Commit Activity](https://img.shields.io/github/commit-activity/m/romantitov/MockQueryable)
![Open Issues](https://img.shields.io/github/issues/romantitov/MockQueryable)

---

## 💡 Why Use MockQueryable?

Avoid hitting the real database in unit tests when querying via `IQueryable`:

```csharp
var query = _userRepository.GetQueryable();

await query.AnyAsync(x => ...);
await query.FirstOrDefaultAsync(x => ...);
await query.ToListAsync();
// etc.
```

---

## 🚀 Getting Started

### 1. Create Test Data

```csharp
var users = new List<UserEntity>
{
    new UserEntity { LastName = "Smith", DateOfBirth = new DateTime(2012, 1, 20) },
    // More test data...
};
```

### 2. Build the Mock

```csharp
var mock = users.BuildMock(); // for IQueryable
```

### 3. Set Up in Your favorite Mocking Framework

#### Moq
```csharp
_userRepository.Setup(x => x.GetQueryable()).Returns(mock);
```

#### NSubstitute
```csharp
_userRepository.GetQueryable().Returns(mock);
```

#### FakeItEasy
```csharp
A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
```

---

## 🗃️ Mocking `DbSet<T>`

```csharp
var mockDbSet = users.BuildMockDbSet();

// Moq
var repo = new TestDbSetRepository(mockDbSet.Object);

// NSubstitute / FakeItEasy
var repo = new TestDbSetRepository(mockDbSet);
```

---

## 🔧 Adding Custom Logic

### Example: Custom `FindAsync`

```csharp
mock.Setup(x => x.FindAsync(userId)).ReturnsAsync((object[] ids) =>
{
    var id = (Guid)ids[0];
    return users.FirstOrDefault(x => x.Id == id);
});
```

### Example: Custom Expression Visitor 
Build a mock with the custom [SampleLikeExpressionVisitor](src/MockQueryable/MockQueryable.Sample/SampleLikeExpressionVisitor.cs) for testing `EF.Functions.Like`

```csharp
var mockDbSet = users.BuildMockDbSet<UserEntity, SampleLikeExpressionVisitor>();
```

---

## 🧩 Extend for Other Frameworks

You can even create your own extensions. Check the [example here](https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable.Moq/MoqExtensions.cs).

---

## 🔍 Sample Project

See the [sample project](https://github.com/romantitov/MockQueryable/tree/master/src/MockQueryable/MockQueryable.Sample) for working examples.

---

