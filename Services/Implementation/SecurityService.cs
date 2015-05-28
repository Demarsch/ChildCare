namespace Core
{
    public class SecurityService : ISecurityService
    {
        private readonly bool hasPrivilegeResponse;

        public SecurityService(bool proposedResponse)
        {
            hasPrivilegeResponse = proposedResponse;
        }

        public bool HasPrivilege(string privilegeName)
        {
            return hasPrivilegeResponse;
        }
    }
}