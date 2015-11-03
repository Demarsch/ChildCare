using Core.Data;
using Core.Data.Misc;

namespace PatientInfoModule.Services
{
    public interface IRecordService
    {
        IDisposableQueryable<RecordType> GetRecordTypeById(int id);
        IDisposableQueryable<RecordType> GetRecordTypesByOptions(string options);
        IDisposableQueryable<RecordTypeRole> GetRecordTypeRolesByOptions(string options);
        IDisposableQueryable<RecordType> GetAllRecordTypes();
        IDisposableQueryable<RecordType> GetRecordTypesByName(string name);
        double GetRecordTypeCost(int recordTypeId);
        IDisposableQueryable<FinancingSource> GetActiveFinancingSources();
        IDisposableQueryable<PaymentType> GetPaymentTypes();
        IDisposableQueryable<PaymentType> GetPaymentTypeById(int id);
        IDisposableQueryable<Visit> GetVisitsByContractId(int contractId);
    }
}
