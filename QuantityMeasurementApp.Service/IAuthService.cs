namespace QuantityMeasurementApp.Service
{
    public interface IAuthService
    {
        string Register(string name, string email, string password);
        string Login(string email, string password);
    }
}
