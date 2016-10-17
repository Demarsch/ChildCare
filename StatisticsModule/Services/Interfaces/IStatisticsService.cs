﻿using Core.Data;
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

        IDisposableQueryable<Record> GetRecords(DateTime beginDate, DateTime endDate, int selectedFinSourceId, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isStationary, bool isDayStationary, int employeeId);

        IDisposableQueryable<Assignment> GetAssignments(DateTime beginDate, DateTime endDate, int selectedFinSourceId, bool isAmbulatory, bool isStationary, bool isDayStationary);

        double GetRecordTypeCost(int recordTypeId, int financingSourceId, DateTime onDate, bool? isChild = null, bool isIncome = true);
    }
}
