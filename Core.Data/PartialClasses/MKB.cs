using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public partial class MKB
    {
        public override string ToString()
        {
            return this.Code + " " + this.Name;
        }
    }
}
