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
using Shared.PatientRecords.DTO;
using System.Data.Entity;

namespace Shared.PatientRecords.Services
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
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.PersonId == personId && !x.VisitId.HasValue && !x.RecordId.HasValue && x.RemovedByUserId == null && !x.CancelUserId.HasValue), context);
        }

        public IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId, bool onlyOpened)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().AsNoTracking().Where(x => x.PersonId == personId && x.RemovedByUserId == null && (!onlyOpened || (x.IsCompleted == null || x.IsCompleted == false))), context);
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

        public void CompleteRecordAsync(int recordId, CancellationToken token)
        {
            ChangeCompleteState(recordId, true, token);
        }

        public void InProgressRecordAsync(int recordId, CancellationToken token)
        {
            ChangeCompleteState(recordId, false, token);
        }

        private async void ChangeCompleteState(int recordId, bool isCompleted, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                Record record = context.Set<Record>().FirstOrDefault(x => x.Id == recordId);
                record.IsCompleted = isCompleted;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
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
                visit.EndDateTime = null;
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
                return GetAllUserPermissions(permissions, new List<Permission>()).SelectMany(x => x.UserPermissions).Where(y => onDate >= y.BeginDateTime && onDate < y.EndDateTime).SelectMany(y => y.User.Person.PersonStaffs).Distinct()
                    .Select(x => new CommonIdName() { Id = x.Id, Name = x.Staff.ShortName + " " + x.Person.ShortName }).ToList();
            }
        }

        private List<Permission> GetAllUserPermissions(List<Permission> permissions, List<Permission> allPermissions)
        {
            allPermissions.AddRange(permissions);
            foreach (var permission in permissions.Where(x => x != null))
                allPermissions.Union(GetAllUserPermissions(permission.PermissionLinks.Select(x => x.Permission1).ToList(), allPermissions));
            return allPermissions;
        }


        public IDisposableQueryable<Assignment> GetAssignment(int assignmentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.Id == assignmentId), context);
        }


        public IDisposableQueryable<RecordPeriod> GetActualRecordPeriods(int executionPlaceId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordPeriod>(context.Set<RecordPeriod>().AsNoTracking().Where(x => x.ExecutionPlaceId == executionPlaceId && onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }


        public IDisposableQueryable<PersonStaff> GetPersonStaff(int personStaffId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<PersonStaff>().AsNoTracking().Where(x => x.Id == personStaffId), context);
        }

        public IDisposableQueryable<Room> GetRooms(DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Room>(context.Set<Room>().AsNoTracking().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }

        public async Task<int> SaveRecordCommonDataAsync(int recordId, int recordTypeId, int personId, int visitId, int roomId, int periodId, int urgentlyId, DateTime beginDateTime, DateTime? endDateTime,
            List<RecordMember> brigade, int assignmentId, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                Record record = context.Set<Record>().FirstOrDefault(x => x.Id == recordId);
                if (record == null)
                {
                    RecordType recordType = context.Set<RecordType>().FirstOrDefault(x => x.Id == recordTypeId);
                    record = new Record()
                    {
                        PersonId = personId,
                        RecordTypeId = recordTypeId,
                        IsCompleted = false,
                        MKB = string.Empty,
                        NumberYear = endDateTime.HasValue ? endDateTime.Value.Year : beginDateTime.Year,
                        NumberType = recordType.NumberType
                    };
                    var number = context.Set<Record>().Any(x => x.NumberType == record.NumberType && x.NumberYear == record.NumberYear) ?
                        context.Set<Record>().Where(x => x.NumberType == record.NumberType && x.NumberYear == record.NumberYear).Max(x => x.Number) + 1 : 1;
                    record.Number = number;
                    context.Entry<Record>(record).State = EntityState.Added;
                }
                record.RoomId = roomId;
                record.VisitId = visitId;
                record.RecordPeriodId = periodId;
                record.UrgentlyId = urgentlyId;
                record.BeginDateTime = beginDateTime;
                record.EndDateTime = endDateTime;
                record.ActualDateTime = endDateTime ?? beginDateTime;

                //Brigade
                var old = record.RecordMembers.Where(x => x.IsActive).ToDictionary(x => x.Id);
                var @new = brigade.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
                var added = brigade.Where(x => x.Id == SpecialValues.NewId).ToArray();
                var removed = record.RecordMembers.Where(x => !@new.ContainsKey(x.Id))
                                 .ToArray();
                var existed = @new.Where(x => old.ContainsKey(x.Key))
                                  .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                                  .ToArray();
                foreach (var member in added)
                {
                    member.RecordId = record.Id;
                    context.Entry(member).State = EntityState.Added;
                }
                foreach (var member in removed)
                {
                    member.IsActive = false;
                }
                foreach (var member in existed.Where(x => x.IsChanged))
                {
                    member.Old.IsActive = member.New.IsActive;
                    member.Old.RecordId = member.New.RecordId;
                    member.Old.PersonStaffId = member.New.PersonStaffId;
                    member.Old.RecordTypeRolePermissionId = member.New.RecordTypeRolePermissionId;
                    context.Entry(member.Old).State = EntityState.Modified;
                }
                //result.Addresses = added.Concat(existed.Select(x => x.New)).ToArray();
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);

                if (!SpecialValues.IsNewOrNonExisting(assignmentId))
                {
                    var assignment = context.Set<Assignment>().FirstOrDefault(x => x.Id == assignmentId);
                    assignment.RecordId = record.Id;
                    assignment.VisitId = visitId;
                    await context.SaveChangesAsync(token);
                }

                return record.Id;
            }
        }


        public Task<bool> IsBrigadeCompleted(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return context.Set<Record>().AsNoTracking().AnyAsync(x => x.Id == recordId &&
                x.RecordType.RecordTypeRolePermissions.Where(y => x.ActualDateTime >= y.BeginDateTime && x.ActualDateTime < y.EndDateTime && y.IsRequired).All(y => y.RecordMembers.Any(z => z.RecordId == recordId && z.IsActive)));
        }

        public IDisposableQueryable<RecordType> GetRecordTypes(bool isAnalyse = false)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().AsNoTracking().Where(x => isAnalyse ? x.IsAnalyse == isAnalyse : true), context);
        }

        public IDisposableQueryable<RecordType> GetChildRecordTypesQuery(int recordTypeId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().AsNoTracking().Where(x => x.Id == recordTypeId).SelectMany(x => x.RecordTypes1), context);
        }

        public async Task<int> CreateAnalyseAssignmentAsync(Assignment assignment, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                Assignment savedAssignment = new Assignment();
                context.Entry<Assignment>(savedAssignment).State = EntityState.Added;
                savedAssignment.ParentId = assignment.ParentId;
                savedAssignment.RecordTypeId = assignment.RecordTypeId;
                savedAssignment.PersonId = assignment.PersonId;
                savedAssignment.AssignDateTime = assignment.AssignDateTime;
                savedAssignment.Duration = assignment.Duration;
                savedAssignment.AssignUserId = assignment.AssignUserId;
                savedAssignment.AssignLpuId = assignment.AssignLpuId;
                savedAssignment.RoomId = assignment.RoomId;
                savedAssignment.FinancingSourceId = assignment.FinancingSourceId;
                savedAssignment.UrgentlyId = assignment.UrgentlyId;
                savedAssignment.ExecutionPlaceId = assignment.ExecutionPlaceId;
                savedAssignment.ParametersOptions = assignment.ParametersOptions;
                savedAssignment.CancelUserId = assignment.CancelUserId;
                savedAssignment.CancelDateTime = assignment.CancelDateTime;
                savedAssignment.Note = assignment.Note;
                savedAssignment.RecordId = assignment.RecordId;
                savedAssignment.VisitId = assignment.VisitId;
                savedAssignment.IsTemporary = assignment.IsTemporary;
                savedAssignment.CreationDateTime = assignment.CreationDateTime;
                savedAssignment.BillingDateTime = assignment.BillingDateTime;
                savedAssignment.Cost = assignment.Cost;
                savedAssignment.RemovedByUserId = assignment.RemovedByUserId;

                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
                return savedAssignment.Id;
            }
        }

        public IDisposableQueryable<RecordType> GetRecordTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().Where(x => x.Id == id), context);
        }

        public void UpdateMKBRecord(int recordId, string mkb)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var record = context.Set<Record>().FirstOrDefault(x => x.Id == recordId);
                if (record == null) return;
                record.MKB = mkb;
                record.Visit.MKB = mkb;
                context.SaveChanges();
            }
        }

        public int SaveDefaultProtocol(DefaultProtocol defaultProtocol)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var saveProtocol = defaultProtocol.Id == SpecialValues.NewId ? new DefaultProtocol() : context.Set<DefaultProtocol>().First(x => x.Id == defaultProtocol.Id);
                saveProtocol.RecordId = defaultProtocol.RecordId;
                saveProtocol.Description = defaultProtocol.Description;
                saveProtocol.Conclusion = defaultProtocol.Conclusion;
                context.Entry(saveProtocol).State = saveProtocol.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                context.SaveChanges();
                return saveProtocol.Id;
            }
        }

        public int[] SaveAnalyseResult(int recordId, AnalyseResult[] analyseResults)
        {            
            using (var context = contextProvider.CreateNewContext())
            {
                var old = context.Set<AnalyseResult>().Where(x => x.RecordId == recordId).ToDictionary(x => x.Id);
                var @new = analyseResults.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
                var added = analyseResults.Where(x => x.Id == SpecialValues.NewId && !string.IsNullOrEmpty(x.Value)).ToArray();
                var existed = @new.Where(x => old.ContainsKey(x.Key))
                                  .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                                  .ToArray();
                foreach (var result in added)
                {
                    result.RecordId = recordId;
                    context.Entry(result).State = EntityState.Added;
                }
                foreach (var result in existed.Where(x => x.IsChanged))
                {
                    if (!string.IsNullOrEmpty(result.Old.Value) && string.IsNullOrEmpty(result.New.Value))
                    {
                        result.Old.Id = SpecialValues.NewId;
                        context.Entry(result.Old).State = EntityState.Deleted;
                    }
                    else
                    {
                        result.Old.ParameterRecordTypeId = result.New.ParameterRecordTypeId;
                        result.Old.Value = result.New.Value;
                        result.Old.UnitId = result.New.UnitId;
                        result.Old.IsNormal = result.New.IsNormal;
                        result.Old.IsAboveRef = result.New.IsAboveRef;
                        result.Old.IsBelowRef = result.New.IsBelowRef;
                        result.Old.Details = result.New.Details;
                        context.Entry(result.Old).State = EntityState.Modified;
                    }
                }
                try
                {
                    context.SaveChanges();
                    return analyseResults.Select(x => x.Id).ToArray();
                }
                catch
                {
                    return new int[0];
                }
            }
        }
        
        public IDisposableQueryable<AnalyseRefference> GetAnalyseReference(int recordTypeId, int parameterRecordTypeId, bool isMale, int age, DateTime date)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<AnalyseRefference>(context.Set<AnalyseRefference>()
                .Where(x => x.RecordTypeId == recordTypeId && x.ParameterRecordTypeId == parameterRecordTypeId && x.IsMale == isMale && 
                            x.AgeFrom <= age && x.AgeTo >= age && x.BeginDateTime <= date && x.EndDateTime > date), context);
        }


        public IDisposableQueryable<AnalyseResult> GetAnalyseResults(int personId, int recordTypeId, int parameterRecordTypeId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<AnalyseResult>(context.Set<AnalyseResult>()
                        .Where(x => x.Record.PersonId == personId && x.Record.RecordTypeId == recordTypeId && x.ParameterRecordTypeId == parameterRecordTypeId), context);
        }
    }
}
