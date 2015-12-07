using System;

namespace Core.Data.Services
{
    public interface IEnvironment
    {
        User CurrentUser { get; }

        DateTime CurrentDate { get; }
    }
}
