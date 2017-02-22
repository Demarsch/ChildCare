using System.Collections.Generic;
namespace Core.Data.Services
{
    public interface IUserService
    {
        User GetCurrentUser();

        int GetCurrentUserId();

        string GetCurrentUserSID();

        IEnumerable<int> GetCurrentUserPersonStaffIds();

        IEnumerable<int> GetCurrentUserStaffIds();

        bool HasPersonStaff(int personStaffId);

        bool HasStaff(int staffId);

        string GetCurrentUserSettingsValue(string parameterName);

        void SetCurrentUserSettingsValue(string parameterName, string value);
    }
}
