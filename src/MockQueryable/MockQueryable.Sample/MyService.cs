using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace MockQueryable.Sample
{
	public class MyService
	{
		private readonly IUserRepository _userRepository;

	    public static void Initialize()
	    {
	        Mapper.Initialize(cfg => cfg.CreateMap<UserEntity, UserReport>()
	                                 .ForMember(dto => dto.FirstName, conf => conf.MapFrom(ol => ol.FirstName))
	                                 .ForMember(dto => dto.LastName, conf => conf.MapFrom(ol => ol.LastName)));
            Mapper.Configuration.AssertConfigurationIsValid();
	    }

	    public MyService(IUserRepository userRepository)
		{
			this._userRepository = userRepository;
		}

		public async Task CreateUserIfNotExist(string firstName, string lastName, DateTime dateOfBirth)
		{
			var query = _userRepository.GetQueryable();

			if (await query.AnyAsync(x => x.LastName == lastName && x.DateOfBirth==dateOfBirth))
			{
				throw new ApplicationException("User already exist");
			}

			var existUser = await query.FirstOrDefaultAsync(x => x.FirstName == firstName);
			if (existUser != null)
			{
				throw new ApplicationException("User with FirstName already exist");
			}

			if (await query.CountAsync(x => x.DateOfBirth == dateOfBirth.Date) > 3)
			{
				throw new ApplicationException("Users with DateOfBirth more than limit");
			}

			await _userRepository.CreateUser(new UserEntity
			{
				FirstName = firstName,
				LastName = lastName,
				DateOfBirth = dateOfBirth.Date,
			});

		}
        
		public async Task<List<UserReport>> GetUserReports(DateTime dateFrom, DateTime dateTo)
		{
			var query = _userRepository.GetQueryable();

			query = query.Where(x => x.DateOfBirth >= dateFrom.Date);
			query = query.Where(x => x.DateOfBirth <= dateTo.Date);

			return await query.Select(x => new UserReport
			{
				FirstName = x.FirstName,
				LastName = x.LastName,
			}).ToListAsync();
		}


	    public async Task<List<UserReport>> GetUserReportsAutoMap(DateTime dateFrom, DateTime dateTo)
	    {
	        var query = _userRepository.GetQueryable();

	        query = query.Where(x => x.DateOfBirth >= dateFrom.Date);
	        query = query.Where(x => x.DateOfBirth <= dateTo.Date);

	        return await query.ProjectTo<UserReport>().ToListAsync();
	    }
    }

	public interface IUserRepository
	{
		IQueryable<UserEntity> GetQueryable();

		Task CreateUser(UserEntity user);

		List<UserEntity> GetAll();
		
		IAsyncEnumerable<UserEntity> GetAllAsync();
	}


	public class UserReport
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	public class UserEntity
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime DateOfBirth { get; set; }
	}
}