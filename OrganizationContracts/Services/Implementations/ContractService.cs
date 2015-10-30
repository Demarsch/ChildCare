using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;

namespace OrganizationContractsModule.Services
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
                    msg = exception;
                    /*var saveContract = contract.Id > 0 ? db.GetById<RecordContract>(contract.Id) : new RecordContract();
                    saveContract.Number = contract.Number;
                    saveContract.ContractName = contract.ContractName;
                    saveContract.BeginDateTime = contract.BeginDateTime;
                    saveContract.EndDateTime = contract.EndDateTime;
                    saveContract.FinancingSourceId = contract.FinancingSourceId;
                    saveContract.ClientId = contract.ClientId;
                    saveContract.ConsumerId = contract.ConsumerId;
                    saveContract.OrgId = contract.OrgId;
                    saveContract.ContractCost = contract.ContractCost;
                    saveContract.PaymentTypeId = contract.PaymentTypeId;
                    saveContract.TransactionNumber = contract.TransactionNumber.ToSafeString();
                    saveContract.TransactionDate = contract.TransactionDate.ToSafeString();
                    saveContract.Priority = contract.Priority;
                    saveContract.Options = contract.Options;
                    saveContract.InUserId = contract.InUserId;
                    saveContract.InDateTime = contract.InDateTime;
                    if (saveContract.Id == 0)
                        db.Add<RecordContract>(saveContract);
                    db.Save();
                    msg = exception;
                    return saveContract.Id;*/
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

        public IDisposableQueryable<RecordContract> GetContractById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<Org> GetOrganizations()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Org>(context.Set<Org>(), context);
        }

        public IDisposableQueryable<RecordContract> GetContractsWithOrgs(DateTime begin, DateTime end, int finSourceId = -1)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().Where(x => x.OrgId.HasValue 
                        && DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(end) && DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(begin)
                        && (finSourceId == -1 ? true : x.FinancingSourceId == finSourceId)), context);
        }

        public IDisposableQueryable<PaymentType> GetPaymentTypes()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PaymentType>(context.Set<PaymentType>(), context);
        }

        public IDisposableQueryable<Org> GetOrganizationById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Org>(context.Set<Org>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<FinancingSource> GetActiveFinancingSources()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().Where(x => x.IsOrgContract && x.IsActive), context);
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

        public IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<RecordTypeRolePermission>()
                .Where(x => x.RecordTypeId == recordTypeId && x.RecordTypeMemberRoleId == memberRoleId)
                    .SelectMany(x => x.Permission.UserPermissions.SelectMany(a => a.User.Person.PersonStaffs)), context);
        }

        public IDisposableQueryable<Visit> GetVisitsByContractId(int contractId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().Where(x => x.ContractId == contractId), context);
        }
    }
}
