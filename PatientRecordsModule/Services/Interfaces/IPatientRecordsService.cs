using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PatientRecordsModule.Services
{
    public interface IPatientRecordsService
    {
        IDisposableQueryable<Person> GetPersonQuery(int personId);

        IDisposableQueryable<Record> GetPersonRecordsQuery(int personId);

        IDisposableQueryable<Assignment> GetPersonRootAssignmentsQuery(int personId);

        IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId);

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

        IDisposableQueryable<Urgently> GetActualUrgentlies(DateTime onDate);

        IEnumerable GetMKBs(string filter);

        Task<int> SaveVisitAsync(int visitId, int personId, DateTime beginDateTime, int recordContractId, int financingSourceId, int urgentlyId, int visitTemplateId, int executionPlaceId, int sentLPUId, string note, CancellationToken token);

        Task<int> CloseVisitAsync(int visitId, DateTime endDateTime, string MKB, int VisitOutcomeId, int VisitResultId, CancellationToken token);

        void DeleteVisitAsync(int visitId, int removedByUserId, CancellationToken token);

        void ReturnToActiveVisitAsync(int visitId, CancellationToken token);
    }
}
