using System;

namespace Core
{
    public interface IEnvironment
    {
        UserDTO CurrentUser { get; }

        DateTime CurrentDate { get; }
    }
}
