using System;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace DataLib
{
    public partial class RecordContract
    {
        public string DisplayName
        {
            get
            {
                return (this.Number.HasValue ? "Договор №" + this.Number.ToSafeString() + " - " : string.Empty) + this.ContractName;
            }
        }
    }
}
