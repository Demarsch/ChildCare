namespace Core.Data.Services
{
    public interface IUserService
    {
        User GetCurrentUser();

        string GetCurrentUserSID();
    }
}
