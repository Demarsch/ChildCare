using System.Collections.Generic;
using System;

namespace Core.Data.Services
{
    public interface IUserService
    {
        List<string> GetCurrentUserPermissions();
    }
}
