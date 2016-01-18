using System.Collections.Generic;
using System.Threading.Tasks;
using AdminModule.Model;

namespace AdminModule.Services
{
    public interface IUserProvider
    {
        Task<IEnumerable<UserInfo>> SearchUsersAsync(string searchPattern);
    }
}
