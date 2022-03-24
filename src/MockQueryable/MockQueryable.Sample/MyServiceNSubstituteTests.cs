using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MockQueryable.Sample
{
    [TestFixture]
    public class MyServiceNSubstituteTests
    {
        private static readonly CultureInfo UsCultureInfo = new CultureInfo("en-US");


        [TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
		[TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
		[TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
		public void CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
		{
            //arrange
		    var userRepository = Substitute.For<IUserRepository>();
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
			var mock = users.BuildMock();
			userRepository.GetQueryable().Returns(mock);
			//act
			var ex= Assert.ThrowsAsync<ApplicationException>(() => service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
			//assert
			Assert.AreEqual(expectedError, ex.Message);

		}

        [TestCase("01/20/2012", "06/20/2018",5)]
		[TestCase("01/20/2012", "06/20/2012",4)]
		[TestCase("01/20/2012", "02/20/2012",3)]
		[TestCase("01/20/2010", "02/20/2011",0)]
		public async Task GetUserReports(DateTime from, DateTime to, int expectedCount)
		{
            //arrange
		    var userRepository = Substitute.For<IUserRepository>();
		    var service = new MyService(userRepository);
            List<UserEntity> users = CreateUserList();

            //expect
            var mock = users.BuildMock();
			userRepository.GetQueryable().Returns(mock);
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
            List<UserEntity> users = CreateUserList();

            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock);
            var service = new MyService(userRepository);
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
            mock.AddAsync(Arg.Any<UserEntity>())
                .Returns(info => null)
                .AndDoes(info => userEntities.Add(info.Arg<UserEntity>()));
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
            var users = CreateUserList();

            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock);
            var service = new MyService(userRepository);
            //act
            var result = await service.GetUserReports(from, to);
            //assert
            Assert.AreEqual(expectedCount, result.Count);
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
            Assert.AreEqual(users.Count, result.Count);
        }

        [TestCase]
        public async Task DbSetGetOneUserTntityAsync()
        {
            // arrange
            var users = CreateUserList();

            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mockDbSet);

            // act
            var result = await userRepository.GetAllAsync()
                .Where(user => user.FirstName == "FirstName1")
                .FirstOrDefaultAsync();

            // assert
            Assert.AreEqual(users.First(), result);
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
