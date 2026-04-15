using QuantityMeasurementApp.Models;
using System.Linq;

namespace QuantityMeasurementApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly QuantityMeasurementDbContext _context;

        public UserRepository(QuantityMeasurementDbContext context)
        {
            _context = context;
        }

        public void AddUser(UserEntity user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public UserEntity? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}
