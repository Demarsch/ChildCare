using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.DTO
{
    public class BrigadeDTO
    {
        public bool IsRequired { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public int RecordTypeRolePermissionId { get; set; }
        public string PersonName { get; set; }
        public string StaffName { get; set; }
        public int PersonStaffId { get; set; }
        public int RecordTypeId { get; set; }
        public DateTime OnDate { get; set; }
    }
}
