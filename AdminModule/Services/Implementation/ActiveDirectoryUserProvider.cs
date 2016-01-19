using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using AdminModule.Model;
using Core.Extensions;
using Core.Misc;

namespace AdminModule.Services
{
    public class ActiveDirectoryUserProvider : IUserProvider
    {
        public async Task<ICollection<UserInfo>> SearchUsersAsync(string searchPattern)
        {
            await Task.Delay(1000);
            return await Task.Factory.StartNew(x => SearchUsers(x.ToString()), searchPattern);
        }

        private ICollection<UserInfo> SearchUsers(string searchPattern)
        {
            return new UserInfo[]
                   {
                       new UserInfo { FullName = "Petrov Petr Petrovich", Login = "XXX\\Login", Sid = "YYYY" },
                       new UserInfo { FullName = "Ivanov Ivanische Ivanovich", Login = "XXX\\AnotherLogin", Sid = "UUUU" }
                   };
            var words = searchPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new PrincipalSearcher(new UserPrincipal(new PrincipalContext(ContextType.Domain)))
                .FindAll()
                .Where(x => x is UserPrincipal
                            && x.StructuralObjectClass == "user"
                            && !string.IsNullOrEmpty(x.UserPrincipalName))
                .Select(x => new
                {
                    UserInfo = new UserInfo { Login = x.UserPrincipalName, Sid = x.Sid.Value, FullName = x.Name },
                    Likeness = words.Count(y => x.UserPrincipalName.CaseInsensitiveContains(y) || x.Name.CaseInsensitiveContains(y))
                })
                .Where(x => x.Likeness >= words.Length / 2)
                .OrderByDescending(x => x.Likeness)
                .Take(AppConfiguration.SearchResultTakeTopCount)
                .Select(x => x.UserInfo)
                .ToArray();
        }
    }
}