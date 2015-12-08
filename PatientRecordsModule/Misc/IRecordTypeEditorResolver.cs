using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.Misc
{
    public interface IRecordTypeEditorResolver
    {
        IRecordTypeProtocol Resolve(string editor);
    }
}
