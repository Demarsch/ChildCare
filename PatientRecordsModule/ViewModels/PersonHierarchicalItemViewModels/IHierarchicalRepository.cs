using Shared.PatientRecords.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels
{
    public interface IHierarchicalRepository
    {
        IHierarchicalItem GetHierarchicalItem(PersonRecItem personRecItem);
        IPersonRecordEditor GetEditor(PersonRecItem personRecItem);
    }
}
