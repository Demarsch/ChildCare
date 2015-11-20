namespace Core.Services
{
    public interface ISecurityService
    {
        bool HasPrivilege(string privilegeName);
    }
}