using System.Collections.Generic;
using DataLib;
using System;

namespace Core
{
    public interface IRecordService
    {
        RecordType GetRecordTypeById(int id);
        ICollection<RecordType> GetRecordTypesByOptions(string[] options);
        ICollection<RecordType> GetRecordTypesByOptions(string options);
        ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string[] options);
        ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string options);
        ICollection<RecordType> GetAllRecordTypes();
        ICollection<RecordType> GetRecordTypesByName(string name);
        double GetRecordTypeCost(int recordTypeId);

        ICollection<FinancingSource> GetActiveFinancingSources();
        ICollection<PaymentType> GetPaymentTypes();
        PaymentType GetPaymentTypeById(int id);
        ICollection<Visit> GetVisitsByContractId(int contractId);

        ICollection<Core.PersonVisitItemsListViewModels.RecordDTO> GetChildRecords(int recordId);
        ICollection<AssignmentDTO> GetChildAssignments(int recordId);
    }
}
