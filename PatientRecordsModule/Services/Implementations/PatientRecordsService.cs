using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Core.Misc;
using Core.Services;
using System.Collections.Generic;
using PatientRecordsModule.DTO;

namespace PatientRecordsModule.Services
{
    public class PatientRecordsService : IPatientRecordsService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ICacheService cacheService;

        public PatientRecordsService(IDbContextProvider contextProvider, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.contextProvider = contextProvider;
            this.cacheService = cacheService;
        }

        public IDisposableQueryable<Person> GetPersonQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().AsNoTracking().Where(x => x.Id == personId), context);
        }

        public IDisposableQueryable<Record> GetPersonRecordsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().AsNoTracking().Where(x => x.PersonId == personId && x.RemovedByUserId == null), context);
        }

        public IDisposableQueryable<Assignment> GetPersonRootAssignmentsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.PersonId == personId && !x.VisitId.HasValue && !x.RecordId.HasValue && x.RemovedByUserId == null), context);
        }

        public IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().AsNoTracking().Where(x => x.PersonId == personId && x.RemovedByUserId == null), context);
        }

        public IDisposableQueryable<Assignment> GetVisitsChildAssignmentsQuery(int visitId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.VisitId == visitId && !x.RecordId.HasValue && x.RemovedByUserId == null), context);
        }

        public IDisposableQueryable<Record> GetVisitsChildRecordsQuery(int visitId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().AsNoTracking().Where(x => x.VisitId == visitId && x.RemovedByUserId == null), context);
        }


        public IDisposableQueryable<Assignment> GetRecordsChildAssignmentsQuery(int recordId)
        {
            //ToDo: Create a field for parent record for Assignment if we need this
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => false), context);
        }

        public IDisposableQueryable<Record> GetRecordsChildRecordsQuery(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().AsNoTracking().Where(x => x.ParentId == recordId && x.RemovedByUserId == null), context);
        }


        public IDisposableQueryable<Assignment> GetAssignmentsChildAssignmentsQuery(int assignmentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.ParentId == assignmentId && x.RemovedByUserId == null), context);
        }


        public IDisposableQueryable<VisitTemplate> GetActualVisitTemplates(DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<VisitTemplate>(context.Set<VisitTemplate>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }


        public IDisposableQueryable<RecordContract> GetActualRecordContracts(DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }

        public IDisposableQueryable<FinancingSource> GetActualFinancingSources()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().AsNoTracking().Where(x => x.IsActive), context);
        }

        public IDisposableQueryable<ExecutionPlace> GetActualExecutionPlaces()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<ExecutionPlace>(context.Set<ExecutionPlace>().AsNoTracking().Where(x => x.IsActive), context);
        }

        public IDisposableQueryable<Urgently> GetActualUrgentlies(DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Urgently>(context.Set<Urgently>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }


        public IDisposableQueryable<VisitTemplate> GetVisitTemplate(int visitTemplateId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<VisitTemplate>(context.Set<VisitTemplate>().AsNoTracking().Where(x => x.Id == visitTemplateId), context);
        }

        public IDisposableQueryable<Org> GetLPUs()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Org>(context.Set<Org>().AsNoTracking().Where(x => x.IsLpu), context);
        }

        public IDisposableQueryable<Visit> GetVisit(int visitId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().AsNoTracking().Where(x => x.Id == visitId && x.RemovedByUserId == null), context);
        }

        public IDisposableQueryable<Record> GetRecord(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().AsNoTracking().Where(x => x.Id == recordId && x.RemovedByUserId == null), context);
        }

        public async Task<int> SaveVisitAsync(int visitId, int personId, DateTime beginDateTime, int recordContractId, int financingSourceId, int urgentlyId, int visitTemplateId, int executionPlaceId, int sentLPUId, string note,
            CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                Visit visit = context.Set<Visit>().FirstOrDefault(x => x.Id == visitId);
                if (visit == null)
                {
                    visit = new Visit()
                    {
                        MKB = string.Empty,
                        OKATO = string.Empty,
                        PersonId = personId,
                        TotalCost = 0
                    };
                    context.Entry<Visit>(visit).State = System.Data.Entity.EntityState.Added;
                }
                visit.ContractId = recordContractId;
                visit.FinancingSourceId = financingSourceId;
                visit.UrgentlyId = urgentlyId;
                visit.VisitTemplateId = visitTemplateId;
                visit.ExecutionPlaceId = executionPlaceId;
                visit.SentLPUId = sentLPUId;
                visit.BeginDateTime = beginDateTime;
                visit.Note = note;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
                return visit.Id;
            }
        }



        public async Task<int> CloseVisitAsync(int visitId, DateTime endDateTime, string MKB, int VisitOutcomeId, int VisitResultId, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                Visit visit = context.Set<Visit>().FirstOrDefault(x => x.Id == visitId);
                visit.EndDateTime = endDateTime;
                visit.MKB = MKB;
                visit.VisitOutcomeId = VisitOutcomeId;
                visit.VisitResultId = VisitResultId;
                visit.IsCompleted = true;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
                return visit.Id;
            }
        }

        public async void DeleteVisitAsync(int visitId, int removedByUserId, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                if (visitId < 1) return;
                var visit = context.Set<Visit>().FirstOrDefault(x => x.Id == visitId);
                visit.RemovedByUserId = removedByUserId;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
            }
        }

        public async void ReturnToActiveVisitAsync(int visitId, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                if (visitId < 1) return;
                var visit = context.Set<Visit>().FirstOrDefault(x => x.Id == visitId);
                visit.IsCompleted = false;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
            }
        }

        public IEnumerable GetMKBs(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new MKB[0];
            }
            var words = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            return cacheService.GetItems<MKB>().Where(x => words.All(y => x.Code.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1 || x.Name.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));
        }

        public MKB GetMKB(string code)
        {
            var context = contextProvider.CreateNewContext();
            return cacheService.GetItems<MKB>().FirstOrDefault(x => x.Code == code);
        }

        public IDisposableQueryable<VisitResult> GetActualVisitResults(int executionPlaceId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<VisitResult>(context.Set<VisitResult>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime && x.ExecutionPlaceId == executionPlaceId), context);
        }

        public IDisposableQueryable<VisitOutcome> GetActualVisitOutcomes(int executionPlaceId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<VisitOutcome>(context.Set<VisitOutcome>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime && x.ExecutionPlaceId == executionPlaceId), context);
        }


        public IDisposableQueryable<RecordMember> GetRecordMembers(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordMember>(context.Set<RecordMember>().AsNoTracking().Where(x => x.RecordId == recordId && x.IsActive), context);
        }

        public IDisposableQueryable<RecordTypeRolePermission> GetRecordTypeMembers(int recordTypeId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordTypeRolePermission>(context.Set<RecordTypeRolePermission>().AsNoTracking().Where(x => x.RecordTypeId == recordTypeId && onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }


        public ICollection<CommonIdName> GetAllowedPersonStaffs(int recordTypeId, int recordTypeRoleId, DateTime onDate)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var permissions = context.Set<RecordTypeRolePermission>().AsNoTracking().Where(x => x.RecordTypeId == recordTypeId && onDate >= x.BeginDateTime && onDate < x.EndDateTime &&
                x.RecordTypeMemberRoleId == recordTypeRoleId).Select(x => x.Permission).ToList();
                return GetAllUserPermissions(permissions, new List<Permission>()).SelectMany(x => x.UserPermissions.Where(y => onDate >= y.BeginDateTime && onDate < y.EndDateTime).SelectMany(y => y.User.Person.PersonStaffs)).Distinct()
                    .Select(x => new CommonIdName() { Id = x.Id, Name = x.Staff.ShortName + x.Person.ShortName }).ToList();
            }
        }

        private List<Permission> GetAllUserPermissions(List<Permission> permissions, List<Permission> allPermissions)
        {
            allPermissions.AddRange(permissions);
            foreach (var permission in permissions.Where(x => x != null))
                allPermissions.AddRange(GetAllUserPermissions(permission.PermissionLinks.Select(x => x.Permission1).ToList(), allPermissions));
            return allPermissions;
        }
    }
}
