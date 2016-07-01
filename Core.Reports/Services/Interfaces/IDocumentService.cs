using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;

namespace Core.Reports.Services
{
    public interface IDocumentService
    {
        IDisposableQueryable<ReportTemplate> GetPrintedDocuments(string documentOption);

        IDisposableQueryable<PrintedDocument> GetPrintedDocumentById(int printedDocumentId);

        IDisposableQueryable<Person> GetPersonById(int id);

        string GetDBSettingValue(string parameter, bool useDisplayName = false);

        IDisposableQueryable<RecordContract> GetContractById(int id);
    }
}
