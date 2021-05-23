using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockQueryable.Sample
{
    public class TestDbSetRepository : IUserRepository
    {
        private readonly DbSet<UserEntity> _dbSet;

        public TestDbSetRepository(DbSet<UserEntity> dbSet)
        {
            _dbSet = dbSet;
        }
        public IQueryable<UserEntity> GetQueryable()
        {
            return _dbSet;
        }

        public async Task CreateUser(UserEntity user)
        {
            await _dbSet.AddAsync(user);
        }

        public List<UserEntity> GetAll() {
            return _dbSet.AsQueryable().ToList();
        }

        public IAsyncEnumerable<UserEntity> GetAllAsync()
        {
            return _dbSet.AsAsyncEnumerable();
        }
    }
}