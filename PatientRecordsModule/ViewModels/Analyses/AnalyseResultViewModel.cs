using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseResultViewModel: TrackableBindableBase, IDisposable, IChangeTrackerMediator
    {
        public IChangeTracker ChangeTracker { get; private set; }
        private IPatientRecordsService recordService;
        private ICacheService cacheService;

        public AnalyseResultViewModel(IPatientRecordsService recordService, ICacheService cacheService)
        {
            this.recordService = recordService;
            this.cacheService = cacheService;

            var unitsQuery = cacheService.GetItems<Unit>().Where(x => !x.OnlyForMedWare).Select(x => new { x.Id, x.ShortName }).ToArray();
            Units = new ObservableCollectionEx<FieldValue>();
            Units.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- отсутствует -" });
            Units.AddRange(unitsQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.ShortName }));
            SelectedUnitId = SpecialValues.NonExistingId;

            ChangeTracker = new ChangeTrackerEx<AnalyseResultViewModel>(this);
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string resultText;
        public string ResultText
        {
            get { return resultText; }
            set 
            { 
                if (SetTrackedProperty(ref resultText, value))
                {
                    var parameterRecordType = recordService.GetRecordTypeById(ParameterRecordTypeId).First();
                    if (parameterRecordType.AnalyseRefferences.Any())
                    {
                        double result = 0.0;
                        if (double.TryParse(value, out result))
                        {
                            var reference = recordService.GetAnalyseReference(RecordTypeId, ParameterRecordTypeId, IsMale, Age, Date).FirstOrDefault();
                            if (reference != null)
                            {                                
                                if (result < reference.RefMin)
                                {
                                    IsBelow = true;
                                    Background = Brushes.LightBlue;
                                }
                                else if (result > reference.RefMax)
                                {
                                    IsAbove = true;
                                    Background = Brushes.Pink;
                                }
                                else
                                {
                                    isNormal = true;
                                    Background = Brushes.White;
                                }
                            }
                        }
                        else
                        {
                            isNormal = true;
                            Background = Brushes.White;
                        }
                    }
                }            
            }
        }

        private string parameterName;
        public string ParameterName
        {
            get { return parameterName; }
            set { SetProperty(ref parameterName, value); }
        }

        private int parameterRecordTypeId;
        public int ParameterRecordTypeId
        {
            get { return parameterRecordTypeId; }
            set { SetProperty(ref parameterRecordTypeId, value); }
        }

        private int priority;
        public int Priority
        {
            get { return priority; }
            set { SetProperty(ref priority, value); }
        }

        private ObservableCollectionEx<FieldValue> units;
        public ObservableCollectionEx<FieldValue> Units
        {
            get { return units; }
            set { SetProperty(ref units, value); }
        }

        private int selectedUnitId;
        public int SelectedUnitId
        {
            get { return selectedUnitId; }
            set { SetTrackedProperty(ref selectedUnitId, value); }
        }

        private string unitView;
        public string UnitView
        {
            get { return unitView; }
            set { SetTrackedProperty(ref unitView, value); }
        }

        private string details;
        public string Details
        {
            get { return details; }
            set { SetTrackedProperty(ref details, value); }
        }

        public string ResultReference
        {
            get 
            {
                if (RefMin == 0.0 && RefMax == 0.0)
                    return string.Empty;
                return RefMin + " - " + RefMax; 
            }
        }

        private SolidColorBrush background;
        public SolidColorBrush Background
        {
            get { return background; }
            set { SetProperty(ref background, value); }
        }

        private bool isNormal;
        public bool IsNormal
        {
            get { return isNormal; }
            set { SetProperty(ref isNormal, value); }
        }

        private bool isAbove;
        public bool IsAbove
        {
            get { return isAbove; }
            set { SetProperty(ref isAbove, value); }
        }

        private bool isBelow;
        public bool IsBelow
        {
            get { return isBelow; }
            set { SetProperty(ref isBelow, value); }
        }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { SetProperty(ref recordTypeId, value); }
        }

        private bool isMale;
        public bool IsMale
        {
            get { return isMale; }
            set { SetProperty(ref isMale, value); }
        }

        private int age;
        public int Age
        {
            get { return age; }
            set { SetProperty(ref age, value); }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        private double refMin;
        public double RefMin
        {
            get { return refMin; }
            set
            {
                if (SetProperty(ref refMin, value))
                    OnPropertyChanged(() => ResultReference);
            }
        }

        private double refMax;
        public double RefMax
        {
            get { return refMax; }
            set 
            {
                if (SetProperty(ref refMax, value))
                    OnPropertyChanged(() => ResultReference);
            }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
