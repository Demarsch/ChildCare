using Core.Data;
using Core.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Shared.PatientRecords.ViewModels
{
    class ComplicationsTreeViewModel : BindableBase, IDisposable, IDialogViewModel
    {
        private readonly IDiagnosService diagnosService;
        private readonly ICacheService cacheService;

        public ComplicationsTreeViewModel(IDiagnosService diagnosService, ICacheService cacheService)
        {
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.diagnosService = diagnosService;
            this.cacheService = cacheService;

            CloseCommand = new DelegateCommand<bool?>(Close);

            ComplicationsTree = new ObservableCollectionEx<ComplicationViewModel>();
            CheckedComplications = new ObservableCollectionEx<ComplicationViewModel>();
        }

        private Complication[] complicationsRoots;

        internal void Initialize()
        {
            ComplicationCollection = cacheService.GetItems<Complication>().ToArray();
            complicationsRoots = ComplicationCollection.Where(x => !x.ParentId.HasValue).ToArray();
            LoadComplicationsTree(complicationsRoots, false);  
        }

        private void LoadComplicationsTree(Complication[] nodes, bool needExpand)
        {
            ComplicationsTree.Clear();
            ComplicationsTree.AddRange(nodes.Where(x => !x.ParentId.HasValue).Select(x => new ComplicationViewModel(diagnosService, x.Complications1.ToArray(), searchComplication, needExpand)
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId
            }));
        }

        #region Properties

        public ICollection<Complication> ComplicationCollection { get; private set; }

        /// <summary>
        /// Returns a collection of root complications in the tree, to which the TreeView can bind.
        /// </summary>       
        private ObservableCollectionEx<ComplicationViewModel> complicationsTree;
        public ObservableCollectionEx<ComplicationViewModel> ComplicationsTree
        {
            get { return complicationsTree; }
            set { SetProperty(ref complicationsTree, value); }
        }

        /// <summary>
        /// Returns a checked collection of complications in the tree
        /// </summary>       
        private ObservableCollectionEx<ComplicationViewModel> checkedComplications;
        public ObservableCollectionEx<ComplicationViewModel> CheckedComplications
        {
            get { return checkedComplications; }
            set { SetProperty(ref checkedComplications, value); }
        }

        #endregion

        #region Search Logic

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        private string searchComplication = string.Empty;
        public string SearchComplication
        {
            get { return searchComplication; }
            set
            {
                if (SetProperty(ref searchComplication, value))
                {
                    RunSearchingComplication();
                }
            }
        }

        private void RunSearchingComplication()
        {
            if (!string.IsNullOrEmpty(searchComplication))
            {
                if (searchComplication.Length >= AppConfiguration.UserInputSearchThreshold)
                {
                    var matches = this.FindMatches(searchComplication/*, ComplicationCollection.ToArray()*/).Distinct();
                    var p1 = matches.Where(x => !x.ParentId.HasValue);
                    var p2 = matches.Where(x => x.ParentId.HasValue).Select(x => x.Complication1);
                    var matchingResult = (p1.Union(p2)).ToArray();
                    LoadComplicationsTree(matchingResult, true);
                }
            }
            else
                LoadComplicationsTree(complicationsRoots, false);
        }

        private IEnumerable<Complication> FindMatches(string searchText/*, Complication[] roots*/)
        {
            return ComplicationCollection.Where(x => x.Name.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1);
            
            /*foreach (var complication in roots)
            {
                if (complication.ContainsFindString(searchText))
                    yield return complication;

                foreach (var child in complication.Complications1)
                    foreach (var match in this.FindMatches(searchText, new Complication[] { child }))
                        yield return match;
            }*/
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Справочник осложнений"; }
        }

        public string ConfirmButtonText
        {
            get { return "Выбрать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                CheckedComplications.Clear();
                foreach (var node in complicationsTree)
                    CheckedComplications.AddRange(GetCheckedItems(node));
                
                if (!CheckedComplications.Any())                
                    return;                
                OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        private List<ComplicationViewModel> GetCheckedItems(ComplicationViewModel node)
        {
            var checkedItems = new List<ComplicationViewModel>();
            ProcessNode(node, checkedItems);
            return checkedItems;
        }

        private void ProcessNode(ComplicationViewModel node, List<ComplicationViewModel> checkedItems)
        {
            foreach (var child in node.Children)
            {
                if (child.IsChecked)
                    checkedItems.Add(child);
                ProcessNode(child, checkedItems);
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        public void Dispose()
        {
        }
    }
}
