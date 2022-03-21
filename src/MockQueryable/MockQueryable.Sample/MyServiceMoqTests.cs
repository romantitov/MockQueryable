﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace MockQueryable.Sample
{
  [TestFixture]
  public class MyServiceMoqTests
  {
    private static readonly CultureInfo UsCultureInfo = new CultureInfo("en-US");

    [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
    [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
    public void CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
    {
      //arrange
      var userRepository = new Mock<IUserRepository>();
      var service = new MyService(userRepository.Object);
      var users = new List<UserEntity>
      {
        new UserEntity {LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {FirstName = "ExistFirstName"},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)}
      };
      //expect
      var mock = users.AsQueryable().BuildMock();
      userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
      //act
      var ex = Assert.ThrowsAsync<ApplicationException>(() =>
        service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
      //assert
      Assert.AreEqual(expectedError, ex.Message);
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task GetUserReports(DateTime from, DateTime to, int expectedCount)
    {
      //arrange
      var userRepository = new Mock<IUserRepository>();
      var service = new MyService(userRepository.Object);
      var users = CreateUserList();
      //expect
      var mock = users.AsQueryable().BuildMock();
      userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
      //act
      var result = await service.GetUserReports(from, to);
      //assert
      Assert.AreEqual(expectedCount, result.Count);
    }



    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task GetUserReports_AutoMap(DateTime from, DateTime to, int expectedCount)
    {
      //arrange
      var userRepository = new Mock<IUserRepository>();
      var service = new MyService(userRepository.Object);
      var users = CreateUserList();
      //expect
      var mock = users.AsQueryable().BuildMock();
      userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
      //act
      var result = await service.GetUserReportsAutoMap(from, to);
      //assert
      Assert.AreEqual(expectedCount, result.Count);
    }


    [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
    [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
    public void DbSetCreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
    {
      //arrange
      var users = new List<UserEntity>
      {
        new UserEntity {LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {FirstName = "ExistFirstName"},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)},
        new UserEntity {DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)}
      };
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock.Object);
      var service = new MyService(userRepository);
      //act
      var ex = Assert.ThrowsAsync<ApplicationException>(() =>
        service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
      //assert
      Assert.AreEqual(expectedError, ex.Message);
    }

    [TestCase("AnyFirstName", "ExistLastName", "01/20/2012")]
    public async Task DbSetCreateUser(string firstName, string lastName, DateTime dateOfBirth)
    {
      //arrange
      var userEntities = new List<UserEntity>();
      var mock = userEntities.AsQueryable().BuildMockDbSet();
      mock.Setup(set => set.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
        .Callback((UserEntity entity, CancellationToken _) => userEntities.Add(entity));
      var userRepository = new TestDbSetRepository(mock.Object);
      var service = new MyService(userRepository);
      //act
      await service.CreateUserIfNotExist(firstName, lastName, dateOfBirth);
      // assert
      var entity = mock.Object.Single();
      Assert.AreEqual(firstName, entity.FirstName);
      Assert.AreEqual(lastName, entity.LastName);
      Assert.AreEqual(dateOfBirth, entity.DateOfBirth);
    }

    [TestCase("01/20/2012", "06/20/2018", 5)]
    [TestCase("01/20/2012", "06/20/2012", 4)]
    [TestCase("01/20/2012", "02/20/2012", 3)]
    [TestCase("01/20/2010", "02/20/2011", 0)]
    public async Task DbSetGetUserReports(DateTime from, DateTime to, int expectedCount)
    {
      //arrange
      var users = CreateUserList();
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock.Object);
      var service = new MyService(userRepository);
      //act
      var result = await service.GetUserReports(from, to);
      //assert
      Assert.AreEqual(expectedCount, result.Count);
    }

    [TestCase]
    public void DbSetGetAllUserEntity()
    {
      //arrange
      var users = CreateUserList();
      var mock = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mock.Object);
      //act
      var result = userRepository.GetAll();
      //assert
      Assert.AreEqual(users.Count, result.Count);
    }

    [TestCase]
    public async Task DbSetFindAsyncUserEntity()
    {
      //arrange
      var userId = Guid.NewGuid();
      var users = new List<UserEntity>
      {
        new UserEntity
        {
          Id = Guid.NewGuid(),
          FirstName = "FirstName1", LastName = "LastName",
          DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },
        new UserEntity
        {
          Id = Guid.NewGuid(),
          FirstName = "FirstName2", LastName = "LastName",
          DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },
        new UserEntity
        {
          Id = userId,
          FirstName = "FirstName3", LastName = "LastName",
          DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },
        new UserEntity
        {
          Id = Guid.NewGuid(),
          FirstName = "FirstName3", LastName = "LastName",
          DateOfBirth = DateTime.Parse("03/20/2012", UsCultureInfo.DateTimeFormat)
        },
        new UserEntity
        {
          Id = Guid.NewGuid(),
          FirstName = "FirstName5", LastName = "LastName",
          DateOfBirth = DateTime.Parse("01/20/2018", UsCultureInfo.DateTimeFormat)
        }
      };

      var mock = users.AsQueryable().BuildMockDbSet();
      mock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) =>
      {
        var id = (Guid) ids.First();
        return users.FirstOrDefault(x => x.Id == id);
      });
      var userRepository = new TestDbSetRepository(mock.Object);

      //act
      var result = await ((DbSet<UserEntity>)userRepository.GetQueryable()).FindAsync(userId);

      //assert
      Assert.IsNotNull(result);
      Assert.AreEqual("FirstName3", result.FirstName);
    }

    [TestCase]
    public async Task DbSetGetAllUserEntitiesAsync()
    {
      // arrange
      var users = CreateUserList();

      var mockDbSet = users.AsQueryable().BuildMockDbSet();
      var userRepository = new TestDbSetRepository(mockDbSet.Object);

      // act
      var result = await userRepository.GetAllAsync().ToListAsync();

      // assert
      Assert.AreEqual(users.Count, result.Count);
    }

    private static List<UserEntity> CreateUserList() => new List<UserEntity>
    {
      new UserEntity { FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat) },
      new UserEntity { FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat) },
      new UserEntity { FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat) },
      new UserEntity { FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012", UsCultureInfo.DateTimeFormat) },
      new UserEntity { FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018", UsCultureInfo.DateTimeFormat) },
    };
  }
}