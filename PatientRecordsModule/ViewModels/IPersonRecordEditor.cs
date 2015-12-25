using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels
{
    public interface IPersonRecordEditor
    {
        void SetRVAIds(int visitId, int assignmentId, int recordId);
    }
}
