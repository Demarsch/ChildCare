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
using System.Collections.Generic;

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

        public IDisposableQueryable<DiagnosType> GetDiagnosTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<DiagnosType>(context.Set<DiagnosType>().AsNoTracking().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<DiagnosType> GetDiagnosTypeByOption(string option)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<DiagnosType>(context.Set<DiagnosType>().AsNoTracking().Where(x => x.Options.Contains(option)), context);
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

        public IDisposableQueryable<Complication> GetRootComplications()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Complication>(context.Set<Complication>().Where(x => !x.ParentId.HasValue), context);
        }

        public IDisposableQueryable<MKB> GetRootMKB()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<MKB>(context.Set<MKB>().Where(x => !x.ParentId.HasValue), context);
        }

        public IDisposableQueryable<MKB> GetMKBChildren(int parentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<MKB>(context.Set<MKB>().Where(x => x.ParentId == parentId), context);
        }

        public IDisposableQueryable<MKB> GetMKBParent(int childId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<MKB>(context.Set<MKB>().Where(x => x.Id == childId).Select(x => x.MKB2), context);
        }

        public IDisposableQueryable<MKB> GetMKBById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<MKB>(context.Set<MKB>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<MKB> GetMKBById(int[] ids)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<MKB>(context.Set<MKB>().Where(x => ids.Contains(x.Id)), context);
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


        public bool Save(int personId, int recordId, int diagnosTypeId, Diagnosis[] diagnoses, out string exception)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                PersonDiagnos personDiagnos = context.Set<PersonDiagnos>().FirstOrDefault(x => x.RecordId == recordId && x.DiagnosTypeId == diagnosTypeId);
                if (personDiagnos == null)
                {
                    personDiagnos = new PersonDiagnos();
                    context.Entry<PersonDiagnos>(personDiagnos).State = EntityState.Added;
                }
                else
                    context.Entry<PersonDiagnos>(personDiagnos).State = EntityState.Modified;
                personDiagnos.PersonId = personId;
                personDiagnos.RecordId = recordId;
                personDiagnos.DiagnosTypeId = diagnosTypeId;


                var old = personDiagnos.Diagnoses.ToDictionary(x => x.Id);
                var @new = diagnoses.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
                var added = diagnoses.Where(x => x.Id == SpecialValues.NewId).ToArray();
                var existed = @new.Where(x => old.ContainsKey(x.Key))
                                  .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                                  .ToArray();
                foreach (var diagnos in added)
                {
                    diagnos.PersonDiagnosId = personDiagnos.Id;
                    context.Entry(diagnos).State = EntityState.Added;
                }                
                foreach (var diagnos in existed.Where(x => x.IsChanged))
                {
                    diagnos.Old.ComplicationId = diagnos.New.ComplicationId;
                    diagnos.Old.DiagnosLevelId = diagnos.New.DiagnosLevelId;
                    diagnos.Old.DiagnosText = diagnos.New.DiagnosText;
                    diagnos.Old.MKB = diagnos.New.MKB;
                    diagnos.Old.IsMainDiagnos = diagnos.New.IsMainDiagnos;
                    diagnos.Old.InDateTime = diagnos.New.InDateTime;
                    diagnos.Old.InPersonId = diagnos.New.InPersonId;
                    context.Entry(diagnos.Old).State = EntityState.Modified;
                }
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

    }
}
