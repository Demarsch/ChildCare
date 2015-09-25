using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;
using System.DirectoryServices.AccountManagement;

namespace Core
{
    public class ADUserSystemInfoService : IUserSystemInfoService
    {
        public string GetCurrentSID()
        {
            return UserPrincipal.Current.Sid.ToString();
        }

        public IList<UserSystemInfo> Find(string someUserName)
        {
            string str = someUserName.ToUpper();
            List<UserSystemInfo> result = new List<UserSystemInfo>();
            try
            {
                foreach (var found in new PrincipalSearcher(new UserPrincipal(new PrincipalContext(ContextType.Domain))).FindAll())
                {
                    UserPrincipal user = found as UserPrincipal;
                    if (user == null || user.Name == string.Empty || user.UserPrincipalName == null || user.StructuralObjectClass != "user") continue;
                    if (user.Name.ToUpper().Contains(str) || user.UserPrincipalName.ToUpper().Contains(str))
                        result.Add(new UserSystemInfo
                        {
                            DisplayName = user.Name + " (" + user.UserPrincipalName + ")",
                            Enabled = (user.Enabled == true),
                            PrincipalName = user.UserPrincipalName,
                            UserName = user.Name,
                            SID = user.Sid.ToString()
                        });
                }
            }
            catch { };
            return result;
        }

        public UserSystemInfo GetBySID(string SID)
        {
            var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), IdentityType.Sid, SID);
            if (user == null) return null;
            return new UserSystemInfo
            {
                DisplayName = user.Name + " (" + user.UserPrincipalName + ")",
                Enabled = (user.Enabled == true),
                PrincipalName = user.UserPrincipalName,
                UserName = user.Name,
                SID = user.Sid.ToString()
            };
        }

    }
}
