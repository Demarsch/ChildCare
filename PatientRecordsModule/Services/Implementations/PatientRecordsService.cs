using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PatientRecordsModule.Services
{
    public class PatientRecordsService : IPatientRecordsService
    {
        private readonly IDbContextProvider contextProvider;

        public PatientRecordsService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
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

        public async Task<int> SaveVisitAsync(Visit visit, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                if (visit.Id > 0)
                    context.Entry<Visit>(visit).State = System.Data.Entity.EntityState.Modified;
                else
                    context.Entry<Visit>(visit).State = System.Data.Entity.EntityState.Added;
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
    }
}
