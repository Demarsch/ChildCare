using System;

namespace Core.Data.Misc
{
    public static class OptionValues
    {
        //Contracts
        public static readonly string Contract = "|contract|";
        public static readonly string ResponsibleForContract = "|responsibleForContract|";
        public static readonly string Cash = "|cash|";
        public static readonly string Cashless = "|cashless|";

        //Diagnoses
        public static readonly string DiagnosSpecialistExamination = "|specialistExamination|";
        public static readonly string DiagnosDischarge = "|discharge|";

        //Rooms
        public static readonly string LaboratoryRoom = "|laboratoryRoom|";

        //AddressTypes
        public static readonly string AddressForAmbCard = "|forAmbCard|";

        //Commissions Filters
        public static readonly string ProtocolsInProcess = "|inProcess|";
        public static readonly string ProtocolsPreliminary = "|preliminary|";
        public static readonly string ProtocolsOnCommission = "|onCommission|";
        public static readonly string ProtocolsOnDate = "|onDate|";
        public static readonly string ProtocolsAdded = "|added|";
        public static readonly string ProtocolsAwaiting = "|awaiting|";
        public static readonly string CommissionFilterHasDate = "|hasDate|";
        public static readonly string SentOnCommission = "|sentOnCommission|";

        //Financing Sources
        public static readonly string OMS = "|oms|";
        public static readonly string Pay = "|pay|";
        public static readonly string Organization = "|org|";
        public static readonly string HMH = "|HMH|";
        public static readonly string Donations = "|donations|";
        public static readonly string DMS = "|dms|";
        public static readonly string IndividualContaract = "|individual|";

        //Execution Places
        public static readonly string Ambulatory = "|Ambulatory|";
        public static readonly string Stationary = "|Stationary|";
        public static readonly string DayStationary = "|DayStationary|";

        //CommissionTypeGroup
        public static readonly string MedicalCommission = "|medicalCommission|";
        public static readonly string HospitalisationCommission = "|hospitalisationCommission|";
        public static readonly string Consilium = "|consilium|";

        //PrintedDocuments
        public static readonly string AmbulatoryDocuments = "|ambulatoryDocuments|";
        public static readonly string ContractDocuments = "|contractDocuments|";
        public static readonly string Agreements = "|agreements|";
        public static readonly string CommissionJournals = "|commissionJournals|";
        public static readonly string CommonCommissionJournal = "|commonCommissionJournal|";
        public static readonly string CommissionConsiliums = "|commissionСonciliums|";
        public static readonly string CommissionProtocols = "|commissionProtocols|";
        public static readonly string CommissionMisc = "|commissionMisc|";
        public static readonly string AmbCard = "|ambCard|";
        public static readonly string AmbTalon = "|ambTalon|";
        public static readonly string AssignmentsOnDate = "|assignmentsOnDate|"; 
        public static readonly string PayContract = "|payContract|";
        public static readonly string ReferralToHospitalisationCommission = "|referralToHospitalisationCommission|";
        public static readonly string ReferralToCommonCommission = "|referralToCommonCommission|";
        public static readonly string HospitalisationCommissionJournal = "|hospitalisationCommissionJournal|";
        public static readonly string KILIJournal = "|KILIJournal|";
        public static readonly string CommonCommissionProtocol = "|commonCommisionProtocol|";
        public static readonly string CommonConsiliumProtocol = "|commonConsiliumProtocol|";
        public static readonly string NoticeSideEffects = "|noticeSideEffects|";
        public static readonly string SanCurCard = "|sanCurCard|";
        public static readonly string ExceptCommissionProtocol = "|exceptCommissionProtocol|";

        //Staffs
        public static readonly string Registrator = "|registrator|";
    }
}
