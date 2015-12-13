namespace Core.Services
{
    public interface ISecurityService
    {
        bool HasPermission(string privilegeName);
    }
}