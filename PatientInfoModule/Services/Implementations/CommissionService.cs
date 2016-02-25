using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Core.Extensions;
using System.Data.Entity;
using System.Threading;
using Core.Misc;

namespace PatientInfoModule.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IDbContextProvider contextProvider;
        private readonly IUserService userService;
        private readonly ICacheService cacheService;

        public CommissionService(IDbContextProvider contextProvider, IUserService userService, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            this.contextProvider = contextProvider;
            this.userService = userService;
        }
       
        public string GetDecisionColorHex(int? decisionId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                string defColor = HexConverter(System.Drawing.Color.White);
                if (!decisionId.HasValue) return defColor;
                var decision = context.Set<Decision>().FirstOrDefault(x => x.Id == decisionId.Value);
                return (decision != null && decision.ColorsSetting != null && !string.IsNullOrEmpty(decision.ColorsSetting.Hex)) ? decision.ColorsSetting.Hex : defColor;
            }
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        
        public IDisposableQueryable<CommissionProtocol> GetCommissionProtocolById(int protocolId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionProtocol>(context.Set<CommissionProtocol>().Where(x => x.Id == protocolId), context);
        }
               
        public IDisposableQueryable<PersonTalon> GetPatientTalons(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.PersonId == personId), context);
        }

        public IEnumerable<MedicalHelpType> GetCommissionMedicalHelpTypes(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<MedicalHelpType>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }      
       
        public IDisposableQueryable<CommissionProtocol> GetPersonCommissionProtocols(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionProtocol>(context.Set<CommissionProtocol>().Where(x => x.PersonId == personId), context);
        }
        
        public IDisposableQueryable<PersonTalon> GetTalonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.Id == id), context);
        }
              
        public IDisposableQueryable<AddressType> GetAddressTypeByCategory(string category)
        {
            var context = contextProvider.CreateNewContext();
            var filter = category.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var query = context.Set<AddressType>().ToList().Where(x => filter.Any(y => x.Category.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1)).AsQueryable();
            return new DisposableQueryable<AddressType>(query.Take(AppConfiguration.SearchResultTakeTopCount), context);
        }

        public IDisposableQueryable<RecordContract> GetRecordContractsByOptions(string options, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>()
                                                                  .Where(x => x.Options.Contains(options) && x.BeginDateTime <= onDate && x.EndDateTime > onDate)
                                                                  .Take(AppConfiguration.SearchResultTakeTopCount), context);
        }

        public async Task<int> SaveTalon(PersonTalon talon, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);
            using (var context = contextProvider.CreateNewContext())
            {
                var savedTalon = talon.Id == SpecialValues.NewId ? new PersonTalon() : context.Set<PersonTalon>().First(x => x.Id == talon.Id);
                savedTalon.PersonId = talon.PersonId;
                savedTalon.TalonNumber = talon.TalonNumber.ToSafeString();
                savedTalon.TalonDateTime = talon.TalonDateTime;
                savedTalon.MKB = talon.MKB.ToSafeString();
                savedTalon.Comment = talon.Comment.ToSafeString();
                savedTalon.RecordContractId = talon.RecordContractId;
                savedTalon.MedicalHelpTypeId = talon.MedicalHelpTypeId;
                savedTalon.IsCompleted = talon.IsCompleted;
                savedTalon.PersonAddressId = talon.PersonAddressId;
                context.Entry<PersonTalon>(savedTalon).State = savedTalon.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;

                if (token.IsCancellationRequested)
                    throw new OperationCanceledException(token);
                await context.SaveChangesAsync(token);
                return savedTalon.Id;
            }
        }

        public async Task<bool> RemoveTalon(int talonId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var contract = context.Set<PersonTalon>().First(x => x.Id == talonId);
                context.Entry(contract).State = EntityState.Deleted;
                try
                {
                    await context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<int> SaveTalonAddress(PersonAddress talonAddress, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);
            using (var context = contextProvider.CreateNewContext())
            {
                var savedAddress = talonAddress.Id == SpecialValues.NewId ? new PersonAddress() : context.Set<PersonAddress>().First(x => x.Id == talonAddress.Id);
                savedAddress.PersonId = talonAddress.PersonId;
                savedAddress.AddressTypeId = talonAddress.AddressTypeId;
                savedAddress.OkatoId = talonAddress.OkatoId;
                savedAddress.UserText = talonAddress.UserText.ToSafeString();
                savedAddress.House = talonAddress.House.ToSafeString();
                savedAddress.Building = talonAddress.Building.ToSafeString();
                savedAddress.Apartment = talonAddress.Apartment.ToSafeString();
                savedAddress.BeginDateTime = talonAddress.BeginDateTime;
                savedAddress.EndDateTime = talonAddress.EndDateTime;
                context.Entry<PersonAddress>(savedAddress).State = savedAddress.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;

                if (token.IsCancellationRequested)
                    throw new OperationCanceledException(token);
                await context.SaveChangesAsync(token);
                return savedAddress.Id;
            }
        }

    }
}
