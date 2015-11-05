using System;

namespace Core.Data.Misc
{
    public static class SpecialValues
    {
        public const int NonExistingId = -1;

        public const int NewId = 0;

        public static readonly DateTime MinDate = new DateTime(1900, 1, 1);

        public static readonly DateTime MaxDate = new DateTime(9998, 12, 31);
    }
}
