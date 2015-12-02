using Core.Misc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.Misc
{
    public interface IRecordTypeProtocol : INotifyPropertyChanged
    {
        ProtocolMode CurrentMode { get; set; }

        void LoadProtocol(int assignmentId, int recordId, int visitId);

        int SaveProtocol(int recordId, int visitId);

        void PrintProtocol();

        IChangeTracker ChangeTracker { get; set; }
    }

    public enum ProtocolMode
    {
        Edit,
        View
    }
}
