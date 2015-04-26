using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Editors;

namespace Core
{
    public class UsersSystemInfoSuggestionProvider : ISuggestionProvider
    {
        private IUserSystemInfoService service;

        public UsersSystemInfoSuggestionProvider(IUserSystemInfoService service)
        {
            this.service = service;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;            

            //return service.Find(filter);
            
            var data = new List<UserSystemInfo>();
            data.Add(new UserSystemInfo() { UserName = "Жариков Игорь Александрович", PrincipalName = "izharikov@lpu", Enabled = true, SID = "izharikov" });
            data.Add(new UserSystemInfo() { UserName = "Жариков Петр Петрович", PrincipalName = "pzharikov@lpu", Enabled = false, SID = "pzharikov" });
            
            return data;
        }
    }
}
