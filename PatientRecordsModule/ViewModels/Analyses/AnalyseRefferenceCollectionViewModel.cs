using Core.Data;
using Core.Data.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseRefferenceCollectionViewModel : BindableBase, IDisposable, IDialogViewModel, IDataErrorInfo
    {
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;
        private int recordTypeId;
        public BusyMediator BusyMediator { get; set; }

        public AnalyseRefferenceCollectionViewModel(IPatientRecordsService recordService, IDialogService messageService, ICacheService cacheService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }            
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            } 
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            this.recordService = recordService;
            this.logService = logService;
            this.messageService = messageService;
            BusyMediator = new BusyMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);
        
            addRefferenceCommand = new DelegateCommand(AddRefference);
            removeRefferenceCommand = new DelegateCommand(RemoveRefference);
        }

        private void AddRefference()
        {
            
        }

        private void RemoveRefference()
        {
            
        }

        internal void Initialize(int recordTypeId, int parameterRecordTypeId)
        {
            this.recordTypeId = recordTypeId;
            LoadDataSources(parameterRecordTypeId);
        }

        private void LoadRefferences(int parameterRecordTypeId)
        {
            var referencesQuery = recordService.GetAnalyseReference(this.recordTypeId, parameterRecordTypeId).ToArray();
            if (referencesQuery.Any())
            {
                var refs = referencesQuery.Select(x => new AnalyseRefferenceViewModel()
                    {
                        SelectedGenderId = x.IsMale ? 1 : 0,
                        AgeFrom = x.AgeFrom,
                        AgeTo = x.AgeTo,
                        RefMin = x.RefMin,
                        RefMax = x.RefMax
                    }).ToArray();
                Refferences = new ObservableCollectionEx<AnalyseRefferenceViewModel>(refs);
            }           
        }

        private void LoadDataSources(int parameterRecordTypeId)
        {
            var type = recordService.GetRecordTypeById(this.recordTypeId).FirstOrDefault();
            if (type != null)
            {
                var parametersQuery = type.RecordTypes1.Select(x => new { x.Id, x.ShortName }).ToArray();
                if (parametersQuery.Any())
                {
                    Parameters = new ObservableCollectionEx<FieldValue>();
                    Parameters.AddRange(parametersQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.ShortName }));
                    SelectedParameterId = parameterRecordTypeId;
                }
            }

            var unitsQuery = cacheService.GetItems<Unit>().Where(x => !x.OnlyForMedWare).Select(x => new { x.Id, x.ShortName }).ToArray();
            Units = new ObservableCollectionEx<FieldValue>();
            Units.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- отсутствует -" });
            Units.AddRange(unitsQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.ShortName }));
            SelectedUnitId = type.RecordTypeUnits.Any() ? type.RecordTypeUnits.FirstOrDefault().UnitId : SpecialValues.NonExistingId;
        }

        private void SaveAnalyseRefferences()
        {
            SaveIsSuccessful = false;


        }

        #region Properties

        private string analyseName;
        public string AnalyseName
        {
            get { return analyseName; }
            set { SetProperty(ref analyseName, value); }
        }

        public ObservableCollectionEx<FieldValue> Parameters { get; set; }

        private int selectedParameterId;
        public int SelectedParameterId
        {
            get { return selectedParameterId; }
            set 
            {
                if (SetProperty(ref selectedParameterId, value))
                    LoadRefferences(value);
            }
        }

        public ObservableCollectionEx<FieldValue> Units { get; set; }

        private int selectedUnitId;
        public int SelectedUnitId
        {
            get { return selectedUnitId; }
            set { SetProperty(ref selectedUnitId, value); }
        }

        public ObservableCollectionEx<AnalyseRefferenceViewModel> Refferences { get; set; }

        private readonly DelegateCommand addRefferenceCommand;
        public ICommand AddRefferenceCommand
        {
            get { return addRefferenceCommand; }
        }

        private readonly DelegateCommand removeRefferenceCommand;
        public ICommand RemoveRefferenceCommand
        {
            get { return removeRefferenceCommand; }
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Редактор референсных значений для анализов"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public bool SaveIsSuccessful;

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                if (IsValid)
                {
                    SaveAnalyseRefferences();
                }
                else
                {
                    messageService.ShowWarning("Проверьте правильность заполнения полей.");
                }
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        public void Dispose()
        {
        }

        #region IDataError

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedAnalyseId")
                {
                    //result = selectedAnalyseId.IsNewOrNonExisting() ? "Укажите наименование исследования" : string.Empty;
                }                

                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        #endregion
    }
}
