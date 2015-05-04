using System;
using GalaSoft.MvvmLight;

namespace AdminTools
{
    public class UserPermissionsDTO : ObservableObject
    {
        public int Id { get; set; }

        private string permissionName;
        public string PermissionName
        {
            get { return permissionName; }
            set { Set("PermissionName", ref permissionName, value); }
        }

        private DateTime beginDate;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set { Set("BeginDate", ref beginDate, value); }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set { Set("EndDate", ref endDate, value); }
        }

        private bool isGranted;
        public bool IsGranted
        {
            get { return isGranted; }
            set { Set("IsGranted", ref isGranted, value); }
        }        
    }
}
