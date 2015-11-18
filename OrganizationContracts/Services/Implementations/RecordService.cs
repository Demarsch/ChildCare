using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using Core.Wpf.Services;

namespace OrganizationContractsModule.Services
{
    public class RecordService : IRecordService
    {
        private readonly IDbContextProvider contextProvider;

        public RecordService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Record> GetRecordById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordType> GetRecordTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordType>(context.Set<RecordType>().Where(x => x.Id == id), context);
        }

        public bool SaveRecordDocument(RecordDocument recordDocument)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var saveDocument = recordDocument.Id == SpecialValues.NewId ? new RecordDocument() : db.Set<RecordDocument>().First(x => x.Id == recordDocument.Id);
                saveDocument.RecordId = recordDocument.RecordId;
                saveDocument.DocumentId = recordDocument.DocumentId;
                db.Entry(saveDocument).State = saveDocument.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public void DeleteRecordDocument(int documentId)
        {            
            var context = contextProvider.CreateNewContext();
            var recordDocument = context.Set<RecordDocument>().FirstOrDefault(x => x.DocumentId == documentId);
            if (recordDocument != null)
            {
                context.Entry(recordDocument).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }
    }
}
