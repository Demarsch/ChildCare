using System;
using Core.DTO;

namespace Core.Misc
{
    public interface IEnvironment
    {
        UserDTO CurrentUser { get; }

        DateTime CurrentDate { get; }
    }
}
