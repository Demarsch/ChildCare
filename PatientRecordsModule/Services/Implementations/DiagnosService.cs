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
    public class DiagnosService : IDiagnosService
    {
        private readonly IDbContextProvider contextProvider;

        public DiagnosService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<DiagnosType> GetActualDiagnosTypes()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<DiagnosType>(context.Set<DiagnosType>().Where(x => x.IsActual).OrderBy(x => x.Priority), context);
        }

        public IDisposableQueryable<DiagnosLevel> GetActualDiagnosLevels()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<DiagnosLevel>(context.Set<DiagnosLevel>().Where(x => x.IsActual).OrderBy(x => x.Priority), context);
        }

        public IDisposableQueryable<Diagnosis> GetRecordDiagnos(int recordId, int? diagnosTypeId = null)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Diagnosis>(
                context.Set<Diagnosis>().Where(a => a.PersonDiagnos == 
                    context.Set<PersonDiagnos>().Where(x => x.RecordId == recordId && (!diagnosTypeId.HasValue || x.DiagnosTypeId == diagnosTypeId))
                                            .OrderByDescending(x => x.Record.ActualDateTime).FirstOrDefault()), context);
        }
    }
}
