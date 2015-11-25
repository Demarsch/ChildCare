using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.Misc
{
    public interface IRecordTypeProtocol: INotifyPropertyChanged
    {
        ProtocolMode CurrentMode { get; set; }

        void LoadProtocol(int assignmentId, int recordId, int visitId);

        bool SaveProtocol();

        void PrintProtocol();        
    }

    public enum ProtocolMode
    {
        Edit,
        View
    }
}
