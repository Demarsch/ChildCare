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
using System.Threading.Tasks;
using Shared.Commissions.Services;
using System.Data.Entity.Validation;

namespace Shared.Commissions.ViewModels
{
    public class CreateAssignmentCommissionViewModel: BindableBase, IDialogViewModel, IDataErrorInfo
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private CancellationTokenSource currentSavingToken;

        public BusyMediator BusyMediator { get; set; }
        public NotificationMediator NotificationMediator { get; private set; }

        public CreateAssignmentCommissionViewModel(ICommissionService commissionService,  ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }                 
            this.logService = logService;
            this.commissionService = commissionService;
            
            BusyMediator = new BusyMediator();
            NotificationMediator = new NotificationMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);

            CommissionTypeGroups = new ObservableCollectionEx<FieldValue>();
            CommissionTypes = new ObservableCollectionEx<FieldValue>();
            CommissionQuestions = new ObservableCollectionEx<FieldValue>();
            
        }

        internal void Initialize(int patientId)
        {
            CommissionTypeGroups.Clear();
            CommissionTypes.Clear();
            CommissionQuestions.Clear();

            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources for commission assignment...");            
            
            try
            {
                PersonId = patientId;
                var person = commissionService.GetPerson(patientId).First();
                PatientFIO = person.FullName + " " + person.BirthDate.ToShortDateString() + " г.р.";
                CommissionDateTime = DateTime.Now;
                logService.InfoFormat("Data sources for commission assignment are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for commission assignment");
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }
        
        private async Task CreateAssignmentCommission()
        {
            logService.Info("Create commission assignment ...");
            saveIsSucceessfull = false; 
            if (currentSavingToken != null)
            {
                currentSavingToken.Cancel();
                currentSavingToken.Dispose();
            }
            currentSavingToken = new CancellationTokenSource();
            var token = currentSavingToken.Token;
            try
            {    
                var exception = string.Empty;
                ProtocolId = await commissionService.CreateCommissionAssignment(personId, commissionDateTime, selectedCommissionTypeId, selectedCommissionQuestionId, codeMKB, commissionDetails, token);
                saveIsSucceessfull = true; 
            }
            catch (DbEntityValidationException e)
            {
                saveIsSucceessfull = false; 
                string err = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    err += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                        err += string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                }
                logService.ErrorFormatEx(e, "Failed to create commission assignment for person with Id = {0}. {1}", this.personId, err);
                NotificationMediator.Activate("Ошибка при направлении на комиссию", NotificationMediator.DefaultHideTime);
            }
            catch (Exception ex)
            {
                saveIsSucceessfull = false; 
                logService.ErrorFormatEx(ex, "Failed to create commission assignment for person with Id = {0}", this.personId);
                NotificationMediator.Activate("Ошибка при направлении на комиссию", NotificationMediator.DefaultHideTime);
            }
            finally
            {
                if (saveIsSucceessfull)
                    NotificationMediator.Activate("Направление на комиссию создано", NotificationMediator.DefaultHideTime);
                BusyMediator.Deactivate();
            }
        }

        #region Properties

        public int ProtocolId { get; set; }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { SetProperty(ref patientFIO, value); }
        }

        private string codeMKB;
        public string CodeMKB
        {
            get { return codeMKB; }
            set { SetProperty(ref codeMKB, value); }
        }

        public ObservableCollectionEx<FieldValue> CommissionTypeGroups { get; set; }

        private int selectedCommissionTypeGroupId;
        public int SelectedCommissionTypeGroupId
        {
            get { return selectedCommissionTypeGroupId; }
            set
            {
                CommissionTypes.Clear();
                CommissionQuestions.Clear();
                selectedCommissionTypeGroupId = 0;
                if (SetProperty(ref selectedCommissionTypeGroupId, value))
                {
                    var сommissionTypesQuery = commissionService.GetCommissionTypes(CommissionDateTime, value).Select(x => new { x.Id, x.Name }).ToArray();
                    CommissionTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите тип подкомиссии -" });
                    CommissionTypes.AddRange(сommissionTypesQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                    SelectedCommissionTypeId = SpecialValues.NonExistingId;
                }
            }
        }

        public ObservableCollectionEx<FieldValue> CommissionTypes { get; set; }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set
            {
                CommissionQuestions.Clear();
                selectedCommissionTypeId = 0;
                if (SetProperty(ref selectedCommissionTypeId, value))
                {
                    var сommissionQuestionsQuery = commissionService.GetCommissionQuestions(CommissionDateTime, value).Select(x => new { x.Id, x.Name }).ToArray();
                    CommissionQuestions.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите тип подкомиссии -" });
                    CommissionQuestions.AddRange(сommissionQuestionsQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                    SelectedCommissionQuestionId = SpecialValues.NonExistingId;
                }
            }
        }

        public ObservableCollectionEx<FieldValue> CommissionQuestions { get; set; }

        private int selectedCommissionQuestionId;
        public int SelectedCommissionQuestionId
        {
            get { return selectedCommissionQuestionId; }
            set
            {
                selectedCommissionQuestionId = 0;
                SetProperty(ref selectedCommissionQuestionId, value); 
            }
        }

        private DateTime commissionDateTime;
        public DateTime CommissionDateTime
        {
            get { return commissionDateTime; }
            set 
            {                
                if (SetProperty(ref commissionDateTime, value))
                {
                    CommissionTypeGroups.Clear();
                    var сommissionTypeGroupsQuery = commissionService.GetCommissionTypeGroups(value).Select(x => new { x.Id, x.Name }).ToArray();
                    CommissionTypeGroups.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите тип комиссии -" });
                    CommissionTypeGroups.AddRange(сommissionTypeGroupsQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                    SelectedCommissionTypeGroupId = SpecialValues.NonExistingId;
                }
            }
        }

        private string commissionDetails;
        public string CommissionDetails
        {
            get { return commissionDetails; }
            set { SetProperty(ref commissionDetails, value); }
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Направление на коммиссию"; }
        }

        public string ConfirmButtonText
        {
            get { return "Направить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }
        private bool saveIsSucceessfull = false;

        private async void Close(bool? validate)
        {
            if (validate == true)
            {
                if (IsValid)
                {
                    await CreateAssignmentCommission();
                    if (saveIsSucceessfull)
                        OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
            }
            else
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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
                if (columnName == "SelectedCommissionTypeGroupId")
                {
                    result = SpecialValues.IsNewOrNonExisting(SelectedCommissionTypeGroupId) ? "Укажите тип комиссии" : string.Empty;
                }
                if (columnName == "SelectedCommissionTypeId")
                {
                    result = SpecialValues.IsNewOrNonExisting(SelectedCommissionTypeId) ? "Укажите тип подкомиссии" : string.Empty;
                }         
                if (columnName == "SelectedCommissionQuestionId")
                {
                    result = SpecialValues.IsNewOrNonExisting(SelectedCommissionQuestionId) ? "Укажите вопрос подкомиссии" : string.Empty;
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
