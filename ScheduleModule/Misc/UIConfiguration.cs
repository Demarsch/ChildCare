using System;

namespace ScheduleModule.Misc
{
    public static class UiConfiguration
    {
        public static TimeSpan ScheduleUnitTimeInterval { get { return TimeSpan.FromMinutes(1.0); } }

        public static double ScheduleUnitPerTimeInterval { get { return 3.0; } }
    }
}
