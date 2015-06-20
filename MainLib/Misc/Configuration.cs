using System;
using DataLib;

namespace Core
{
    public static class Configuration
    {
        #region UI related properties

        public static TimeSpan ScheduleUnitTimeInterval { get { return TimeSpan.FromMinutes(1.0); } }

        public static double ScheduleUnitPerTimeInterval { get { return 3.0; } }

        #endregion

        public static string CurrentLpuName { get; private set; }

        public static string DefaultRecordTypeTime { get; private set; }

        public static void Initialize(ICacheService cacheService)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException("The configuration was already initialized");
            }
            var currentSetting = cacheService.GetItemByName<DBSetting>(DBSetting.CurrentLpuName);
            CurrentLpuName = currentSetting == null ? "Больница №1" : currentSetting.Value;
            currentSetting = cacheService.GetItemByName<DBSetting>(DBSetting.DefaultRecordTypeTime);
            DefaultRecordTypeTime = currentSetting == null ? "8:00 - 12:00, 13:00 - 17:00" : currentSetting.Value;
            isInitialized = true;
        }

        private static bool isInitialized;
    }
}
