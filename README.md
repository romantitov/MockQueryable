
# MockQueryable 

[![Build status](https://github.com/romantitov/MockQueryable/workflows/.NET%20Core/badge.svg)](https://github.com/romantitov/MockQueryable/actions)
[![Build status](https://ci.appveyor.com/api/projects/status/ggdbipcyyfb4av9e?svg=true)](https://ci.appveyor.com/project/handybudget/mockqueryable)
[![Build Status](https://travis-ci.org/romantitov/MockQueryable.svg?branch=master)](https://travis-ci.org/romantitov/MockQueryable)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Moq.svg)](https://www.nuget.org/packages/MockQueryable.Moq/)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.NSubstitute.svg)](https://www.nuget.org/packages/MockQueryable.NSubstitute/)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.FakeItEasy.svg)](https://www.nuget.org/packages/MockQueryable.FakeItEasy/)


[![Build history](https://buildstats.info/appveyor/chart/handybudget/mockqueryable)](https://ci.appveyor.com/project/handybudget/mockqueryable/history)



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
var mock = users.AsQueryable().BuildMock();

//3 - setup the mock as Queryable for Moq
_userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);

//3 - setup the mock as Queryable for NSubstitute
_userRepository.GetQueryable().Returns(mock);

//3 - setup the mock as Queryable for FakeItEasy
A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
```

Do you prefer *DbSet*? 

```csharp
//2 - build mock by extension
var mock = users.AsQueryable().BuildMockDbSet();

//3 - setup DbSet for Moq
var userRepository = new TestDbSetRepository(mock.Object);

//3 - setup DbSet for NSubstitute or FakeItEasy
var userRepository = new TestDbSetRepository(mock);
```

Do you use *DbQuery*? 

```csharp
//2 - build mock by extension
var mock = users.AsQueryable().BuildMockDbQuery();

//3 - setup the mock as Queryable for Moq
_userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);

//3 - setup the mock as Queryable for NSubstitute
_userRepository.GetQueryable().Returns(mock);

//3 - setup the mock as Queryable for FakeItEasy
A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
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


Then [make own extension](https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable.Moq/MoqExtensions.cs) for your favorite mock framework
