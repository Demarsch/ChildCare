using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

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
    }
}
