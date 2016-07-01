using Core.Data;
using Core.Data.Misc;
using System;

namespace PatientInfoModule.Services
{
    public interface IRecordService
    {
        IDisposableQueryable<RecordType> GetRecordTypeById(int id);
        IDisposableQueryable<RecordType> GetRecordTypesByOptions(string options);
        IDisposableQueryable<RecordTypeRole> GetRecordTypeRolesByOptions(string options);
        IDisposableQueryable<RecordType> GetAllRecordTypes();
        IDisposableQueryable<RecordType> GetRecordTypesByName(string name);
        double GetRecordTypeCost(int recordTypeId, int financingSourceId, DateTime onDate, bool? isChild = null, bool isIncome = true);
        IDisposableQueryable<FinancingSource> GetActiveFinancingSources();
        IDisposableQueryable<PaymentType> GetPaymentTypes();
        IDisposableQueryable<PaymentType> GetPaymentTypeById(int id);
        IDisposableQueryable<Visit> GetVisitsByContractId(int contractId);

        string GetDBSettingValue(string parameter, bool useDisplayName = false);

        string GetPrintedDocument(string option);
    }
}
