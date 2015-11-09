using Core.Data;
using Core.Data.Misc;
using System;

namespace PatientInfoModule.Services
{
    public interface IContractService
    {
        IDisposableQueryable<RecordContract> GetContracts(int? consumerId = null, DateTime? fromDate = null, DateTime? toDate = null, int inUserId = -1);

        IDisposableQueryable<RecordContractItem> GetContractItems(int contractId, int? appendix = null);

        double GetContractCost(int[] contractIds);

        double GetContractCost(int contractId);

        IDisposableQueryable<RecordContract> GetContractById(int id);

        IDisposableQueryable<RecordContractItem> GetContractItemById(int id);

        int SaveContractData(RecordContract contract);

        void DeleteContract(int contractId);
              
        void DeleteContractItemById(int id);  
    }
}
