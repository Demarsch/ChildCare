using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using Core.Misc;
using Core.Wpf.Mvvm;

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

        public int SaveContractData(RecordContract contract, int[] limitedRecordTypes)
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

                if (limitedRecordTypes.Any())
                {
                    foreach (var item in saveContract.RecordContractLimits.ToList())
                        db.Entry(item).State = EntityState.Deleted;
                    foreach (var recordTypeId in limitedRecordTypes)
                    {
                        var limitedItem = new RecordContractLimit();
                        limitedItem.RecordContractId = saveContract.Id;
                        limitedItem.RecordTypeId = recordTypeId;
                        limitedItem.Count = 0;
                        db.Entry(limitedItem).State = EntityState.Added;                       
                    }
                    db.SaveChanges();
                }

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
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().Where(x => x.Options.Contains(OptionValues.Organization) && x.IsActive), context);
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

        public IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<RecordTypeRolePermission>()
                                                               .Where(
                                                                      x =>
                                                                      x.RecordTypeId == recordTypeId && onDate >= x.BeginDateTime && onDate < x.EndDateTime && x.RecordTypeMemberRoleId == memberRoleId)
                                                               .SelectMany(x => x.Permission.PermissionGroupMemberships)
                                                               .Select(x => x.PermissionGroup)
                                                               .SelectMany(x => x.UserPermissionGroups)
                                                               .SelectMany(x => x.User.Person.PersonStaffs), context);
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

        public IDisposableQueryable<FinancingSource> GetFinancingSourceById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().Where(x => x.Id == id), context);
        }

        public IEnumerable GetPersonsByFullName(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new FieldValue[0];
            }
            var words = (filter.Contains(',') ? filter.Split(',')[0] : filter).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            using (var context = contextProvider.CreateNewContext())
                return context.Set<Person>().AsNoTracking()
                    .Select(x => new FieldValue() { Value = x.Id, Field = x.FullName + ", " + x.BirthDate.Year + " г.р." })
                    .ToArray()
                    .Where(x => words.All(y => x.Field.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1))
                    .ToArray();
        }

        public IDisposableQueryable<Person> GetPersonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == id), context);
        }


        public void RemoveRecord(int contractId, int recordTypeId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var item = db.Set<RecordContractLimit>().FirstOrDefault(x => x.RecordContractId == contractId && x.RecordTypeId == recordTypeId);
                if (item != null)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
        }
    }
}
