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

        public int SaveContractData(RecordContract contract, out string msg)
        {
            string exception = string.Empty;
            try
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
                    saveContract.OrgDetails = contract.OrgDetails;
                    saveContract.ContractCost = contract.ContractCost;
                    saveContract.PaymentTypeId = contract.PaymentTypeId;
                    saveContract.TransactionNumber = contract.TransactionNumber;
                    saveContract.TransactionDate = contract.TransactionDate;
                    saveContract.Priority = contract.Priority;
                    saveContract.Options = contract.Options;
                    saveContract.InUserId = contract.InUserId;
                    saveContract.InDateTime = contract.InDateTime;

                    db.Entry(saveContract).State = saveContract.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                    db.SaveChanges();
                    msg = exception;
                    return saveContract.Id;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public int SaveContractItemData(RecordContractItem contractItem, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = contextProvider.CreateNewContext())
                {
                    msg = exception;
                    /*var saveContractItem = contractItem.Id > 0 ? db.GetById<RecordContractItem>(contractItem.Id) : new RecordContractItem();
                    saveContractItem.RecordContractId = contractItem.RecordContractId;
                    saveContractItem.AssignmentId = contractItem.AssignmentId;
                    saveContractItem.RecordTypeId = contractItem.RecordTypeId;
                    saveContractItem.Count = contractItem.Count;
                    saveContractItem.Cost = contractItem.Cost;
                    saveContractItem.IsPaid = contractItem.IsPaid;
                    saveContractItem.Appendix = contractItem.Appendix;
                    saveContractItem.InUserId = contractItem.InUserId;
                    saveContractItem.InDateTime = contractItem.InDateTime;

                    if (saveContractItem.Id == 0)
                        db.Add<RecordContractItem>(saveContractItem);
                    db.Save();
                    msg = exception;
                    return saveContractItem.Id;*/
                    return 0;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public void DeleteContract(int contractId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                /*var contract = db.GetById<RecordContract>(contractId);
                db.Remove<RecordContract>(contract);
                db.Save();*/
            }
        }

        public void DeleteContractItems(int contractId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                /*var contractItems = db.GetData<RecordContractItem>().Where(x => x.RecordContractId == contractId);
                db.RemoveRange<RecordContractItem>(contractItems);
                db.Save();*/
            }
        }

        public void DeleteContractItemById(int id)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                /*db.Remove<RecordContractItem>(db.GetData<RecordContractItem>().ById(id));
                db.Save();*/
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
            return context.Set<RecordContract>().Where(x => contractIds.Contains(x.Id)).SelectMany(x => x.RecordContractItems).Where(x => x.IsPaid).Sum(x => x.Cost);
        }

        public double GetContractCost(int contractId)
        {
            var context = contextProvider.CreateNewContext();
            return context.Set<RecordContract>().Where(x => x.Id == contractId).SelectMany(x => x.RecordContractItems).Where(x => x.IsPaid).Sum(x => x.Cost);
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
