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

        public IDisposableQueryable<DiagnosLevel> GetDiagnosLevelById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<DiagnosLevel>(context.Set<DiagnosLevel>().AsNoTracking().Where(x => x.Id == id), context);
        }        

        public IDisposableQueryable<Diagnosis> GetDiagnosById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Diagnosis>(context.Set<Diagnosis>().AsNoTracking().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<Diagnosis> GetRecordDiagnos(int recordId, int? diagnosTypeId = null)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Diagnosis>(
                context.Set<Diagnosis>().Where(a => a.PersonDiagnos == 
                    context.Set<PersonDiagnos>().Where(x => x.RecordId == recordId && (!diagnosTypeId.HasValue || x.DiagnosTypeId == diagnosTypeId))
                                            .OrderByDescending(x => x.Record.ActualDateTime).FirstOrDefault()), context);
        }

        public bool DeleteDiagnos(int diagnosId, out string exception)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var diagnos = context.Set<Diagnosis>().FirstOrDefault(x => x.Id == diagnosId);
                int personDiagnosId = diagnos.PersonDiagnosId;
                if (diagnos != null)
                {
                    context.Entry(diagnos).State = EntityState.Deleted;
                    try
                    {
                        context.SaveChanges();
                        var personDiagnos = context.Set<PersonDiagnos>().First(x => x.Id == personDiagnosId);
                        if (!personDiagnos.Diagnoses.Any())
                        {
                            if (!DeletePersonDiagnos(personDiagnosId, out exception))
                                return false;
                        }
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
        }

        public bool DeletePersonDiagnos(int personDiagnosId, out string exception)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var personDiagnos = context.Set<PersonDiagnos>().FirstOrDefault(x => x.Id == personDiagnosId);
                if (personDiagnos != null)
                {
                    context.Entry(personDiagnos).State = EntityState.Deleted;
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
        }
    }
}
