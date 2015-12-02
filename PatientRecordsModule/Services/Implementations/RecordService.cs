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

namespace PatientRecordsModule.Services
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

        public IDisposableQueryable<Assignment> GetAssignmentById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().Where(x => x.Id == id), context);
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

        public void UpdateMKBRecord(int recordId, string mkb)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var record = context.Set<Record>().FirstOrDefault(x => x.Id == recordId);
                if (record == null) return;
                record.MKB = mkb;
                record.Visit.MKB = mkb;
                context.SaveChanges();
            }
        }

        public bool SaveRecordDocument(RecordDocument recordDocument, out string exception)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var saveDocument = recordDocument.Id == SpecialValues.NewId ? new RecordDocument() : context.Set<RecordDocument>().First(x => x.Id == recordDocument.Id);
                saveDocument.AssignmentId = recordDocument.AssignmentId;
                saveDocument.RecordId = recordDocument.RecordId;
                saveDocument.DocumentId = recordDocument.DocumentId;
                context.Entry(saveDocument).State = saveDocument.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                try
                {
                    context.SaveChanges();
                    exception = string.Empty;
                    return true;
                }
                catch (Exception ex)
                {
                    exception = ex.Message;
                    return false;
                }
            }
        }

        public bool DeleteRecordDocument(int documentId, out string exception)
        {            
            var context = contextProvider.CreateNewContext();
            var recordDocument = context.Set<RecordDocument>().FirstOrDefault(x => x.DocumentId == documentId);
            if (recordDocument != null)
            {
                context.Entry(recordDocument).State = EntityState.Deleted;
                try
                {
                    context.SaveChanges();
                    exception = string.Empty;
                    return true;
                }
                catch (Exception ex)
                {
                    exception = ex.Message;
                    return false;
                }
            }
            exception = "Ошибка удаления.";
            return false;
        }


        public int SaveDefaultProtocol(DefaultProtocol defaultProtocol)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var saveProtocol = defaultProtocol.Id == SpecialValues.NewId ? new DefaultProtocol() : context.Set<DefaultProtocol>().First(x => x.Id == defaultProtocol.Id);
                saveProtocol.RecordId = defaultProtocol.RecordId;
                saveProtocol.Description = defaultProtocol.Description;
                saveProtocol.Conclusion = defaultProtocol.Conclusion;
                context.Entry(saveProtocol).State = saveProtocol.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                context.SaveChanges();
                return saveProtocol.Id;
            }
        }
    }
}
