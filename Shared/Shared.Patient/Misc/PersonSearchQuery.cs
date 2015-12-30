using System;
using System.Collections.Generic;
using Core.Data;
using Core.Data.Misc;

namespace Shared.Patient.Misc
{
    public class PersonSearchQuery : IDisposable
    {
        public static readonly PersonSearchQuery Empty = new PersonSearchQuery(null, DisposableQueryable<Person>.Empty);

        public IEnumerable<string> ParsedTokens { get; private set; }

        public IDisposableQueryable<Person> PersonsQuery { get; private set; }

        public PersonSearchQuery(IEnumerable<string> parsedTokens, IDisposableQueryable<Person> personsQuery)
        {
            ParsedTokens = parsedTokens ?? new string[0];
            PersonsQuery = personsQuery;

        }

        public void Dispose()
        {
            if (PersonsQuery != null)
            {
                PersonsQuery.Dispose();
            }
        }
    }
}
