using QuantityMeasurementApp.Models;

namespace QuantityMeasurementApp.Repository
{
    public interface IUserRepository
    {
        void AddUser(UserEntity user);
        UserEntity? GetUserByEmail(string email);
    }
}
