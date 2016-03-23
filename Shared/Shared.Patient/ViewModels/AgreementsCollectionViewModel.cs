using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Mvvm;
using Shared.Patient.Services;
using Prism.Commands;
using System.Windows.Navigation;
using Core.Reports;

namespace Shared.Patient.ViewModels
{
    public class AgreementsCollectionViewModel : BindableBase, IDialogViewModel
    {
        private readonly IPatientAssignmentService patientAssignmentService;
        private readonly ILog log;
        private int personId;
        private readonly IReportGeneratorHelper reportGenerator;

        public AgreementsCollectionViewModel(IPatientAssignmentService patientAssignmentService, ILog log, ICacheService cacheService, IReportGeneratorHelper reportGenerator)
        {
            if (patientAssignmentService == null)
            {
                throw new ArgumentNullException("patientAssignmentService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (reportGenerator == null)
            {
                throw new ArgumentNullException("reportGenerator");
            }
            this.patientAssignmentService = patientAssignmentService;
            this.log = log;
            this.reportGenerator = reportGenerator;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Agreements = new ObservableCollectionEx<AgreementViewModel>();
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        private ObservableCollectionEx<AgreementViewModel> agreements;
        public ObservableCollectionEx<AgreementViewModel> Agreements
        {
            get { return agreements; }
            set { SetProperty(ref agreements, value); }
        }

        private AgreementViewModel selectedAgreement;
        public AgreementViewModel SelectedAgreement
        {
            get { return selectedAgreement; }
            set
            {
                if (SetProperty(ref selectedAgreement, value))
                {
                    Agreements.Where(x => x.IsChecked && x.Id != value.Id).ForEach(x => x.IsChecked = false);
                    value.IsChecked = true;
                }
            }
        } 

        public BusyMediator BusyMediator { get; private set; }
        public FailureMediator FailureMediator { get; private set; }
               
        private bool patientIsSelected;
        public bool PatientIsSelected
        {
            get { return patientIsSelected; }
            set { SetProperty(ref patientIsSelected, value); }
        }            

        public async void LoadAgreementsAsync(int patientId)
        {
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка списка...");
            this.personId = patientId;
            Agreements.Clear();
            try
            {
                var agreementsQuery = await patientAssignmentService.GetAgreementDocuments()
                                                                .Select(x => new
                                                                             {
                                                                                 x.Id,
                                                                                 x.ReportTitle,
                                                                                 x.Name,
                                                                                 x.Description
                                                                             })
                                                                .ToArrayAsync();
                Agreements.AddRange(agreementsQuery.Select(x => new AgreementViewModel
                                                                {
                                                                    Id = x.Id,
                                                                    IsChecked = false,
                                                                    Title = x.ReportTitle,
                                                                    Name = x.Name,
                                                                    Description = x.Description
                                                                }));
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load agreement documents");
                FailureMediator.Activate("Не удалось загрузить список документов", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void PrintDocument()
        {                    
            var report = reportGenerator.CreateDocX(SelectedAgreement.Name);
            string emptyValue = string.Empty;
            string defValue = "____________";
            string nonExistValue = "отсутствует";

            int childAge = patientAssignmentService.GetDBSettingValue(DBSetting.ChildAge).ToInt();

            report.Data["NIKIName"] = patientAssignmentService.GetDBSettingValue(DBSetting.NIKIName);
            report.Data["NIKIAddress"] = patientAssignmentService.GetDBSettingValue(DBSetting.NIKIAddress);
            report.Data["CurrentDate"] = DateTime.Now.ToShortDateString();

            var patient = patientAssignmentService.GetPersonById(personId).First();
            if (patient.BirthDate.AddYears(childAge).Date > DateTime.Now.Date)
            {
                var relativeRelation = patient.PersonRelatives.Any(x => x.IsRepresentative) ? patient.PersonRelatives.First(x => x.IsRepresentative) : null;
                if (relativeRelation != null)
                {
                    var relative = relativeRelation.Person1;
                    report.Data["ClientFullName"] = relative.ToString();
                    report.Data["RelativeTypeName"] = relativeRelation.RelativeRelationship.Name;
                    if (relative.PersonIdentityDocuments.Any())
                    {
                        var doc = relative.PersonIdentityDocuments.OrderByDescending(x => x.BeginDate).First();
                        report.Data["ClientIdentityDocument"] = doc.IdentityDocumentType.Name + ": " + doc.SeriesAndNumber + " выдан: " + doc.GivenOrg + " " + doc.BeginDate.ToShortDateString();
                    }
                    else
                        report.Data["ClientIdentityDocument"] = defValue;
                }
                else
                {
                    report.Data["ClientFullName"] = defValue;
                    report.Data["ClientIdentityDocument"] = defValue;
                    report.Data["RelativeTypeName"] = defValue;
                }
            }
            else
                report.Data["ClientFullName"] = patient.ToString();

            report.Data["PatientFullName"] = patient.ToString();
            if (patient.PersonIdentityDocuments.Any())
            {
                var doc = patient.PersonIdentityDocuments.OrderByDescending(x => x.BeginDate).First();
                report.Data["PatientIdentityDocument"] = doc.IdentityDocumentType.Name + ": " + doc.SeriesAndNumber + " выдан: " + doc.GivenOrg + " " + doc.BeginDate.ToShortDateString();
            }
            else
                report.Data["PatientIdentityDocument"] = defValue;

            if (patient.PersonAddresses.Any(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)))
            {
                var address = patient.PersonAddresses.Where(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["PatientAddress"] = address.UserText +
                                         (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
            }
            else
                report.Data["PatientAddress"] = defValue;

            report.Editable = false;
            report.Show();
        }

        #region IDialogViewModel

        public string Title
        {
            get { return "Документы леч-диагн процесса"; }
        }

        public string ConfirmButtonText
        {
            get { return "Печать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private async void Close(bool? validate)
        {
            if (validate == true)
            {
                if (Agreements.Any(x => x.IsChecked))
                    PrintDocument();
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
    }
}
