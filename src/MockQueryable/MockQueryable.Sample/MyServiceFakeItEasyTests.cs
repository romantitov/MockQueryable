using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using MockQueryable.FakeItEasy;
using NUnit.Framework;

namespace MockQueryable.Sample
{
  [TestFixture]
  public class MyServiceFakeItEasyTests
  {
    private static readonly CultureInfo UsCultureInfo = new("en-US");

    [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
    [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
    public void CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
    {
      // arrange
      var userRepository = A.Fake<IUserRepository>();
      var service = new MyService(userRepository);
      var users = new List<UserEntity>
      {
        new() {LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {FirstName = "ExistFirstName"},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
      };

      // expect
      var mock = users.BuildMock();
      A.CallTo(() => userRepository.GetQueryable()).Returns(mock);

      // act
      var ex = Assert.ThrowsAsync<ApplicationException>(() =>
        service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));

      // assert
      Assert.That(expectedError, Is.EqualTo(ex.Message));
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task GetUserReports(DateTime from, DateTime to, int expectedCount)
    {
      //arrange
      var userRepository = A.Fake<IUserRepository>();
      var service = new MyService(userRepository);
      var users = CreateUserList();
      //expect
      var mock = users.BuildMock();
      A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
      //act
      var result = await service.GetUserReports(from, to);
      //assert
      Assert.That(expectedCount, Is.EqualTo(result.Count));
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task GetUserReports_AutoMap(DateTime from, DateTime to, int expectedCount)
    {
      // arrange
      var userRepository = A.Fake<IUserRepository>();
      var service = new MyService(userRepository);
      var users = CreateUserList();
      // expect
      var mock = users.BuildMock();
      A.CallTo(() => userRepository.GetQueryable()).Returns(mock);

      // act
      var result = await service.GetUserReportsAutoMap(from, to);

      // assert
      Assert.That(expectedCount, Is.EqualTo(result.Count));
    }

    [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
    [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
    public void DbSetCreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
    {
      // arrange
      var users = new List<UserEntity>
      {
        new() {LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {FirstName = "ExistFirstName"},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
      };
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);

      // act
      var ex = Assert.ThrowsAsync<ApplicationException>(() =>
        service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));

      // assert
      Assert.That(expectedError, Is.EqualTo(ex.Message));
    }

    [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
    [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
    public void DbSetCreatedFromCollectionCreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
    {
      // arrange
      var users = new List<UserEntity>
        {
            new() {LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
            new() {FirstName = "ExistFirstName"},
            new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
            new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
            new() {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        };
      var mock = users.BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);
      // act
      var ex = Assert.ThrowsAsync<ApplicationException>(() =>
        service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
      // assert
      Assert.That(expectedError, Is.EqualTo(ex.Message));
    }

    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012")]
    public async Task DbSetCreateUser(string firstName, string lastName, DateTime dateOfBirth)
    {
      // arrange
      var userEntities = new List<UserEntity>();
      var mock = userEntities.AsQueryable().BuildMockDbSet();
      A.CallTo(() => mock.AddAsync(A<UserEntity>._, A<CancellationToken>._))
        .ReturnsLazily(call =>
        {
          userEntities.Add((UserEntity)call.Arguments[0]);
          return default;
        });
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);

      // act
      await service.CreateUserIfNotExist(firstName, lastName, dateOfBirth);

      // assert
      var entity = mock.Single();
      Assert.That(firstName, Is.EqualTo(entity.FirstName));
      Assert.That(lastName, Is.EqualTo(entity.LastName));
      Assert.That(dateOfBirth, Is.EqualTo(entity.DateOfBirth));
    }

    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012")]
    public async Task DbSetCreatedFromCollectionCreateUser1(string firstName, string lastName, DateTime dateOfBirth)
    {
      // arrange
      var userEntities = new List<UserEntity>();
      var mock = userEntities.BuildMockDbSet();
      A.CallTo(() => mock.AddAsync(A<UserEntity>._, A<CancellationToken>._))
       .ReturnsLazily(call =>
       {
         userEntities.Add((UserEntity)call.Arguments[0]);
         return default;
       });
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);

      // act
      await service.CreateUserIfNotExist(firstName, lastName, dateOfBirth);

      // assert
      var entity = mock.Single();
      Assert.That(firstName, Is.EqualTo(entity.FirstName));
      Assert.That(lastName, Is.EqualTo(entity.LastName));
      Assert.That(dateOfBirth, Is.EqualTo(entity.DateOfBirth));
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task DbSetGetUserReports(DateTime from, DateTime to, int expectedCount)
    {
      // arrange
      var users = CreateUserList();
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);

      // act
      var result = await service.GetUserReports(from, to);

      // assert
      Assert.That(expectedCount, Is.EqualTo(result.Count));
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task DbSetCreatedFromCollectionGetUserReports(DateTime from, DateTime to, int expectedCount)
    {
      // arrange
      var users = CreateUserList();
      var mock = users.BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);
      var service = new MyService(userRepository);

      // act
      var result = await service.GetUserReports(from, to);

      // assert
      Assert.That(expectedCount, Is.EqualTo(result.Count));
    }

    [TestCase]
    public async Task DbSetGetAllUserEntitiesAsync()
    {
      // arrange
      var users = CreateUserList();

      var mockDbSet = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mockDbSet);

      // act
      var result = await userRepository.GetAllAsync().ToListAsync();

      // assert
      Assert.That(users.Count, Is.EqualTo(result.Count));
    }

    [TestCase]
    public async Task DbSetCreatedFromCollectionGetAllUserEntitiesAsync()
    {
      // arrange
      var users = CreateUserList();

      var mockDbSet = users.BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mockDbSet);

      // act
      var result = await userRepository.GetAllAsync().ToListAsync();

      // assert
      Assert.That(users.Count, Is.EqualTo(result.Count));
    }

    [TestCase]
    public async Task DbSetToListAsyncAsync_ShouldReturnAllEntities_WhenSourceIsChanged()
    {
      // arrange
      var users = new List<UserEntity>();

      var mockDbSet = users.AsQueryable().BuildMockDbSet();

      // act
      var result1 = await mockDbSet.ToListAsync();
      users.AddRange(CreateUserList());
      var result2 = await mockDbSet.ToListAsync();

      // assert
      Assert.That(0, Is.EqualTo(result1.Count));
      Assert.That(users.Count, Is.EqualTo(result2.Count));
    }

    [TestCase]
    public async Task DbSetGetAllUserEntity()
    {
      //arrange
      var users = CreateUserList();
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);

      //act
      var result = await userRepository.GetAll();

      //assert
      Assert.That(users.Count, Is.EqualTo(result.Count));
    }

    [TestCase]
    public async Task DbSetCreatedFromCollectionGetAllUserEntity()
    {
      // arrange
      var users = CreateUserList();
      var mock = users.BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock);

      // act
      var result = await userRepository.GetAll();

      // assert
      Assert.That(users.Count, Is.EqualTo(result.Count));
    }

    [TestCase]
    public void GetUsersByFirstName_ExpressionVisitorMissing_ThrowsException()
    {
      // arrange
      var users = CreateUserList();

      var mockDbSet = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mockDbSet);

      // act
      var exception = Assert.ThrowsAsync<InvalidOperationException>(() => userRepository.GetUsersByFirstName("naME"));

      // assert
      Assert.That(
        exception.Message,
        Is.EqualTo(
          "The 'ILike' method is not supported because the query has switched to client-evaluation. " +
          "This usually happens when the arguments to the method cannot be translated to server. " +
          "Rewrite the query to avoid client evaluation of arguments so that method can be translated to server."));
    }

    [TestCase]
    public async Task GetUsersByFirstName_PartOfNameCaseInsensitiveSearch_AllMatchesReturned()
    {
      // arrange
      var users = CreateUserList();

      var mockDbSet = users.AsQueryable().BuildMockDbSet<UserEntity, SampleILikeExpressionVisitor>();
      var userRepository = new TestDbSetRepository(mockDbSet);

      // act
      var result = await userRepository.GetUsersByFirstName("naME");

      // assert
      Assert.That(users.Count, Is.EqualTo(result.Count()));
    }

    private static List<UserEntity> CreateUserList() => new()
    {
      new UserEntity
      {
        FirstName = "FirstName1", LastName = "LastName",
        DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
      },
      new UserEntity
      {
        FirstName = "FirstName2", LastName = "LastName",
        DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
      },
      new UserEntity
      {
        FirstName = "FirstName3", LastName = "LastName",
        DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
      },
      new UserEntity
      {
        FirstName = "FirstName3", LastName = "LastName",
        DateOfBirth = DateTime.Parse("03/20/2012", UsCultureInfo.DateTimeFormat)
      },
      new UserEntity
      {
        FirstName = "FirstName5", LastName = "LastName",
        DateOfBirth = DateTime.Parse("01/20/2018", UsCultureInfo.DateTimeFormat)
      },
    };
  }
}
