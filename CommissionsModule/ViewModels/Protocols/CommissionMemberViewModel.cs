using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionMemberViewModel : BindableBase
    {
        public int Id { get; set; }
        public string StaffName { get; set; }
        public string PersonName { get; set; }
        public string MemberTypeName { get; set; }

        public bool showBtnsPanel = true;
        public bool ShowBtnsPanel
        {
            get { return showBtnsPanel; }
            set { SetProperty(ref showBtnsPanel, value); }
        }
    }
}
