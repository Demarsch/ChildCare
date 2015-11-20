using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;

namespace PatientInfoModule.Services
{
    public class ContractService : IContractService
    {
        private readonly IDbContextProvider contextProvider;

        public ContractService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public int SaveContractData(RecordContract contract)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var saveContract = contract.Id == SpecialValues.NewId ? new RecordContract() : db.Set<RecordContract>().First(x => x.Id == contract.Id);
                saveContract.Number = contract.Number;
                saveContract.ContractName = contract.ContractName;
                saveContract.BeginDateTime = contract.BeginDateTime;
                saveContract.EndDateTime = contract.EndDateTime;
                saveContract.FinancingSourceId = contract.FinancingSourceId;
                saveContract.ClientId = contract.ClientId;
                saveContract.ConsumerId = contract.ConsumerId;
                saveContract.OrgId = contract.OrgId;
                saveContract.OrgDetails = contract.OrgDetails == null ? string.Empty : contract.OrgDetails;
                saveContract.ContractCost = contract.ContractCost;
                saveContract.PaymentTypeId = contract.PaymentTypeId;
                saveContract.TransactionNumber = contract.TransactionNumber == null ? string.Empty : contract.TransactionNumber;
                saveContract.TransactionDate = contract.TransactionDate == null ? string.Empty : contract.TransactionDate;
                saveContract.Priority = contract.Priority;
                saveContract.Options = contract.Options;
                saveContract.InUserId = contract.InUserId;
                saveContract.InDateTime = contract.InDateTime;
                db.Entry(saveContract).State = saveContract.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;

                foreach (var item in contract.RecordContractItems)
                {
                    var contractItem = item.Id == SpecialValues.NewId ? new RecordContractItem() : db.Set<RecordContractItem>().First(x => x.Id == item.Id);
                    contractItem.RecordContract = saveContract;
                    contractItem.AssignmentId = item.AssignmentId;
                    contractItem.RecordTypeId = item.RecordTypeId;
                    contractItem.Count = item.Count;
                    contractItem.Cost = item.Cost;
                    contractItem.IsPaid = item.IsPaid;
                    contractItem.Appendix = item.Appendix;
                    contractItem.InUserId = item.InUserId;
                    contractItem.InDateTime = item.InDateTime;
                    db.Entry(contractItem).State = contractItem.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                }
                db.SaveChanges();
                return saveContract.Id;
            }

        }

        public void DeleteContract(int contractId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var contract = db.Set<RecordContract>().First(x => x.Id == contractId);
                db.Entry(contract).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public void DeleteContractItemById(int id)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var contractItem = db.Set<RecordContractItem>().First(x => x.Id == id);
                db.Entry(contractItem).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public IDisposableQueryable<RecordContract> GetContracts(int? consumerId = null, DateTime? fromDate = null, DateTime? toDate = null, int inUserId = -1)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>()
                .Where(x => x.ClientId.HasValue && x.ClientId != 0 && !x.OrgId.HasValue
                        && (consumerId.HasValue ? x.ConsumerId == consumerId.Value : true)
                        && ((fromDate.HasValue && toDate.HasValue) ? DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(toDate.Value) && DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(fromDate.Value) : true)
                        && (inUserId != -1 ? x.InUserId == inUserId : true)), context);
        }

        public IDisposableQueryable<RecordContractItem> GetContractItems(int contractId, int? appendix = null)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContractItem>(context.Set<RecordContractItem>()
                .Where(x => x.RecordContractId == contractId && (appendix.HasValue ? x.Appendix == appendix.Value : true)), context);
        }

        public double GetContractCost(int[] contractIds)
        {
            var context = contextProvider.CreateNewContext();
            var paidRecordContractItems = context.Set<RecordContract>().Where(x => contractIds.Contains(x.Id)).SelectMany(x => x.RecordContractItems).Where(x => x.IsPaid);
            return paidRecordContractItems.Any() ? paidRecordContractItems.Sum(x => x.Cost) : 0.0;
        }

        public double GetContractCost(int contractId)
        {
            var context = contextProvider.CreateNewContext();
            var paidRecordContractItems = context.Set<RecordContract>().Where(x => x.Id == contractId).SelectMany(x => x.RecordContractItems).Where(x => x.IsPaid);
            return paidRecordContractItems.Any() ? paidRecordContractItems.Sum(x => x.Cost) : 0.0;
        }

        public IDisposableQueryable<RecordContract> GetContractById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordContractItem> GetContractItemById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContractItem>(context.Set<RecordContractItem>().Where(x => x.Id == id), context);
        }
    }
}
