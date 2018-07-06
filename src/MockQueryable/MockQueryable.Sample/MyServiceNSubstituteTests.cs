using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;

namespace MockQueryable.Sample
{
	[TestFixture]
    public class MyServiceNSubstituteTests
    {
		private IUserRepository _userRepository;
		private MyService _service;
		[SetUp]
	    public void Init()
	    {
			_userRepository = Substitute.For<IUserRepository>();
		    _service = new MyService(_userRepository);
		}

		[TestCase("AnyFirstName", "AnyExistLastName", "01/20/2012", "Users with DateOfBirth more than limit")]
		[TestCase("ExistFirstName", "AnyExistLastName", "02/20/2012", "User with FirstName already exist")]
		[TestCase("AnyFirstName", "ExistLastName", "01/20/2012", "User already exist")]
		public void CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth, string expectedError)
		{
			//arrange
			var users = new List<UserEntity>()
			{
				new UserEntity{LastName = "ExistLastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "ExistFirstName"},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{DateOfBirth = DateTime.Parse("01/20/2012")},
			};
			//expect
			var mock = users.AsQueryable().BuildMock();
			_userRepository.GetQueryable().Returns(mock);
			//act
			var ex= Assert.ThrowsAsync<ApplicationException>(() => _service.CreateUserIfNotExist(firstName, lastName, dateOfBirth));
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
			var users = new List<UserEntity>()
			{
				new UserEntity{FirstName = "FirstName1", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "FirstName2", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},
				new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2012")},

				new UserEntity{FirstName = "FirstName3", LastName = "LastName", DateOfBirth = DateTime.Parse("03/20/2012")},

				new UserEntity{FirstName = "FirstName5", LastName = "LastName", DateOfBirth = DateTime.Parse("01/20/2018")},
			};
			//expect
			var mock = users.AsQueryable().BuildMock();
			_userRepository.GetQueryable().Returns(mock);
			//act
			var result = await _service.GetUserReports(from, to);
			//assert
			Assert.AreEqual(expectedCount, result.Count);
		}

	}
}
