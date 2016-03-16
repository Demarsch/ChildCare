using System;
using System.Data.Entity;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;

namespace PatientInfoModule.Services
{
    public class RecordService : IRecordService
    {
        private readonly IDbContextProvider contextProvider;

        public RecordService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<RecordType> GetRecordTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordType> GetRecordTypesByOptions(string options)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().Where(x => x.Options.Contains(options)), context);
        }

        public IDisposableQueryable<RecordTypeRole> GetRecordTypeRolesByOptions(string options)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordTypeRole>(context.Set<RecordTypeRole>().Where(x => x.Options.Contains(options)), context);
        }

        public IDisposableQueryable<RecordType> GetAllRecordTypes()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>(), context);
        }

        public IDisposableQueryable<RecordType> GetRecordTypesByName(string name)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().Where(x => x.Name.ToLower().Trim().Contains(name.ToLower().Trim()))
                                                              .Take(AppConfiguration.SearchResultTakeTopCount), context);
        }

        public IDisposableQueryable<FinancingSource> GetActiveFinancingSources()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().Where(x => !string.IsNullOrEmpty(x.Options) && x.IsActive), context);
        }

        public IDisposableQueryable<PaymentType> GetPaymentTypes()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PaymentType>(context.Set<PaymentType>(), context);
        }

        public IDisposableQueryable<PaymentType> GetPaymentTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PaymentType>(context.Set<PaymentType>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<Visit> GetVisitsByContractId(int contractId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().Where(x => x.ContractId == contractId), context);
        }

        public double GetRecordTypeCost(int recordTypeId, int financingSourceId, DateTime onDate, bool? isChild = null, bool isIncome = true)
        {
            var context = contextProvider.CreateNewContext();
            var cost = 0.0;
            var recordCost = context.Set<RecordTypeCost>()
                                    .Where(x => x.RecordTypeId == recordTypeId && x.FinancingSourceId == financingSourceId &&
                                                DbFunctions.TruncateTime(onDate) >= DbFunctions.TruncateTime(x.BeginDate) && DbFunctions.TruncateTime(onDate) < DbFunctions.TruncateTime(x.EndDate) &&
                                                x.IsIncome == isIncome)
                                    .OrderByDescending(x => x.InDateTime)
                                    .FirstOrDefault(x => (x.IsChild != null ? x.IsChild == isChild : true));
            if (recordCost != null)
            {
                cost = recordCost.FullPrice * recordCost.Profitability;
            }
            return cost;
        }
    }
}