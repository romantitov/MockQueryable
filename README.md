
# MockQueryable 

[![MockQueryable.Core](https://img.shields.io/nuget/dt/MockQueryable.Core.svg)](https://www.nuget.org/packages/MockQueryable.Core/)
[![MockQueryable.EntityFrameworkCore](https://img.shields.io/nuget/dt/MockQueryable.EntityFrameworkCore.svg)](https://www.nuget.org/packages/MockQueryable.EntityFrameworkCore/)
[![MockQueryable.Moq](https://img.shields.io/nuget/dt/MockQueryable.Moq.svg)](https://www.nuget.org/packages/MockQueryable.Moq/)
[![MockQueryable.NSubstitute](https://img.shields.io/nuget/dt/MockQueryable.NSubstitute.svg)](https://www.nuget.org/packages/MockQueryable.NSubstitute/)
[![MockQueryable.FakeItEasy](https://img.shields.io/nuget/dt/MockQueryable.FakeItEasy.svg)](https://www.nuget.org/packages/MockQueryable.FakeItEasy/)

[![Build status](https://github.com/romantitov/MockQueryable/workflows/.NET%20Core/badge.svg)](https://github.com/romantitov/MockQueryable/actions)
[![Build status](https://ci.appveyor.com/api/projects/status/ggdbipcyyfb4av9e?svg=true)](https://ci.appveyor.com/project/handybudget/mockqueryable)
[![License](https://img.shields.io/github/license/romantitov/MockQueryable.svg)](https://github.com/romantitov/MockQueryable/blob/master/LICENSE)

[![GitHub Repo stars](https://img.shields.io/github/stars/romantitov/MockQueryable)](https://github.com/romantitov/MockQueryable/stargazers)
[![Version](https://badge.fury.io/gh/romantitov%2FMockQueryable.svg)](https://github.com/romantitov/MockQueryable/releases)
[![GitHub contributors](https://img.shields.io/github/contributors/romantitov/MockQueryable)](https://github.com/romantitov/MockQueryable/graphs/contributors)
[![GitHub last commit](https://img.shields.io/github/last-commit/romantitov/MockQueryable)](https://github.com/romantitov/MockQueryable)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/romantitov/MockQueryable)](https://github.com/romantitov/MockQueryable/graphs/commit-activity)
[![open issues](https://img.shields.io/github/issues/romantitov/MockQueryable)](https://github.com/romantitov/MockQueryable/issues)

------



Extensions for mocking [Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore/) (EFCore) operations such ToListAsync, FirstOrDefaultAsync etc. by [Moq](https://github.com/moq/moq), [NSubstitute](http://nsubstitute.github.io/) or [FakeItEasy](https://fakeiteasy.github.io/)
When writing tests for your application it is often desirable to avoid hitting the database. The extensions allow you to achieve this by creating a context – with behavior defined by your tests – that makes use of in-memory data.

### When should I use it?

If you have something similar to the following code: 
```csharp
var query = _userRepository.GetQueryable();

await query.AnyAsync(x =>...)
await query.FirstOrDefaultAsync(x =>...)
query.CountAsync(x => ...)
query.ToListAsync()
//etc.
```
and you want to cover it by unit tests

### How do I get started?

```csharp
//1 - create a List<T> with test items
var users = new List<UserEntity>()
{
  new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
  ...
};

//2 - build mock by extension
var mock = users.BuildMock();

//3 - setup the mock as Queryable for Moq
_userRepository.Setup(x => x.GetQueryable()).Returns(mock);

//3 - setup the mock as Queryable for NSubstitute
_userRepository.GetQueryable().Returns(mock);

//3 - setup the mock as Queryable for FakeItEasy
A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
```

Do you prefer *DbSet*? 

```csharp
//2 - build mock by extension
var mock = users.BuildMockDbSet();

//3 - setup DbSet for Moq
var userRepository = new TestDbSetRepository(mock.Object);

//3 - setup DbSet for NSubstitute or FakeItEasy
var userRepository = new TestDbSetRepository(mock);

//3 - setup the mock as Queryable for FakeItEasy
A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
```
### Can I extend the mock object created by MockQueryable with custom logic?
MockQueryable creates for your tests a mock object based on in-memory data, but you can also add some custome logic to it.

``` C#
var userId = Guid.NewGuid();
var users = new List<UserEntity>
{
    new UserEntity{Id = userId,LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
   //etc. 
};
var mock = users.BuildMockDbSet();

//Aditional setup for FindAsync
mock.Setup(x => x.FindAsync(userId)).ReturnsAsync((object[] ids) =>
{
    var id = (Guid)ids[0];
    return users.FirstOrDefault(x => x.Id == id);
});
var userRepository = new TestDbSetRepository(mock.Object);

//Execution FindAsync
var user = await ((DbSet<UserEntity>) userRepository.GetQueryable()).FindAsync(userId);
```

You can also add your custom expression visitor with custom logic:

```C#

var users = new List<UserEntity>
{
    new UserEntity{Id = userId,LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
    //etc. 
};

//Bould mock with custom  SampleLikeExpressionVisitor, that emulates EF.Functions.Like
var mockDbSet = users.BuildMockDbSet<UserEntity, SampleLikeExpressionVisitor>();
var userRepository = new TestDbSetRepository(mockDbSet.Object);

```

Check out the [sample project](https://github.com/romantitov/MockQueryable/tree/master/src/MockQueryable/MockQueryable.Sample)

### Where can I get it?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). 

If you are using **Moq** - then, install [MockQueryable.Moq](https://www.nuget.org/packages/MockQueryable.Moq/) from the package manager console:

```
PM> Install-Package MockQueryable.Moq
```

If you are using **NSubstitute** - then, install [MockQueryable.NSubstitute](https://www.nuget.org/packages/MockQueryable.NSubstitute/) from the package manager console:

```
PM> Install-Package MockQueryable.NSubstitute
```

If you are using **FakeItEasy** - then, install [MockQueryable.FakeItEasy](https://www.nuget.org/packages/MockQueryable.FakeItEasy/) from the package manager console:

```
PM> Install-Package MockQueryable.FakeItEasy
```

### Can I use it with my favorite mock framework?

You can install [MockQueryable.EntityFrameworkCore](https://www.nuget.org/packages/MockQueryable.EntityFrameworkCore/) from the package manager console:

```
PM> Install-Package MockQueryable.EntityFrameworkCore
```
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.EntityFrameworkCore.svg)](https://www.nuget.org/packages/MockQueryable.EntityFrameworkCore/)

or even [MockQueryable.Core](https://www.nuget.org/packages/MockQueryable.Core/)
```
PM> Install-Package MockQueryable.Core
```
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Core.svg)](https://www.nuget.org/packages/MockQueryable.Core/)


Then [make your own extension](https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable.Moq/MoqExtensions.cs) for your favorite mock framework


