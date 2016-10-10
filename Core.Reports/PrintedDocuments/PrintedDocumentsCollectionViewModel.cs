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
using Prism.Commands;
using System.Windows.Navigation;
using Core.Reports;
using Core.Reports.Services;
using Core.Reports.DTO;
using System.Collections.Generic;

namespace Core.Reports
{
    public class PrintedDocumentsCollectionViewModel : BindableBase, IDialogViewModel
    {
        private readonly IDocumentService documentService;
        private readonly ILog log;
        private object obj;
        private FieldValue parameter;
        private bool canEdit;
        private readonly IReportGeneratorHelper reportGenerator;

        public PrintedDocumentsCollectionViewModel(IDocumentService documentService, ILog log, ICacheService cacheService, IReportGeneratorHelper reportGenerator)
        {           
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (reportGenerator == null)
            {
                throw new ArgumentNullException("reportGenerator");
            }
            this.documentService = documentService;
            this.log = log;
            this.reportGenerator = reportGenerator;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            PrintedDocuments = new ObservableCollectionEx<PrintedDocumentViewModel>();
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        private ObservableCollectionEx<PrintedDocumentViewModel> printedDocuments;
        public ObservableCollectionEx<PrintedDocumentViewModel> PrintedDocuments
        {
            get { return printedDocuments; }
            set { SetProperty(ref printedDocuments, value); }
        }

        private PrintedDocumentViewModel selectedPrintedDocument;
        public PrintedDocumentViewModel SelectedPrintedDocument
        {
            get { return selectedPrintedDocument; }
            set
            {
                if (SetProperty(ref selectedPrintedDocument, value))
                {
                    PrintedDocuments.Where(x => x.IsChecked && x.TemplateId != value.TemplateId).ForEach(x => x.IsChecked = false);
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

        public void LoadPrintedDocumentsAsync(int printedDocumentId, FieldValue parameter, object obj = null, bool canEdit = false)
        {
            FailureMediator.Deactivate();
            this.parameter = parameter;
            this.obj = obj;
            this.canEdit = canEdit;
            try
            {
                var document = documentService.GetPrintedDocumentById(printedDocumentId).FirstOrDefault();
                if (document != null && document.ReportTemplateId.HasValue)
                {
                    SelectedPrintedDocument = new PrintedDocumentViewModel()
                    {
                        TemplateId = document.ReportTemplateId.Value,
                        SystemName = document.ReportTemplate.Name,
                        ReportFullName = document.Name,
                        ReportShortName = document.ShortName,
                        Options = document.Options
                    };
                    PrintDocument();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load printed document");
                FailureMediator.Activate("Не удалось печатную форму документа", null, ex, true);
            }            
        }
        
        public async void LoadPrintedDocumentsAsync(string documentOption, FieldValue parameter, object obj = null, bool canEdit = false)
        {
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка списка...");
            this.parameter = parameter;
            this.obj = obj;
            this.canEdit = canEdit;
            PrintedDocuments.Clear();
            try
            {
                var documentsQuery = await documentService.GetPrintedDocuments(documentOption)                                                               
                                                                .Select(x => new { TemplateId = x.Id, 
                                                                                   SystemName = x.Name, 
                                                                                   ReportFullName = x.PrintedDocuments.FirstOrDefault().Name, 
                                                                                   ReportShortName = x.PrintedDocuments.FirstOrDefault().ShortName,
                                                                                   Options = x.PrintedDocuments.FirstOrDefault().Options })
                                                                .ToArrayAsync();
                PrintedDocuments.AddRange(documentsQuery.Select(x => new PrintedDocumentViewModel { TemplateId = x.TemplateId, IsChecked = false, SystemName = x.SystemName, ReportFullName = x.ReportFullName, ReportShortName = x.ReportShortName, Options = x.Options }));

                if (PrintedDocuments.Select(x => x.TemplateId).Distinct().Count() == 1)
                {
                    SelectedPrintedDocument = PrintedDocuments.First();
                    PrintDocument();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load printed documents");
                FailureMediator.Activate("Не удалось загрузить список документов", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        string emptyValue = string.Empty;
        string defValue = "_____________________________________";
        string nonExistValue = "отсутствует";
        int childAge = 0;

        private void PrintDocument()
        {                    
            var report = reportGenerator.CreateDocX(SelectedPrintedDocument.SystemName);
            this.childAge = documentService.GetDBSettingValue(DBSetting.ChildAge).ToInt();

            #region CommonData
            report.Data["DocumentName"] = SelectedPrintedDocument.ReportFullName;
            report.Data["CurrentDate"] = DateTime.Now.ToShortDateString();
            report.Data["OrgName"] = documentService.GetDBSettingValue(DBSetting.OrgName);
            report.Data["OrgAddress"] = documentService.GetDBSettingValue(DBSetting.OrgAddress);
            report.Data["OrgShortName"] = documentService.GetDBSettingValue(DBSetting.OrgShortName);
            report.Data["OrgOKPO"] = documentService.GetDBSettingValue(DBSetting.OrgOKPO);
            report.Data["DirectorGenitive"] = documentService.GetDBSettingValue(DBSetting.DirectorFullName, true);
            report.Data["DirectorShortName"] = documentService.GetDBSettingValue(DBSetting.DirectorShortName);
            report.Data["PayContractLicense"] = documentService.GetDBSettingValue(DBSetting.PayContractLicense);
            #endregion

            var patient = (parameter != null && (parameter is FieldValue) && (parameter as FieldValue).Field == "PersonId") ? documentService.GetPersonById((parameter as FieldValue).Value).First() : null;
            var assignmentId = (parameter != null && (parameter is FieldValue) && (parameter as FieldValue).Field == "AssignmentId") ? documentService.GetAssignmentById((parameter as FieldValue).Value).First() : null;
            var recordId = (parameter != null && (parameter is FieldValue) && (parameter as FieldValue).Field == "RecordId") ? documentService.GetRecordById((parameter as FieldValue).Value).First() : null;
            var visitId = (parameter != null && (parameter is FieldValue) && (parameter as FieldValue).Field == "VisitId") ? documentService.GetVisitById((parameter as FieldValue).Value).First() : null;
            var contract = (parameter != null && (parameter is FieldValue) && (parameter as FieldValue).Field == "ContractId") ? documentService.GetContractById((parameter as FieldValue).Value).First() : null;

            if (patient != null)
            {
                #region Common Patient Data
                report.Data["PatientCardNumber"] = patient.AmbNumberString;  //TODO: or CaseNumber
                report.Data["PatientFullName"] = patient.FullName;
                report.Data["PatientShortName"] = patient.ShortName;
                var personNames = patient.PersonNames.OrderByDescending(x => x.BeginDateTime).First();
                report.Data["PatientLastName"] = personNames.LastName;
                report.Data["PatientFirstName"] = personNames.FirstName;
                report.Data["PatientMiddleName"] = personNames.MiddleName;
                report.Data["PatientBirthDate"] = patient.BirthDate.ToShortDateString() + " г.р.";
                report.Data["PatientGender"] = patient.IsMale ? "муж." : "жен.";
                report.Data["PatientContactPhone"] = patient.Phones;
                report.Data["PatientSNILS"] = patient.Snils;
                if (patient.PersonAddresses.Any(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)))
                {
                    var address = patient.PersonAddresses.Where(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)).OrderByDescending(x => x.BeginDateTime).First();
                    report.Data["PatientAddress"] = address.UserText +
                                             (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                             (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                             (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
                    report.Data["PatientRegistration"] = address.UserText.ContainsAny(", г ", ", пгт ") ? "городская" : "сельская";
                }
                else
                {
                    report.Data["PatientAddress"] = defValue;
                    report.Data["PatientRegistration"] = defValue;
                }
                if (patient.PersonIdentityDocuments.Any())
                {
                    var doc = patient.PersonIdentityDocuments.OrderByDescending(x => x.BeginDate).First();
                    report.Data["PatientIdentityDocument"] = doc.IdentityDocumentType.Name + ": " + doc.SeriesAndNumber + " выдан: " + doc.GivenOrg + " " + doc.BeginDate.ToShortDateString();
                }
                else
                    report.Data["PatientIdentityDocument"] = defValue;

                string diagnos = defValue;
                string mkb = defValue;
                if (patient.PersonDiagnoses.Any())
                {
                    var lastDiagnos = patient.PersonDiagnoses.OrderByDescending(x => x.Record.ActualDateTime).First().Diagnoses
                                                            .Select(x => new { Level = x.DiagnosLevel.ShortName, Diagnos = x.DiagnosText, MKB = x.MKB, x.DiagnosLevel.Priority}).ToArray();
                    diagnos = lastDiagnos.Select(x => x.Level + ": " + x.Diagnos + (!string.IsNullOrEmpty(x.MKB) ? " " + x.MKB : string.Empty)).Aggregate((x, y) => x + "\r\n" + y);
                    mkb = lastDiagnos.OrderBy(x => x.Priority).First().MKB;
                }
                report.Data["PatientLastDiagnos"] = diagnos;
                report.Data["PatientMKB"] = mkb;

                var relativeRelation = patient.PersonRelatives.Any(x => x.IsRepresentative) ? patient.PersonRelatives.First(x => x.IsRepresentative) : null;
                if (relativeRelation != null)
                {
                    var relative = relativeRelation.Person1;
                    report.Data["RelativeFullName"] = relative.ToString();
                    report.Data["RelativeTypeName"] = relativeRelation.RelativeRelationship.Name;
                    if (relative.PersonIdentityDocuments.Any())
                    {
                        var doc = relative.PersonIdentityDocuments.OrderByDescending(x => x.BeginDate).First();
                        report.Data["RelativeIdentityDocument"] = doc.IdentityDocumentType.Name + ": " + doc.SeriesAndNumber + " выдан: " + doc.GivenOrg + " " + doc.BeginDate.ToShortDateString();
                    }
                    else
                        report.Data["RelativeIdentityDocument"] = defValue;

                    if (relative.PersonAddresses.Any(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)))
                    {
                        var address = relative.PersonAddresses.Where(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)).OrderByDescending(x => x.BeginDateTime).First();
                        report.Data["RelativeAddress"] = address.UserText +
                                                 (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                                 (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                                 (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
                    }
                    else
                        report.Data["RelativeAddress"] = defValue;
                }

                #endregion
            }

            if (SelectedPrintedDocument.Options == OptionValues.Agreements)
                report = GetAgreementsPartReport(ref report, patient);

            if (SelectedPrintedDocument.Options == OptionValues.AmbCard)
                report = GetAmbCardPartReport(ref report, patient);

            if (SelectedPrintedDocument.Options == OptionValues.AmbTalon)
                report = GetAmbTalonPartReport(ref report, patient);

            if (SelectedPrintedDocument.Options == OptionValues.PayContract)
                report = GetPayContractPartReport(ref report, contract);

            if (SelectedPrintedDocument.Options == OptionValues.CommonCommissionJournal)
                report = GetCommissionJournalPartReport(ref report, (obj as CommissionJournalDTO[]));

            if (SelectedPrintedDocument.Options == OptionValues.HospitalisationCommissionJournal)
                report = GetHospitalisationCommissionJournalPartReport(ref report, (obj as CommissionJournalDTO[]));

            if (SelectedPrintedDocument.Options == OptionValues.KILIJournal)
                report = GetKILIJournalPartReport(ref report, (obj as CommissionJournalDTO[]));

            if (SelectedPrintedDocument.Options == OptionValues.CommonCommissionProtocol)
                report = GetCommonCommissionProtocolPartReport(ref report, (obj as CommissionJournalDTO));

            if (SelectedPrintedDocument.Options == OptionValues.ReferralToHospitalisationCommission)
                report = GetReferralHospitalisationCommissionPartReport(ref report, (obj as CommissionJournalDTO));

            if (SelectedPrintedDocument.Options == OptionValues.ReferralToCommonCommission)
                report = GetReferralToCommonCommissionPartReport(ref report, (obj as CommissionJournalDTO));

            if (SelectedPrintedDocument.Options == OptionValues.CommonConsiliumProtocol)
                report = GetCommonConsiliumProtocolPartReport(ref report, (obj as CommissionJournalDTO));

            if (SelectedPrintedDocument.Options == OptionValues.NoticeSideEffects)
                report = GetNoticeSideEffectsPartReport(ref report, patient);

            if (SelectedPrintedDocument.Options == OptionValues.SanCurCard)
                report = GetSanCurCardPartReport(ref report, patient);

            if (SelectedPrintedDocument.Options == OptionValues.ExceptCommissionProtocol)
                report = GetExceptCommissionProtocolPartReport(ref report, (obj as CommissionJournalDTO));

            if (SelectedPrintedDocument.Options == OptionValues.AssignmentsOnDate)
                report = GetAssignmentsOnDatePartReport(ref report, patient, (DateTime)obj);
            
            report.Editable = this.canEdit;
            report.Show();
        }

        private IReportGenerator GetReferralToCommonCommissionPartReport(ref IReportGenerator report, CommissionJournalDTO commissionJournalItemViewModel)
        {
            return report;
        }

        private IReportGenerator GetReferralHospitalisationCommissionPartReport(ref IReportGenerator report, CommissionJournalDTO commissionJournalItemViewModel)
        {
            return report;
        }

        private IReportGenerator GetHospitalisationCommissionJournalPartReport(ref IReportGenerator report, CommissionJournalDTO[] collection)
        {
            return report;
        }

        private IReportGenerator GetCommonConsiliumProtocolPartReport(ref IReportGenerator report, CommissionJournalDTO commissionJournalItemViewModel)
        {
            return report;
        }

        private IReportGenerator GetNoticeSideEffectsPartReport(ref IReportGenerator report, Person patient)
        {
            return report;
        }

        private IReportGenerator GetSanCurCardPartReport(ref IReportGenerator report, Person patient)
        {
            return report;
        }

        private IReportGenerator GetExceptCommissionProtocolPartReport(ref IReportGenerator report, CommissionJournalDTO commissionJournalItemViewModel)
        {
            return report;
        }

        private IReportGenerator GetCommonCommissionProtocolPartReport(ref IReportGenerator report, CommissionJournalDTO commissionJournalItemViewModel)
        {
            return report;
        }

        private IReportGenerator GetKILIJournalPartReport(ref IReportGenerator report, CommissionJournalDTO[] collection)
        {
            return report;
        }

        private IReportGenerator GetAssignmentsOnDatePartReport(ref IReportGenerator report, Person patient, DateTime onDate)
        {
            if (patient == null) return report;
            report.Data["onDate"] = onDate.ToShortDateString();            
            var assignments = patient.Assignments.Where(x => x.AssignDateTime.Date == onDate.Date).OrderBy(x => x.AssignDateTime).ThenBy(x => x.RecordType.Name);
            if (!assignments.Any())
                report.Data.Tables["tbldata"].AddRow();
            foreach (var item in assignments)
                report.Data.Tables["tbldata"].AddRow(item.AssignDateTime.ToShortTimeString(), item.Room.Number + " - " + item.Room.Name, item.RecordType.Name);
            return report;
        }        

        private IReportGenerator GetCommissionJournalPartReport(ref IReportGenerator report, CommissionJournalDTO[] collection)
        {
            if (!collection.Any())
                report.Data.Tables["tblval"].AddRow();

            foreach (var item in collection)
            {
                report.Data.Tables["tblval"].AddRow(
                    item.CommissionNumber,
                    item.ProtocolNumber,
                    item.CommissionDate,
                    item.AssignPerson,
                    item.PatientFIO,
                    item.PatientBirthDate,
                    item.PatientSocialStatus,
                    item.CardNumber,
                    item.BranchName,
                    item.PatientDiagnos,
                    item.CommissionType,
                    item.CommissionName,
                    item.Decision,
                    item.Recommendations,
                    item.Details,
                    item.Experts);
            }
            return report;
        }

        private IReportGenerator GetPayContractPartReport(ref IReportGenerator report, RecordContract contract)
        {
            if (contract == null) return report;
            report.Data["ContractNumber"] = contract.Number.ToSafeString();
            report.Data["ContractBeginDate"] = contract.BeginDateTime.ToShortDateString();
            report.Data["ContractEndDate"] = contract.EndDateTime.ToShortDateString();
            report.Data["ClientName"] = contract.Person.ToString();
            report.Data["ClientShortName"] = contract.Person.ShortName;
            report.Data["PatientShortName"] = contract.Person1.ShortName;
            if (contract.Person.PersonIdentityDocuments.Any())
            {
                var document = contract.Person.PersonIdentityDocuments.OrderByDescending(x => x.BeginDate).First();
                report.Data["ClientPassport"] = document.IdentityDocumentType.Name + ": " + document.SeriesAndNumber + ";<br>Выдан: " + document.GivenOrg + " " + document.BeginDate.ToShortDateString();
            }
            else
                report.Data["ClientPassport"] = defValue;

            if (contract.Person.PersonAddresses.Any())
            {
                var address = contract.Person.PersonAddresses.OrderByDescending(x => x.BeginDateTime).First();
                report.Data["ClientAddress"] = address.UserText +
                                             (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                             (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                             (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
            }
            else
                report.Data["ClientAddress"] = defValue;

            int index = 0;
            if (contract.RecordContractItems.Any())
            {
                foreach (var item in contract.RecordContractItems)
                    report.Data.Tables["tbldata"].AddRow(++index, item.RecordType.Name, item.Cost + " руб.");
                report.Data["TotalSum"] = contract.RecordContractItems.Sum(x => x.Cost) + " руб.";
            }
            else
            {
                report.Data.Tables["tbldata"].AddRow(defValue, defValue, defValue);
                report.Data["TotalSum"] = defValue;
            }

            return report;
        }

        private IReportGenerator GetAmbTalonPartReport(ref IReportGenerator report, Person patient)
        {
            if (patient == null) return report;
            var omsDocument = patient.InsuranceDocuments.OrderByDescending(x => x.BeginDate).FirstOrDefault();
            if (omsDocument != null)
            {
                report.Data["InsuranceDocument"] = omsDocument.InsuranceDocumentType.Name + ": серия " + omsDocument.Series + " № " + omsDocument.Number;
                report.Data["InsuranceCompany"] = omsDocument.InsuranceCompany.NameSMOK;
            }
            else
            {
                report.Data["OMSDocument"] = nonExistValue;
                report.Data["InsuranceCompany"] = nonExistValue;
            }

            if (patient.PersonDisabilities.Any(x => !x.DisabilityType.IsDisability))
                report.Data["BenefitCode"] = patient.PersonDisabilities.Where(x => !x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First().DisabilityType.BenefitCode;
            else
                report.Data["BenefitCode"] = defValue;                

            report.Data["Employment"] = patient.PersonSocialStatuses.Any() ? patient.PersonSocialStatuses.OrderByDescending(x => x.BeginDateTime).First().SocialStatusType.Name : defValue;
            if (patient.PersonSocialStatuses.Any(x => x.OrgId.HasValue))
            {
                var business = patient.PersonSocialStatuses.Where(x => x.OrgId.HasValue).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["Business"] = business.Org.Name + (!string.IsNullOrEmpty(business.Office) + (", " + business.Office) + string.Empty);
            }
            else
                report.Data["Business"] = defValue;

            if (patient.PersonDisabilities.Any(x => x.DisabilityType.IsDisability))
            {
                var disability = patient.PersonDisabilities.Where(x => x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First();
                report.Data["Disability"] = disability.DisabilityType.Name;
                report.Data["IsDisabledChild"] = disability.DisabilityType.BenefitCode == 9 ? "да" : "нет";
            }
            else
            {
                report.Data["Disability"] = nonExistValue;
                report.Data["IsDisabledChild"] = defValue;
            }
            return report;
        }       

        private IReportGenerator GetAmbCardPartReport(ref IReportGenerator report, Person patient)
        {
            if (patient == null) return report;
            report.Data["CardNumber"] = patient.AmbNumberString;
            report.Data["CardDate"] = patient.AmbNumberCreationDate.Value.ToShortDateString();
            report.Data["PatientFIO"] = patient.FullName;
            report.Data["BirthDate"] = patient.BirthDate.ToShortDateString();
            report.Data["Gender"] = patient.IsMale ? "муж. - 1" : "жен. - 2";
            if (patient.PersonAddresses.Any(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)))
            {
                var address = patient.PersonAddresses.Where(x => x.AddressType.Options.Contains(OptionValues.AddressForAmbCard)).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["Address"] = address.UserText +
                                         (!string.IsNullOrEmpty(address.House) ? " д." + address.House : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Building) ? " корп." + address.Building : string.Empty) +
                                         (!string.IsNullOrEmpty(address.Apartment) ? " кв." + address.Apartment : string.Empty);
                report.Data["Registration"] = address.UserText.ContainsAny(", г ", ", пгт ") ? "городская - 1" : "сельская – 2";
            }
            else
            {
                report.Data["Address"] = nonExistValue;
                report.Data["Registration"] = nonExistValue;
            }
            report.Data["ContactPhone"] = patient.Phones;
            report.Data["SNILS"] = patient.Snils;

            var omsDocument = patient.InsuranceDocuments.OrderByDescending(x => x.BeginDate).FirstOrDefault();
            if (omsDocument != null)
            {
                report.Data["InsuranceDocument"] = omsDocument.InsuranceDocumentType.Name + ": серия " + omsDocument.Series + " № " + omsDocument.Number;
                report.Data["InsuranceCompany"] = omsDocument.InsuranceCompany.NameSMOK;
            }
            else
            {
                report.Data["OMSDocument"] = nonExistValue;
                report.Data["InsuranceCompany"] = nonExistValue;
            }

            if (patient.PersonDisabilities.Any(x => !x.DisabilityType.IsDisability))
            {
                var benefit = patient.PersonDisabilities.Where(x => !x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First();
                report.Data["BenefitCode"] = benefit.DisabilityType.BenefitCode;
                report.Data["BenefitDocument"] = benefit.DisabilityType.Name + " (серия " + benefit.Series + " № " + benefit.Number + ")";
            }
            else
            {
                report.Data["BenefitCode"] = defValue;
                report.Data["BenefitDocument"] = defValue;
            }

            report.Data["MaritalStatus"] = patient.PersonMaritalStatuses.Any() ? patient.PersonMaritalStatuses.OrderByDescending(x => x.BeginDateTime).First().MaritalStatus.Name : defValue;
            report.Data["Education"] = patient.PersonEducations.Any() ? patient.PersonEducations.OrderByDescending(x => x.BeginDateTime).First().Education.Name : defValue;
            report.Data["Employment"] = patient.PersonSocialStatuses.Any() ? patient.PersonSocialStatuses.OrderByDescending(x => x.BeginDateTime).First().SocialStatusType.Name : defValue;
            if (patient.PersonSocialStatuses.Any(x => x.OrgId.HasValue))
            {
                var business = patient.PersonSocialStatuses.Where(x => x.OrgId.HasValue).OrderByDescending(x => x.BeginDateTime).First();
                report.Data["Business"] = business.Org.Name + (!string.IsNullOrEmpty(business.Office) + (", " + business.Office) + string.Empty);
            }
            else
                report.Data["Business"] = defValue;

            if (patient.PersonDisabilities.Any(x => x.DisabilityType.IsDisability))
            {
                var disability = patient.PersonDisabilities.Where(x => x.DisabilityType.IsDisability).OrderByDescending(x => x.BeginDate).First();
                report.Data["Disability"] = disability.DisabilityType.Name + " (серия " + disability.Series + " № " + disability.Number + ")";
            }
            else
                report.Data["Disability"] = defValue;

            report.Data["BloodGroup"] = defValue;
            report.Data["BloodRh"] = defValue;
            report.Data["Allergy"] = defValue;

            report.Data.Tables["hospdata"].AddRow(emptyValue, emptyValue, emptyValue);
            report.Data.Tables["radiationdata"].AddRow(emptyValue, emptyValue, emptyValue);

            return report;
        }

        private IReportGenerator GetAgreementsPartReport(ref IReportGenerator report, Person patient)
        {
            if (patient == null) return report;
            if (patient.BirthDate.AddYears(childAge).Date > DateTime.Now.Date)
            {
                report.Data.Sections["PatientSection"].Clear();
                report.Data["RelativeSection"] = string.Empty;
            }
            else
            {
                report.Data.Sections["RelativeSection"].Clear();
                report.Data["PatientSection"] = string.Empty;
            }
            return report;
        }

        #region IDialogViewModel

        public string Title
        {
            get { return "Печатные документы"; }
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
                if (PrintedDocuments.Any(x => x.IsChecked))
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
