using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using MockQueryable.FakeItEasy;
using NUnit.Framework;

namespace MockQueryable.Sample
{
    [TestFixture]
    public class MyServiceFakeItEasyTests
    {
        private static readonly CultureInfo UsCultureInfo = new CultureInfo("en-US");

        [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
        [TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
        [TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
        public void CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
        {
            //arrange

            var userRepository = A.Fake<IUserRepository>();
            var service = new MyService(userRepository);
            var users = new List<UserEntity>
            {
                new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "ExistFirstName"},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
            };
            //expect
            var mock = users.AsQueryable().BuildMock();
            A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
            //act
            var ex = Assert.ThrowsAsync<ApplicationException>(() => service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
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
            var userRepository = A.Fake<IUserRepository>();
            var service = new MyService(userRepository);
            var users = new List<UserEntity>
            {
                new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018",UsCultureInfo.DateTimeFormat)},
            };
            //expect
            var mock = users.AsQueryable().BuildMock();
            A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
            //act
            var result = await service.GetUserReports(from, to);
            //assert
            Assert.AreEqual(expectedCount, result.Count);

        }

        [TestCase("01/20/2012", "06/20/2018", 5)]
        [TestCase("01/20/2012", "06/20/2012", 4)]
        [TestCase("01/20/2012", "02/20/2012", 3)]
        [TestCase("01/20/2010", "02/20/2011", 0)]
        public async Task GetUserReports_DbQuery(DateTime from, DateTime to, int expectedCount)
        {
            //arrange
            var userRepository = A.Fake<IUserRepository>();
            var service = new MyService(userRepository);
            var users = new List<UserEntity>
            {
                new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018",UsCultureInfo.DateTimeFormat)},
            };
            //expect
            var mock = users.AsQueryable().BuildMockDbQuery();
            A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
            //act
            var result = await service.GetUserReports(from, to);
            //assert
            Assert.AreEqual(expectedCount, result.Count);

        }

        //[TestCase("01/20/2012", "06/20/2018", 5)]
        //[TestCase("01/20/2012", "06/20/2012", 4)]
        //[TestCase("01/20/2012", "02/20/2012", 3)]
        [TestCase("01/20/2010", "02/20/2011", 0)]
        public async Task GetUserReports_AutoMap(DateTime from, DateTime to, int expectedCount)
        {
            //arrange
            var userRepository = A.Fake<IUserRepository>();
            var service = new MyService(userRepository);
            var users = new List<UserEntity>
            {
                new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018",UsCultureInfo.DateTimeFormat)},
            };
            //expect
            var mock = users.AsQueryable().BuildMock();
            A.CallTo(() => userRepository.GetQueryable()).Returns(mock);
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
                new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "ExistFirstName"},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
            };
            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock);
            var service = new MyService(userRepository);
            //act
            var ex = Assert.ThrowsAsync<ApplicationException>(() => service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
            //assert
            Assert.AreEqual(expectedError, ex.Message);

        }

        [TestCase("AnyFirstName", "ExistLastName", "01/20/2012")]
        public async Task DbSetCreateUser(string firstName, string lastName, DateTime dateOfBirth)
        {
            //arrange
            var userEntities = new List<UserEntity>();
            var mock = userEntities.AsQueryable().BuildMockDbSet();
            A.CallTo(() => mock.AddAsync(A<UserEntity>._, A<CancellationToken>._))
             .ReturnsLazily(call =>
             {
                 userEntities.Add((UserEntity) call.Arguments[0]);
                 return default;
             });
            var userRepository = new TestDbSetRepository(mock);
            var service = new MyService(userRepository);
            //act
            await service.CreateUserIfNotExist(firstName, lastName, dateOfBirth);
            // assert
            var entity = mock.Single();
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
            var users = new List<UserEntity>
            {
                new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012",UsCultureInfo.DateTimeFormat)},
                new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018",UsCultureInfo.DateTimeFormat)},
            };
            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock);
            var service = new MyService(userRepository);
            //act
            var result = await service.GetUserReports(from, to);
            //assert
            Assert.AreEqual(expectedCount, result.Count);
        }


    }
}
