using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace MockQueryable.Sample
{
	[TestFixture]
    public class MyServiceMoqTests
	{

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
				new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "ExistFirstName"},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
			};
			//expect
			var mock = users.AsQueryable().BuildMock();
			userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
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
		    var userRepository = new Mock<IUserRepository>();
		    var service = new MyService(userRepository.Object);
            var users = new List<UserEntity>
			{
				new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012")},
				new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018")},
			};
			//expect
			var mock = users.AsQueryable().BuildMock();
			userRepository.Setup(x => x.GetQueryable()).Returns(mock.Object);
			//act
			var result = await service.GetUserReports(from, to);
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
                new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{FirstName = "ExistFirstName"},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
            };
            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock.Object);
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
	        var mock = new List<UserEntity>().AsQueryable().BuildMockDbSet();
	        var userRepository = new TestDbSetRepository(mock.Object);
	        var service = new MyService(userRepository);
	        //act
	        await service.CreateUserIfNotExist(firstName, lastName, dateOfBirth);
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
                new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
                new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012")},
                new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018")},
            };
            var mock = users.AsQueryable().BuildMockDbSet();
            var userRepository = new TestDbSetRepository(mock.Object);
            var service = new MyService(userRepository);
            //act
            var result = await service.GetUserReports(from, to);
            //assert
            Assert.AreEqual(expectedCount, result.Count);
        }

    }
}
