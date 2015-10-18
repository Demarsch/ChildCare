using System;
using System.Collections.Generic;
using Core.Data;
using Core.Data.Misc;

namespace PatientSearchModule.Misc
{
    public class PatientSearchQuery : IDisposable
    {
        public static readonly PatientSearchQuery Empty = new PatientSearchQuery(null, DisposableQueryable<Person>.Empty);

        public IEnumerable<string> ParsedTokens { get; private set; }

        public IDisposableQueryable<Person> PatientsQuery { get; private set; }

        public PatientSearchQuery(IEnumerable<string> parsedTokens, IDisposableQueryable<Person> patientsQuery)
        {
            ParsedTokens = parsedTokens ?? new string[0];
            PatientsQuery = patientsQuery;

        }

        public void Dispose()
        {
            if (PatientsQuery != null)
            {
                PatientsQuery.Dispose();
            }
        }
    }
}
