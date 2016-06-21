using Core.Data;
using Core.Data.Misc;
using Core.Notification;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.Services
{
    public interface ICommissionService
    {
        IDisposableQueryable<CommissionFilter> GetCommissionFilters(string options = "");

        CommissionFilter GetCommissionFilterById(int id);

        bool IsCommissionFilterHasDate(int id);

        IDisposableQueryable<CommissionProtocol> GetCommissionProtocols(int filterId, DateTime? date = null, bool onlyMyCommissions = false);

        IDisposableQueryable<CommissionDecision> GetCommissionDecisions(int commissionProtocolId);

        IDisposableQueryable<CommissionMember> GetCommissionMembers(int commissionTypeId, DateTime onDate);

        IDisposableQueryable<CommissionMember> GetCommissionMember(int commissionMemberId);

        string GetDecisionColorHex(int? decisionId);

        IDisposableQueryable<CommissionDecision> GetCommissionDecision(int commissionDecisionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommissionQuestionIdAndCommissionTypeMemberId">Tuple<int, int, DateTime> with commissionQuestionId and commissionTypeMemberId and onDate (SpecialValue.NotExist for int items is allDecisions)</param>
        /// <returns></returns>
        IEnumerable<Decision> GetDecisions(object commissionQuestionIdAndCommissionTypeMemberId);

        IDisposableQueryable<CommissionProtocol> GetCommissionProtocolById(int protocolId);

        Task<string> SaveDecision(int commissionDecisionId, int decisionId, string comment, DateTime? decisionDateTime, CancellationToken token);

        Task<int> SaveCommissionProtocolAsync(CommissionProtocol newProtocol, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription);

        Task UpdateCommissionProtocolAsync(int protocolId, DateTime protocolDate, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription);

        IEnumerable<CommissionType> GetCommissionTypes(object onDate);

        IEnumerable<CommissionSource> GetCommissionSource(object onDate);

        IDisposableQueryable<Org> GetCommissionSentLPUs(DateTime onDate);

        IDisposableQueryable<PersonTalon> GetPatientTalons(int personId);

        IDisposableQueryable<PersonTalon> GetTalonById(int id);

        IDisposableQueryable<RecordContract> GetRecordContractsByOptions(string options, DateTime onDate);

        IDisposableQueryable<PersonAddress> GetPatientAddresses(int personId);

        IEnumerable<MedicalHelpType> GetCommissionMedicalHelpTypes(object onDate);

        IEnumerable<CommissionQuestion> GetCommissionQuestions(object onDate);

        IDisposableQueryable<Person> GetPerson(int personId);

        Task SaveCommissionProtocol(CommissionProtocol commissionProtocol, CancellationToken token);

        IDisposableQueryable<CommissionProtocol> GetPersonCommissionProtocols(int personId);

        IDisposableQueryable<AddressType> GetAddressTypeByCategory(string category);

        Task<int> SaveTalon(PersonTalon talon, CancellationToken token);

        Task<int> SaveTalonAddress(PersonAddress talonAddress, CancellationToken token);

        Task<bool> RemoveTalon(int talonId);

        Task<string> RemoveCommissionProtocol(int protocolId, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription);

        Task<string> ChangeIsExecutingCommissionProtocol(int protocolId, bool isExecuting, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription);

        IEnumerable<CommissionMemberType> GetCommissionMemberTypes();

        IDisposableQueryable<PersonStaff> GetPersonStaffs(object onDate);

        IEnumerable<Staff> GetStaffs();

        Task SaveCommissionMembersAsync(CommissionMember[] commissionMembers, DateTime commissionOnDate);

        IDisposableQueryable<CommissionMember> CommissionMemberById(int id);
    }
}
