using System;

namespace Core.Misc
{
    public interface ITimeInterval
    {
        TimeSpan StartTime { get; }
        
        TimeSpan EndTime { get; }
    }
}
