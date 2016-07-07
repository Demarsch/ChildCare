using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shared.Commissions.Services
{
    public interface ICommissionService
    {  
        IEnumerable<CommissionTypeGroup> GetCommissionTypeGroups(object onDate);

        IEnumerable<CommissionType> GetCommissionTypes(object onDate, int commissionTypeGroupId = -1);

        IEnumerable<CommissionQuestion> GetCommissionQuestions(object onDate, int commissionTypeId = -1);
        
        IDisposableQueryable<Person> GetPerson(int personId);

        Task<int> CreateCommissionAssignment(int personId, DateTime commissionDate, int commissionTypeId, int commissionQuestionId, string codeMKB, CancellationToken token);
    }
}
