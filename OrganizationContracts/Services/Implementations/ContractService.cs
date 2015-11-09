using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;

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

        public async Task<int> SaveOrgAsync(Org org)
        {           
            using (var db = contextProvider.CreateNewContext())
            {                
                db.Entry<Org>(org).State = org.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;  
                await db.SaveChangesAsync();
                return org.Id;
            }
        }
    }
}
