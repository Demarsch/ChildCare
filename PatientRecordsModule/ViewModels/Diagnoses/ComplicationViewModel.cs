using Core.Data;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels
{
    public class ComplicationViewModel : BindableBase
    {   
        private IDiagnosService diagnosService;

        public ComplicationViewModel(IDiagnosService diagnosService, Complication[] childs, string searchComplication, bool needExpand)
        {
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            this.diagnosService = diagnosService;

            if (!string.IsNullOrEmpty(searchComplication))
                childs = childs.Where(x => x.Name.IndexOf(searchComplication, StringComparison.InvariantCultureIgnoreCase) > -1).ToArray();
            Children = new ObservableCollectionEx<ComplicationViewModel>
                        (childs.Select(x => new ComplicationViewModel(diagnosService, x.Complications1.ToArray(), searchComplication, needExpand)
                           {
                               Id = x.Id,
                               Name = x.Name,
                               ParentId = x.ParentId
                           }));
            IsExpanded = needExpand;
        }

        #region Complication Properties

        private ObservableCollectionEx<ComplicationViewModel> children;
        public ObservableCollectionEx<ComplicationViewModel> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
      
        private int? parentId;
        public int? ParentId
        {
            get { return parentId; }
            set { SetProperty(ref parentId, value); }
        }

        #endregion

        #region TreeProperties

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        } 
       
        #endregion
    
    }
}
