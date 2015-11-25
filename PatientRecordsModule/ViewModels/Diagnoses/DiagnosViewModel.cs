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

        private int id;
        public int Id
        {
            get { return id; }
            set { SetTrackedProperty(ref id, value); }
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

        private int levelId;
        public int LevelId
        {
            get { return levelId; }
            set { SetTrackedProperty(ref levelId, value); }
        }

        private string levelName;
        public string LevelName
        {
            get { return levelName; }
            set { SetTrackedProperty(ref levelName, value); }
        }

        private int levelPriority;
        public int LevelPriority
        {
            get { return levelPriority; }
            set { SetTrackedProperty(ref levelPriority, value); }
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

        private bool needSetMainDiagnos;
        public bool NeedSetMainDiagnos
        {
            get { return needSetMainDiagnos; }
            set { SetTrackedProperty(ref needSetMainDiagnos, value); }
        } 

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
