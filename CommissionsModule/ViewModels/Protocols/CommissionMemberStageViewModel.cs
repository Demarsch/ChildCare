using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels.Protocols
{
    public class CommissionMemberStageViewModel : BindableBase
    {
        public int CommissionMemberId { get; set; }

        private int stage = 0;
        public int Stage
        {
            get { return stage; }
            set
            {
                SetProperty(ref stage, value);
                OnPropertyChanged(() => StageText);
            }
        }

        public string StageText
        {
            get { return Stage + "-й этап"; }
        }

        public override string ToString()
        {
            return StageText;
        }
    }
}
