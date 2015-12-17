using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseRefferenceCollectionViewModel : BindableBase, IDisposable, IDialogViewModel
    {
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;
        private int recordTypeId;
        public BusyMediator BusyMediator { get; set; }
        private CancellationTokenSource currentSavingToken;

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

            Refferences = new ObservableCollectionEx<AnalyseRefferenceViewModel>();
        }

        private void AddRefference()
        {
            Refferences.Add(new AnalyseRefferenceViewModel()
                {
                    SelectedGenderId = 1,
                    AgeFrom = 0,
                    AgeTo = 99
                });
        }

        private void RemoveRefference()
        {
            if (SelectedRefference != null)
            {
                if (SelectedRefference.Id != 0)
                    recordService.DeleteAnalyseRefference(SelectedRefference.Id);
                Refferences.Remove(selectedRefference);
                if (Refferences.Any())
                    SelectedRefference = Refferences.First();
            }
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
                        Id = x.Id,
                        SelectedGenderId = x.IsMale ? 1 : 0,
                        AgeFrom = x.AgeFrom,
                        AgeTo = x.AgeTo,
                        RefMin = x.RefMin,
                        RefMax = x.RefMax
                    }).ToArray();
                Refferences.Clear();
                Refferences.AddRange(refs);
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

                    var unitsQuery = cacheService.GetItems<Unit>().Where(x => !x.OnlyForMedWare).Select(x => new { x.Id, x.ShortName }).ToArray();
                    Units = new ObservableCollectionEx<FieldValue>();
                    Units.Add(new FieldValue() { Value = SpecialValues.NonExistingId, Field = "- отсутствует -" });
                    Units.AddRange(unitsQuery.Select(x => new FieldValue() { Value = x.Id, Field = x.ShortName }));
                    var parameterRecordType = recordService.GetRecordTypeById(parameterRecordTypeId).First();
                    SelectedUnitId = parameterRecordType.RecordTypeUnits.Any() ? parameterRecordType.RecordTypeUnits.FirstOrDefault().UnitId : SpecialValues.NonExistingId;
                }
            }
        }

        private bool AllowSave()
        {
            if (Refferences.Any(x => x.RefMax == 0 && x.RefMin == 0))
            {
                messageService.ShowWarning("В одном или нескольких референсных интервалов не указаны минимальные и максимальные значения.");
                return false;
            }
            if (Refferences.Any(x => x.RefMax < x.RefMin))
            {
                messageService.ShowWarning("Минимальное значение референса не может быть больше максимального.");
                return false;
            }
            return true;
        }

        private async void SaveAnalyseRefferences()
        {
            BusyMediator.Activate("Сохранение референсных интервалов...");
            logService.Info("Create analyse ...");
            if (currentSavingToken != null)
            {
                currentSavingToken.Cancel();
                currentSavingToken.Dispose();
            }
            currentSavingToken = new CancellationTokenSource();
            var token = currentSavingToken.Token;
            SaveIsSuccessful = false;
            try
            {
                foreach (var item in Refferences)
                {
                    var refference = recordService.GetAnalyseReferenceById(item.Id).FirstOrDefault();
                    if (refference == null)
                        refference = new AnalyseRefference();

                    refference.RecordTypeId = this.recordTypeId;
                    refference.ParameterRecordTypeId = SelectedParameterId;
                    refference.IsMale = item.SelectedGenderId == 1;
                    refference.AgeFrom = item.AgeFrom;
                    refference.AgeTo = item.AgeTo;
                    refference.RefMin = item.RefMin;
                    refference.RefMax = item.RefMax;
                    item.Id = await recordService.SaveAnalyseRefference(refference, token);                    
                }

                if (!SpecialValues.IsNewOrNonExisting(SelectedUnitId))
                {
                    var recordTypeUnit = recordService.GetRecordTypeUnit(SelectedParameterId, SelectedUnitId).FirstOrDefault();
                    if (recordTypeUnit == null)
                        recordTypeUnit = new RecordTypeUnit();
                    recordTypeUnit.RecordTypeId = SelectedParameterId;
                    recordTypeUnit.UnitId = SelectedUnitId;
                    int savedUnitId = await recordService.SaveRecordTypeUnit(recordTypeUnit, token);
                }
                SaveIsSuccessful = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save refferences for parameter with Id = {0}", selectedParameterId);
                messageService.ShowError("Ошибка сохранения референсного интервала.");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
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

        private AnalyseRefferenceViewModel selectedRefference;
        public AnalyseRefferenceViewModel SelectedRefference
        {
            get { return selectedRefference; }
            set { SetProperty(ref selectedRefference, value); }

        }
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
            get { return "Закрыть"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public bool SaveIsSuccessful;

        private void Close(bool? validate)
        {           
            if (validate == true)
            {
                if (AllowSave())
                    SaveAnalyseRefferences();
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
    }
}
