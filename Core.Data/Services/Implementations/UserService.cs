using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

namespace Core.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextProvider contextProvider;

        public UserService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        private string GetCurrentSID()
        {
            return UserPrincipal.Current.Sid.ToString();
        }

        public List<string> GetCurrentUserPermissions()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentSID();
                return StackRoles(new List<Permission>(), db.Set<User>().Where(z => z.SID != null && z.SID.ToLower() == curSID).SelectMany(x => x.UserPermissions.Select(y => new { y.Permission, y.IsGranted })).ToList()
                    .Select(x => new Tuple<Permission, bool>(x.Permission, x.IsGranted)).ToList()).Select(x => x.Name).ToList();
            }
        }

        private List<Tuple<Permission, bool>> DeepRun(List<Tuple<Permission, bool>> roles)
        {
            List<Tuple<Permission, bool>> ret = new List<Tuple<Permission, bool>>();
            foreach (Tuple<Permission, bool> rl in roles)
            {
                ret.Add(rl);
                if (rl.Item2)
                    ret.AddRange(DeepRun(rl.Item1.PermissionLinks.Select(x => new Tuple<Permission, bool>(x.Permission1, rl.Item2)).ToList()));
            }
            return ret;
        }

        private List<Permission> StackRoles(List<Permission> has, List<Tuple<Permission, bool>> roles)
        {
            List<Tuple<Permission, bool>> income = DeepRun(roles);
            List<Permission> deniedPermissions = income.Where(x => !x.Item2).Select(x => x.Item1).ToList();
            return has.Union(income.Select(x => x.Item1)).Where(x => !deniedPermissions.Contains(x)).ToList();
        }
    }
}
