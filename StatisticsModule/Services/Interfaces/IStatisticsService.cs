using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.Services
{
    public interface IStatisticsService
    {
        IDisposableQueryable<FinancingSource> GetActualFinancingSources();

        IDisposableQueryable<PersonStaff> GetPersonStaffs();

        IDisposableQueryable<Record> GetRecords(DateTime beginDate, DateTime endDate, int selectedFinSourceId, string codeOKATO, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isStationary, bool isDayStationary, int employeeId);

        IDisposableQueryable<Visit> GetVisits(DateTime beginDate, DateTime endDate, int selectedFinSourceId, string codeOKATO, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isPlanned, bool isStationary, bool isDayStationary);

        IDisposableQueryable<Assignment> GetAssignments(DateTime beginDate, DateTime endDate, int selectedFinSourceId, bool isAmbulatory, bool isStationary, bool isDayStationary);

        IDisposableQueryable<Assignment> GetAssignments(DateTime beginDate, DateTime endDate, int employeeId);

        double GetRecordTypeCost(int recordTypeId, int financingSourceId, DateTime onDate, bool? isChild = null, bool isIncome = true);

        IEnumerable<MKBGroup> GetMKBGroups();

        bool MKBFilter(string filter, string mkb);
    }
}
