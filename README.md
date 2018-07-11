
# MockQueryable

[![Build status](https://ci.appveyor.com/api/projects/status/ggdbipcyyfb4av9e?svg=true)](https://ci.appveyor.com/project/handybudget/mockqueryable)
[![Build Status](https://travis-ci.org/romantitov/MockQueryable.svg?branch=master)](https://travis-ci.org/romantitov/MockQueryable)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Moq.svg)](https://www.nuget.org/packages/MockQueryable.Moq/)
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.NSubstitute.svg)](https://www.nuget.org/packages/MockQueryable.NSubstitute/)



Extensions for mocking [Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore/) operations such ToListAsync, FirstOrDefaultAsync etc. by [Moq](https://github.com/moq/moq) or [NSubstitute](http://nsubstitute.github.io/)
When writing tests for your application it is often desirable to avoid hitting the database. The extensions allow you to achieve this by creating a context – with behavior defined by your tests – that makes use of in-memory data.

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

### Can I use it with my favorite mock framework?

You can install [MockQueryable.Core](https://www.nuget.org/packages/MockQueryable.Core/) from the package manager console:

```
PM> Install-Package MockQueryable.Core
```
[![Downloads](https://img.shields.io/nuget/dt/MockQueryable.Core.svg)](https://www.nuget.org/packages/MockQueryable.Core/)

Then [make own extension](https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable.Moq/MoqExtensions.cs) for your favorite mock framework
