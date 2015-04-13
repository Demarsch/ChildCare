using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public class UserSystemInfo
    {
        /// <summary>
        /// ФИО
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// логин+домен
        /// </summary>
        public string PrincipalName { get; set; }

        /// <summary>
        /// SID
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// ФИО + логин + домен
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// вкл/выкл
        /// </summary>
        public bool Enabled { get; set; }
    }
}
