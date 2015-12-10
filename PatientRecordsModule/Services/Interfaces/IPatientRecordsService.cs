using Core.Data;
using Core.Data.Misc;
using Shared.PatientRecords.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.PatientRecords.Services
{
    public interface IPatientRecordsService
    {
        IDisposableQueryable<Person> GetPersonQuery(int personId);

        IDisposableQueryable<Record> GetPersonRecordsQuery(int personId);

        IDisposableQueryable<Assignment> GetPersonRootAssignmentsQuery(int personId);

        IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId, bool onlyOpened);

        IDisposableQueryable<Assignment> GetVisitsChildAssignmentsQuery(int visitId);

        IDisposableQueryable<Record> GetVisitsChildRecordsQuery(int visitId);

        IDisposableQueryable<Assignment> GetRecordsChildAssignmentsQuery(int recordId);

        IDisposableQueryable<Record> GetRecordsChildRecordsQuery(int recordId);

        IDisposableQueryable<Assignment> GetAssignmentsChildAssignmentsQuery(int assignmentId);

        IDisposableQueryable<VisitTemplate> GetActualVisitTemplates(DateTime onDate);

        IDisposableQueryable<VisitTemplate> GetVisitTemplate(int visitTemplateId);

        IDisposableQueryable<RecordContract> GetActualRecordContracts(DateTime onDate);

        IDisposableQueryable<FinancingSource> GetActualFinancingSources();

        IDisposableQueryable<ExecutionPlace> GetActualExecutionPlaces();

        IDisposableQueryable<VisitResult> GetActualVisitResults(int executionPlaceId, DateTime onDate);

        IDisposableQueryable<VisitOutcome> GetActualVisitOutcomes(int executionPlaceId, DateTime onDate);


        IDisposableQueryable<Org> GetLPUs();

        MKB GetMKB(string code);

        IDisposableQueryable<Visit> GetVisit(int visitId);

        IDisposableQueryable<Record> GetRecord(int recordId);

        Task<bool> IsBrigadeCompleted(int recordId);

        IDisposableQueryable<Assignment> GetAssignment(int assignmentId);

        IDisposableQueryable<Urgently> GetActualUrgentlies(DateTime onDate);

        IDisposableQueryable<Room> GetRooms(DateTime onDate);

        IDisposableQueryable<RecordPeriod> GetActualRecordPeriods(int executionPlaceId, DateTime onDate);

        ICollection<CommonIdName> GetAllowedPersonStaffs(int recordTypeId, int recordTypeRoleId, DateTime onDate);

        IDisposableQueryable<PersonStaff> GetPersonStaff(int personStaffId);

        IEnumerable GetMKBs(string filter);

        IDisposableQueryable<RecordMember> GetRecordMembers(int recordId);

        IDisposableQueryable<RecordTypeRolePermission> GetRecordTypeMembers(int recordTypeId, DateTime onDate);

        Task<int> SaveVisitAsync(int visitId, int personId, DateTime beginDateTime, int recordContractId, int financingSourceId, int urgentlyId, int visitTemplateId, int executionPlaceId, int sentLPUId, string note, CancellationToken token);

        Task<int> CloseVisitAsync(int visitId, DateTime endDateTime, string MKB, int VisitOutcomeId, int VisitResultId, CancellationToken token);

        Task<int> SaveRecordCommonDataAsync(int recordId, int recordTypeId, int personId, int visitId, int roomId, int periodId, int urgentlyId, DateTime beginDateTime, DateTime? endDateTime, List<RecordMember> brigade, CancellationToken token);

        void DeleteVisitAsync(int visitId, int removedByUserId, CancellationToken token);

        void ReturnToActiveVisitAsync(int visitId, CancellationToken token);

        void CompleteRecordAsync(int recordId, CancellationToken token);

        void InProgressRecordAsync(int recordId, CancellationToken token);

        IDisposableQueryable<RecordType> GetRecordTypes(bool isAnalyse = false);

        IDisposableQueryable<RecordType> GetChildRecordTypesQuery(int recordTypeId);

        Task<int> CreateAnalyseAssignmentAsync(Assignment assignment, CancellationToken token);

        IDisposableQueryable<RecordType> GetRecordTypeById(int id);
    }
}
