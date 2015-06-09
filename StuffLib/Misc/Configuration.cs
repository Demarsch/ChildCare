using System;

namespace Core
{
    public static class Configuration
    {
        //TODO: initialize properties from DBSettings table
        #region UI related properties

        public static TimeSpan ScheduleUnitTimeInterval { get { return TimeSpan.FromMinutes(1.0); } }

        public static double ScheduleUnitPerTimeInterval { get { return 3.0; } }

        #endregion

        public static string CurrentLpuName { get { return "Больница №2"; } }
    }
}
