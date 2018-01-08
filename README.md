
# MockQueryable

[![Build status](https://ci.appveyor.com/api/projects/status/ggdbipcyyfb4av9e?svg=true)](https://ci.appveyor.com/project/handybudget/mockqueryable)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Moq.svg)](https://www.nuget.org/packages/MockQueryable.Moq/)

Extension for mocking [Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore/) operations such ToListAsync, FirstOrDefaultAsync etc. by [Moq](https://github.com/moq/moq)
When writing tests for your application it is often desirable to avoid hitting the database. The extension allows you to achieve this by creating a context – with behavior defined by your tests – that makes use of in-memory data.

### How do I get started?

```csharp
//1 - create a List<T> with test items
var users = new List<UserEntity>()
{
  new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
  ...
};

//2 - build mock by extension
var mock = users.AsQueryable().BuildMock();

//3 - setup the mock as Queryable
_userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
```
Check out the [sample project](https://github.com/romantitov/MockQueryable/tree/master/src/MockQueryable/MockQueryable.Sample)

### Where can I get it?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [MockQueryable.Moq](https://www.nuget.org/packages/MockQueryable.Moq/) from the package manager console:

```
PM> Install-Package MockQueryable.Moq
```

### Can I use it with my favorite mock framework?

You can install [MockQueryable.Core](https://www.nuget.org/packages/MockQueryable.Core/) from the package manager console:

```
PM> Install-Package MockQueryable.Core
```

Then [make own extension](https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable.Moq/MoqExtensions.cs) for your favorite mock framework
