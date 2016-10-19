using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared.PatientRecords.ViewModels
{
    public interface IPersonRecordEditor : INotifyPropertyChanged
    {
        bool IsViewModeInCurrentProtocolEditor { get; }
        bool IsEditModeInCurrentProtocolEditor { get; }
        bool IsRecordCanBeCompleted { get; set; }
        bool AllowDocuments { get; set; }
        bool AllowDICOM { get; set; }
        bool CanAttachDICOM { get; set; }
        bool CanDetachDICOM { get; set; }

        void SetRVAIds(int visitId, int assignmentId, int recordId);

        ICommand PrintProtocolCommand { get; }
        ICommand SaveProtocolCommand { get; }
        ICommand ShowInEditModeCommand { get; }
        ICommand ShowInViewModeCommand { get; }
        ICommand AttachDocumentCommand { get; }
        ICommand DetachDocumentCommand { get; }
        ICommand AttachDICOMCommand { get; }
        ICommand DetachDICOMCommand { get; }
    }
}
