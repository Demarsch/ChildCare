using System.Collections.Generic;
using System.Threading.Tasks;
using AdminModule.Model;

namespace AdminModule.Services
{
    public interface IUserProvider
    {
        Task<ICollection<UserInfo>> SearchUsersAsync(string searchPattern);
    }
}
