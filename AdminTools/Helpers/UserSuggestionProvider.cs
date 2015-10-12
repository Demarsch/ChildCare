using Core;
using System.Linq;
using WpfControls.Editors;

namespace AdminTools
{
    public class UserSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public UserSuggestionProvider(IPersonService service)
        {
            this.service = service;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;            

            return service.GetPersonsByFullName(filter)
                            .Select(x => new AdminTools.DTO.UserDTO()
                            { 
                                Id = x.Id,
                                FullName = x.FullName,
                                BirthDate = x.BirthDate,
                                Snils = x.Snils,
                                EditImage = "pack://application:,,,/Resources;component/Images/Account_16x16.png",
                                SynchImage = "pack://application:,,,/Resources;component/Images/DB_refresh_24x24.png",
                                SID = string.Empty,
                                Enabled = (bool?)null,
                                PrincipalName = string.Empty
                            }).ToArray();            
        }
    }
}
