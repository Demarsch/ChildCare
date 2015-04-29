using System;

namespace Core
{
    public interface ITimeInterval
    {
        TimeSpan StartTime { get; }
        
        TimeSpan EndTime { get; }
    }
}
