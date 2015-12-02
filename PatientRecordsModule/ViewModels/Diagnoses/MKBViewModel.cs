using Core.Data;
using Core.Wpf.Mvvm;
using PatientRecordsModule.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.ViewModels
{
    public class MKBViewModel : BindableBase
    {   
        private IDiagnosService diagnosService;

        public MKBViewModel(IDiagnosService diagnosService, MKB[] childs, string searchMKB, bool needExpand)
        {
            if (!childs.Any()) return;
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            this.diagnosService = diagnosService;

            if (!string.IsNullOrEmpty(searchMKB))
                childs = childs.Where(x => x.Name.IndexOf(searchMKB, StringComparison.InvariantCultureIgnoreCase) > -1 ||
                                           x.Code.IndexOf(searchMKB, StringComparison.InvariantCultureIgnoreCase) > -1).ToArray();
            
            Children = new ObservableCollectionEx<MKBViewModel>
                        (childs.Select(x => new MKBViewModel(diagnosService, new MKB[0], searchMKB, needExpand)
                           {
                               Id = x.Id,
                               Name = x.Name,
                               Code = x.Code,
                               ParentId = x.ParentId
                           }));
            IsExpanded = needExpand;
        }

        #region MKB Properties

        private ObservableCollectionEx<MKBViewModel> children;
        public ObservableCollectionEx<MKBViewModel> Children
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

        private string code;
        public string Code
        {
            get { return code; }
            set { SetProperty(ref code, value); }
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


        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        } 
       
        #endregion
    }
}
