﻿using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace OrganizationContractsModule.Services
{
    public interface IContractService
    {
        IDisposableQueryable<RecordContract> GetContractById(int id);
        int SaveContractData(RecordContract contract, int[] limitedRecordTypes);
        void DeleteContract(int contractId);
        void DeleteContractItemById(int id); 
        IDisposableQueryable<Org> GetOrganizations();
        IDisposableQueryable<RecordContract> GetContractsWithOrgs(DateTime begin, DateTime end, int finSourceId = -1);
        IDisposableQueryable<PaymentType> GetPaymentTypes();
        IDisposableQueryable<Org> GetOrganizationById(int id);
        IDisposableQueryable<FinancingSource> GetActiveFinancingSources();
        IDisposableQueryable<RecordType> GetRecordTypesByOptions(string options);
        IDisposableQueryable<RecordTypeRole> GetRecordTypeRolesByOptions(string options);
        IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId, DateTime onDate);
        IDisposableQueryable<Visit> GetVisitsByContractId(int contractId);
        Task<int> SaveOrgAsync(Org org);
        IDisposableQueryable<FinancingSource> GetFinancingSourceById(int id);
        IEnumerable GetPersonsByFullName(string filter);
        IDisposableQueryable<Person> GetPersonById(int id);

        void RemoveRecord(int contractId, int recordTypeId);
    }
}
