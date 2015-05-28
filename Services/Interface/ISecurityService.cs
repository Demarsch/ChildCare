namespace Core
{
    public interface ISecurityService
    {
        bool HasPrivilege(string privilegeName);
    }
}
