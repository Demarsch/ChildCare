using System;

namespace Core.Misc
{
    public static class AppConfiguration
    {
        public static readonly TimeSpan UserInputDelay = TimeSpan.FromSeconds(0.75);

        public static readonly TimeSpan PendingOperationDelay = TimeSpan.FromSeconds(1.5);

        public static readonly int UserInputSearchThreshold = 3;
    }
}
