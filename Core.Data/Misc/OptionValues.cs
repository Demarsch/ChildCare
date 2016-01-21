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

        //Commissions Filters
        public static readonly string ProtocolsInProcess = "|inProcess|";
        public static readonly string ProtocolsPreliminary = "|preliminary|";
        public static readonly string ProtocolsOnCommission = "|onCommission|";
        public static readonly string ProtocolsOnDate = "|onDate|";
        public static readonly string ProtocolsAdded = "|added|";
        public static readonly string ProtocolsAwaiting = "|awaiting|";
        public static readonly string CommissionFilterHasDate = "|hasDate|";
    }
}
