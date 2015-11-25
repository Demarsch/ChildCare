using Core.Misc;
using Core.Wpf.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.ViewModels
{
    public class DiagnosViewModel : TrackableBindableBase, IDisposable, IChangeTrackerMediator
    {
        public IChangeTracker ChangeTracker { get; private set; }
                
        public DiagnosViewModel()
        {
            ChangeTracker = new ChangeTrackerEx<DiagnosViewModel>(this);
        }

        private int diagnosId;
        public int DiagnosId
        {
            get { return diagnosId; }
            set { SetTrackedProperty(ref diagnosId, value); }
        }

        private int recordId;
        public int RecordId
        {
            get { return recordId; }
            set { SetTrackedProperty(ref recordId, value); }
        }

        private string recordName;
        public string RecordName
        {
            get { return recordName; }
            set { SetTrackedProperty(ref recordName, value); }
        }

        private int diagnosTypeId;
        public int DiagnosTypeId
        {
            get { return diagnosTypeId; }
            set { SetTrackedProperty(ref diagnosTypeId, value); }
        }

        private string diagnosTypeName;
        public string DiagnosTypeName
        {
            get { return diagnosTypeName; }
            set { SetTrackedProperty(ref diagnosTypeName, value); }
        }

        private string diagnosText;
        public string DiagnosText
        {
            get { return diagnosText; }
            set { SetTrackedProperty(ref diagnosText, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetTrackedProperty(ref mkb, value); }
        }

        private int diagnosLevelId;
        public int DiagnosLevelId
        {
            get { return diagnosLevelId; }
            set { SetTrackedProperty(ref diagnosLevelId, value); }
        }

        private string diagnosLevelName;
        public string DiagnosLevelName
        {
            get { return diagnosLevelName; }
            set { SetTrackedProperty(ref diagnosLevelName, value); }
        }

        private int? complicationId;
        public int? ComplicationId
        {
            get { return complicationId; }
            set { SetTrackedProperty(ref complicationId, value); }
        }

        private bool isMainDiagnos;
        public bool IsMainDiagnos
        {
            get { return isMainDiagnos; }
            set { SetTrackedProperty(ref isMainDiagnos, value); }
        }

        private bool needSelectMainDiagnos;
        public bool NeedSelectMainDiagnos
        {
            get { return needSelectMainDiagnos; }
            set { SetTrackedProperty(ref needSelectMainDiagnos, value); }
        } 

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
