using Core.Data;
using Core.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PatientRecordsModule.ViewModels
{
    public class MKBTreeViewModel : BindableBase, IDisposable, IDialogViewModel
    {
        private readonly IDiagnosService diagnosService;
        private readonly ICacheService cacheService;

        public MKBTreeViewModel(IDiagnosService diagnosService, ICacheService cacheService)
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

            MKBTree = new ObservableCollectionEx<MKBViewModel>();
        }

        private MKB[] mkbRoots;

        internal void Initialize()
        {
            MKBCollection = cacheService.GetItems<MKB>().ToArray();
            mkbRoots = MKBCollection.Where(x => !x.ParentId.HasValue).ToArray();
            LoadMKBTree(mkbRoots, false);            
        }

        private void LoadMKBTree(MKB[] nodes, bool needExpand)
        {
            MKBTree.Clear();
            MKBTree.AddRange(nodes.Where(x => !x.ParentId.HasValue).Select(x => new MKBViewModel(diagnosService, x.MKB1.ToArray(), searchMKB, needExpand)
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    ParentId = x.ParentId
                }));
        }

        #region Properties

        public ICollection<MKB> MKBCollection { get; private set; }

        /// <summary>
        /// Returns a collection of root mkb in the tree, to which the TreeView can bind.
        /// </summary>       
        private ObservableCollectionEx<MKBViewModel> mkbTree;
        public ObservableCollectionEx<MKBViewModel> MKBTree
        {
            get { return mkbTree; }
            set { SetProperty(ref mkbTree, value); }
        }       

        #endregion

        #region Search Logic
                
        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        private string searchMKB = string.Empty;
        public string SearchMKB
        {
            get { return searchMKB; }
            set
            {
                if (SetProperty(ref searchMKB, value))
                {
                    RunSearchingMKB();
                }
            }
        }

        private void RunSearchingMKB()
        {
            if (!string.IsNullOrEmpty(searchMKB))
            {
                bool isLatinLetter = Regex.IsMatch(searchMKB, "^[a-zA-Z0-9]*$");
                if (isLatinLetter || (!isLatinLetter && AppConfiguration.UserInputSearchThreshold <= searchMKB.Length))
                {
                    var matches = this.FindMatches(searchMKB/*, MKBCollection*/);
                    var p1 = matches.Where(x => !x.ParentId.HasValue);
                    var p2 = matches.Where(x => x.ParentId.HasValue).Select(x => x.MKB2);
                    var matchingResult = (p1.Union(p2)).ToArray();
                    LoadMKBTree(matchingResult, true);
                }
            }
            else
                LoadMKBTree(mkbRoots, false);
        }

        private IEnumerable<MKB> FindMatches(string searchText/*, MKB[] roots*/)
        {
            return MKBCollection.Where(x => x.Name.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1 || x.Code.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1);
            /*foreach (var mkb in roots)
            {
                if (mkb.ContainsFindString(searchText))
                    yield return mkb;

                foreach (MKB child in mkb.MKB1)
                    foreach (MKB match in this.FindMatches(searchText, new MKB[] { child }))
                        yield return match;
            }*/
        }

        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Справочник МКБ-10"; }
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
                if (!MKBTree.Any(x => x.IsSelected || x.Children.Any(a => a.IsSelected)))
                {
                    return;
                }
                OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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
