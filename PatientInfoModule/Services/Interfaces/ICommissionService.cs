using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PatientInfoModule.Services
{
    public interface ICommissionService
    {        
        string GetDecisionColorHex(int? decisionId);
      
        IDisposableQueryable<CommissionProtocol> GetCommissionProtocolById(int protocolId);    

        IDisposableQueryable<PersonTalon> GetPatientTalons(int personId);

        IDisposableQueryable<PersonTalon> GetTalonById(int id);

        IEnumerable<MedicalHelpType> GetCommissionMedicalHelpTypes(object onDate);
               
        IDisposableQueryable<CommissionProtocol> GetPersonCommissionProtocols(int personId);

        IDisposableQueryable<AddressType> GetAddressTypeByCategory(string category);

        IDisposableQueryable<RecordContract> GetRecordContractsByOptions(string options, DateTime onDate);

        Task<int> SaveTalon(PersonTalon talon, CancellationToken token);

        Task<int> SaveTalonAddress(PersonAddress talonAddress, CancellationToken token);

        Task<bool> RemoveTalon(int talonId);
    }
}
