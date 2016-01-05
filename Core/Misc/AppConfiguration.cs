using System;

namespace Core.Misc
{
    public static class AppConfiguration
    {
        public static readonly TimeSpan UserInputDelay = TimeSpan.FromSeconds(0.75);

        public static readonly int UserInputSearchThreshold = 2;

        public static readonly int SearchResultTakeTopCount = 10;
    }
}
