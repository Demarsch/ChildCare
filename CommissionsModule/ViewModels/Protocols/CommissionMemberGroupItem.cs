using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionMemberGroupItem : BindableBase
    {
        private int stage = 0;
        public int Stage
        {
            get { return stage; }
            set
            {
                SetProperty(ref stage, value);
                OnPropertyChanged(() => StageText);
                OnPropertyChanged(() => IsNotFirstItem);
            }
        }

        private bool needAllMembers;
        public bool NeedAllMembers
        {
            get { return needAllMembers; }
            set { SetProperty(ref needAllMembers, value); }
        }

        public bool IsNotFirstItem
        {
            get { return stage != 1; }
        }

        private bool isNotLastItem;
        public bool IsNotLastItem
        {
            get { return isNotLastItem; }
            set { SetProperty(ref isNotLastItem, value); }
        }

        public string StageText
        {
            get { return Stage + "-й этап"; }
        }

        public override bool Equals(object obj)
        {
            var commissionMemberGroupItem = obj as CommissionMemberGroupItem;
            if (commissionMemberGroupItem == null) return false;
            return commissionMemberGroupItem.Stage == this.Stage;
        }

        public override int GetHashCode()
        {
            return this.stage;
        }

        public override string ToString()
        {
            return StageText;
        }
    }
}
