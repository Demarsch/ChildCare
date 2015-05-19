using System;
using System.Collections.Generic;

namespace DataLib
{
    public partial class ScheduleItem : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
