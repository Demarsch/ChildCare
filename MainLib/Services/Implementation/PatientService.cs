using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataLib;

namespace Core
{
    public class PatientService : IPatientService
    {
        private readonly IDataContextProvider dataContextProvider;

        public PatientService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<PersonRelativeDTO> GetPersonRelatives(int personId)
        {
            var context = dataContextProvider.GetNewDataContext();
            return (context.GetData<PersonRelative>().Where(x => x.PersonId == personId)
                .Select(x => new PersonRelativeDTO()
                {
                     RelativePersonId = x.RelativeId,
                     ShortName = x.Person1.ShortName,
                     RelativeRelationName = x.RelativeRelationship.Name,
                     IsRepresentative = x.IsRepresentative
                }).ToList());
        }

        public EntityContext<Person> GetPersonById(int id)
        {
            var context = dataContextProvider.GetNewDataContext();
            return new EntityContext<Person>(context.GetData<Person>().FirstOrDefault(x => x.Id == id), context);
        }

        public string SavePersonName(int personId, string firstName, string lastName, string middleName, int changeNameReasonId, DateTime fromDateTime)
        {
            string resString = string.Empty;
            var context = dataContextProvider.GetNewDataContext();
            if (!context.GetData<Person>().Any(x => x.Id == personId))
                return "Данный человек не найден!";
            
            var currentPersonName = context.GetData<PersonName>().FirstOrDefault(x => fromDateTime >= x.BeginDateTime && fromDateTime < x.EndDateTime);
            var changeNameReason = context.GetData<ChangeNameReason>().FirstOrDefault(x => x.Id == changeNameReasonId);
            if (currentPersonName != null)
            {
                if (changeNameReason == null)
                    return "Не указана причина смены ФИО";
                if (changeNameReason.NeedCreateNewPersonName)
                {
                    currentPersonName.ChangeNameReason = changeNameReason;
                    currentPersonName.EndDateTime = fromDateTime;
                    var newPersonName = new PersonName()
                    {
                        PersonId = personId,
                        FirstName = firstName,
                        LastName = lastName,
                        MiddleName = middleName,
                        BeginDateTime = fromDateTime,
                        EndDateTime = new DateTime(9000, 1, 1)
                    };
                    context.Add<PersonName>(newPersonName);
                }
                else
                {
                    currentPersonName.FirstName = firstName;
                    currentPersonName.LastName = lastName;
                    currentPersonName.MiddleName = middleName;
                }
            }
            else
            {
               var newPersonName = new PersonName()
               {
                   PersonId = personId,
                   FirstName = firstName,
                   LastName = lastName,
                   MiddleName = middleName,
                   BeginDateTime = DateTime.Now,
                   EndDateTime = new DateTime(9000, 1, 1)
               };
                context.Add<PersonName>(newPersonName);
            }
            context.Save();
            return string.Empty;
        }

        public ICollection<InsuranceDocument> GetPersonInsuranceDocuments(int personId)
        {
            var context = dataContextProvider.GetNewDataContext();
            return context.GetData<InsuranceDocument>().Where(x => x.PersonId == personId).ToArray();
        }

        public ICollection<Person> GetPatients(string searchString, int topCount = 0)
        {
            var parsedUserInput = ParseUserInput(searchString);
            using (var context = dataContextProvider.GetNewDataContext())
            {
                //TODO: do we need to check for old names?
                var result = context.GetData<PersonName>()
                    .Where(x => parsedUserInput.Names.Any(y => x.FirstName.StartsWith(y))
                                                 || parsedUserInput.Names.Any(y => x.LastName.StartsWith(y))
                                                 || parsedUserInput.Names.Any(y => x.MiddleName.StartsWith(y))
                                                 || parsedUserInput.Number == x.Person.Snils
                                                 || parsedUserInput.Number == x.Person.MedNumber
                                                 || parsedUserInput.Date == x.Person.BirthDate);
                //Every match in name, middle name, last name, social number or insurance number gives one point to result
                //Then we sort them by this result descending and show top 5 of them (i.e. those who have more matches will be first on the list)
                result = result.OrderByDescending(x => (parsedUserInput.Names.Any(y => x.FirstName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Names.Any(y => x.LastName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Names.Any(y => x.MiddleName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Number == x.Person.Snils ? 1 : 0)
                                                     + (parsedUserInput.Number == x.Person.MedNumber ? 1 : 0)
                                                     + (parsedUserInput.Date == x.Person.BirthDate ? 1 : 0));
                if (topCount > 0)
                    result = result.Take(topCount);
                return result.Select(x => x.Person)
                             .ToArray();
            }
        }

        private static readonly Regex DateRegex = new Regex(@"\d{1,2}[- /\.]\d{1,2}[- /\.]\d{2,4}");

        private static readonly Regex SnilsRegex = new Regex(@"\d{3} ?\d{3} ?\d{3}-?\d{2}");

        private static readonly Regex MedNumberRegex = new Regex(@"\d{13}");

        internal ParsedUserInput ParseUserInput(string userInput)
        {
            var result = new ParsedUserInput();
            //Extracting dates
            var matches = DateRegex.Matches(userInput);
            for (var index = matches.Count - 1; index >= 0; index--)
            {
                var match = matches[index];
                DateTime date;
                if (index == 0 && DateTime.TryParse(match.Value
                    .Replace("-", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
                    .Replace("/", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
                    .Replace(" ", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator), CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
                    result.Date = date;
                userInput = userInput.Remove(match.Index, match.Length);
            }
            //Extracting mednumber
            matches = MedNumberRegex.Matches(userInput);
            for (var index = matches.Count - 1; index >= 0; index--)
            {
                var match = matches[index];
                if (index == 0)
                    result.Number = match.Value;
                userInput = userInput.Remove(match.Index, match.Length);
            }
            //Extracting SNILS
            matches = SnilsRegex.Matches(userInput);
            for (var index = matches.Count - 1; index >= 0; index--)
            {
                var match = matches[index];
                if (index == 0)
                    result.Number = SnilsCanBeDelimitized(match.Value) ? DelimitizeSnils(match.Value) : match.Value;
                userInput = userInput.Remove(match.Index, match.Length);
            }
            foreach (var word in userInput.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                result.Names.Add(word);
            return result;
        }

        internal bool SnilsCanBeDelimitized(string snils)
        {
            return !string.IsNullOrEmpty(snils) && snils.Length == 11;
        }

        internal string DelimitizeSnils(string undelimitedSnils)
        {
            var result = new StringBuilder(14);
            result.Append(undelimitedSnils.Substring(0, 3))
                .Append(' ')
                .Append(undelimitedSnils.Substring(3, 3))
                .Append(' ')
                .Append(undelimitedSnils.Substring(6, 3))
                .Append('-')
                .Append(undelimitedSnils.Substring(9));
            return result.ToString();
        }

        internal class ParsedUserInput
        {
            public IList<string> Names { get; private set; }

            public string Number { get; set; }

            public DateTime Date { get; set; }

            public ParsedUserInput()
            {
                Names = new List<string>();
            }
        }
    }
}
