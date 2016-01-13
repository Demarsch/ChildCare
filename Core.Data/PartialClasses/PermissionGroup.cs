using System.Collections.Generic;
using System.Diagnostics;
using Core.Misc;

namespace Core.Data
{
    [DebuggerDisplay("{Id} - {Name}")]
    public partial class PermissionGroup
    {
        public override string ToString()
        {
            return string.Format("{0} - {1}", Id, Name);
        }
    }
}
