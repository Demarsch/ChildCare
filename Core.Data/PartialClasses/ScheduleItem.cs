using System;

namespace Core.Data
{
    public partial class ScheduleItem : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
