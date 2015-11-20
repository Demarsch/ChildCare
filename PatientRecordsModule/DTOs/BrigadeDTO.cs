using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.DTO
{
    public class BrigadeDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public int PermissionId { get; set; }

        public bool IsRequired { get; set; }

        public int PersonStaffId { get; set; }
        public string StaffName { get; set; }
        public string PersonName { get; set; }
    }
}
