using Core.Services;

namespace Core.Data.Services
{
    public class DbSecurityService : ISecurityService
    {
        public bool HasPrivilege(string privilegeName)
        {
            //TODO: this is just a mock
            return true;
        }
    }
}
