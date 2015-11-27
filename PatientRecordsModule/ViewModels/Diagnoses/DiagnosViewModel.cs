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
            set { SetProperty(ref id, value); }
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
            set { SetProperty(ref levelId, value); }
        }

        private string levelName;
        public string LevelName
        {
            get { return levelName; }
            set { SetProperty(ref levelName, value); }
        }

        private int levelPriority;
        public int LevelPriority
        {
            get { return levelPriority; }
            set { SetProperty(ref levelPriority, value); }
        }

        private bool isComplication;
        public bool IsComplication
        {
            get { return isComplication; }
            set { SetProperty(ref isComplication, value); }
        }

        private bool hasMKB;
        public bool HasMKB
        {
            get { return hasMKB; }
            set { SetProperty(ref hasMKB, value); }
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
      
        private bool allowClarification;
        public bool AllowClarification
        {
            get { return allowClarification; }
            set { SetProperty(ref allowClarification, value); }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
