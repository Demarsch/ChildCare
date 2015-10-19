using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;
using System.Data.Entity.Core.Objects;

namespace Core
{
    public class RecordService : IRecordService
    {
        private readonly IDataContextProvider provider;

        public RecordService(IDataContextProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            this.provider = provider;
        }

        public RecordType GetRecordTypeById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<RecordType>(id);
            }
        }

        public ICollection<RecordType> GetRecordTypesByOptions(string[] options)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordType>().Where(x => x.Options.ContainsAny(options)).ToArray();
            }
        }

        public ICollection<RecordType> GetRecordTypesByOptions(string options)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordType>().Where(x => x.Options.Contains(options)).ToArray();
            }
        }

        public ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string[] options)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordTypeRole>().Where(x => x.Options.ContainsAny(options)).ToArray();
            }
        }

        public ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string options)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordTypeRole>().Where(x => x.Options.Contains(options)).ToArray();
            }
        }

        public ICollection<RecordType> GetAllRecordTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordType>().ToArray();
            }
        }

        public ICollection<RecordType> GetRecordTypesByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RecordType>().Where(x => x.Name.ToLower().Trim().Contains(name.ToLower().Trim())).ToArray();
            }
        }

        public ICollection<FinancingSource> GetActiveFinancingSources()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<FinancingSource>().Where(x => x.IsActive).ToArray();
            }
        }

        public ICollection<PaymentType> GetPaymentTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PaymentType>().ToArray();
            }
        }

        public PaymentType GetPaymentTypeById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<PaymentType>(id);
            }
        }

        public ICollection<Visit> GetVisitsByContractId(int contractId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Visit>().Where(x => x.ContractId == contractId).ToArray();
            }
        }

        public double GetRecordTypeCost(int recordTypeId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return 1000;
            }
        }

        public ICollection<PersonVisitItemsListViewModels.RecordDTO> GetChildRecords(int recordId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Record>().Where(x => x.ParentId == recordId).Select(x => new Core.PersonVisitItemsListViewModels.RecordDTO()
                {
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                    IsCompleted = x.IsCompleted
                }).ToArray();
            }
        }

        public ICollection<AssignmentDTO> GetChildAssignments(int recordId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Assignment>().Where(x => x.RecordId == recordId).Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    AssignDateTime = x.AssignDateTime,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name
                }).ToList();
            }
        }
    }
}
