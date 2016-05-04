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

    }
}
