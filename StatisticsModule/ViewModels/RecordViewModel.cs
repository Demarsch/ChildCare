using Core.Wpf.Mvvm;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace StatisticsModule.DTO
{
    public class RecordViewModel : BindableBase
    {
        public RecordViewModel(RecordDTO[] childs, bool needExpand)
        {
            if (!childs.Any()) return;

            /*Children = new ObservableCollectionEx<RecordViewModel>
                        (childs.Select(x => new RecordViewModel(new RecordDTO[0], needExpand)
                           {
                               Id = x.Id,
                               RecordTypeId = x.RecordTypeId,
                               RecordName = x.RecordName,
                               FinSource = x.FinSource,
                               BeginDate = x.BeginDate.ToFullString(),
                               EndDate = x.EndDate.ToFullString(),
                               Count = childs.Count()
                           }));*/
            IsExpanded = needExpand;
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { SetProperty(ref recordTypeId, value); }
        }

        private string recordName;
        public string RecordName
        {
            get { return recordName; }
            set { SetProperty(ref recordName, value); }
        }

        private string finSource;
        public string FinSource
        {
            get { return finSource; }
            set { SetProperty(ref finSource, value); }
        }

        private string beginDate;
        public string BeginDate
        {
            get { return beginDate; }
            set { SetProperty(ref beginDate, value); }
        }

        private string endDate;
        public string EndDate
        {
            get { return endDate; }
            set { SetProperty(ref endDate, value); }
        }

        private int count;
        public int Count
        {
            get { return count; }
            set { SetProperty(ref count, value); }
        }

        private ObservableCollectionEx<RecordViewModel> children;
        public ObservableCollectionEx<RecordViewModel> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        #region TreeProperties

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }


        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        #endregion
    }
}
