using Core.Data;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using Prism.Commands;
using Shared.Patient.ViewModels;
using Shared.PatientRecords.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommissionsModule.ViewModels
{
    public class PreliminaryProtocolViewModel : TrackableBindableBase
    {
        #region Fields
        private readonly IDialogServiceAsync dialogService;

        private readonly MKBTreeViewModel mkbTreeViewModel;
        #endregion

        #region Constructors
        public PreliminaryProtocolViewModel(IDialogServiceAsync dialogService, MKBTreeViewModel mkbTreeViewModel, AddressViewModel addressViewModel)
        {
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (mkbTreeViewModel == null)
            {
                throw new ArgumentNullException("mkbTreeViewModel");
            }
            this.dialogService = dialogService;
            this.mkbTreeViewModel = mkbTreeViewModel;

            AddressViewModel = addressViewModel;
        }
        #endregion

        #region Properties

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set { SetTrackedProperty(ref selectedCommissionTypeId, value); }
        }

        private int selectedCommissionQuestionId;
        public int SelectedCommissionQuestionId
        {
            get { return selectedCommissionQuestionId; }
            set { SetTrackedProperty(ref selectedCommissionQuestionId, value); }
        }

        private int selectedCommissionSourceId;
        public int SelectedCommissionSourceId
        {
            get { return selectedCommissionSourceId; }
            set { SetTrackedProperty(ref selectedCommissionSourceId, value); }
        }

        private int selectedSentLPUId;
        public int SelectedSentLPUId
        {
            get { return selectedSentLPUId; }
            set { SetTrackedProperty(ref selectedSentLPUId, value); }
        }

        private int selectedTalonId;
        public int SelectedTalonId
        {
            get { return selectedTalonId; }
            set { SetTrackedProperty(ref selectedTalonId, value); }
        }

        private int selectedHelpTypeId;
        public int SelectedHelpTypeId
        {
            get { return selectedHelpTypeId; }
            set { SetTrackedProperty(ref selectedHelpTypeId, value); }
        }

        private DateTime incomeDateTime;
        public DateTime IncomeDateTime
        {
            get { return incomeDateTime; }
            set { SetTrackedProperty(ref incomeDateTime, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetTrackedProperty(ref mkb, value); }
        }

        public AddressViewModel AddressViewModel { get; private set; }

        //DataSources
        public IEnumerable<CommissionType> CommissionTypes { get; private set; }

        public IEnumerable<CommissionQuestion> CommissionQuestions { get; private set; }
        public IEnumerable<CommissionSource> CommissionSources { get; private set; }
        public IEnumerable<Org> SentLPUs { get; private set; }
        public IEnumerable<PersonTalon> Talons { get; private set; }
        public IEnumerable<MedicalHelpType> HelpTypes { get; private set; }

        #endregion

        #region Commands
        private DelegateCommand selectMKBCommand;
        public ICommand SelectMKBCommand { get { return selectMKBCommand; } }
        #endregion

        #region Methods
        public async void SelectMKB()
        {
            var result = await dialogService.ShowDialogAsync(mkbTreeViewModel);
            if (result == true)
            {
                var selectedMKB = mkbTreeViewModel.MKBTree.Any(x => x.IsSelected) ? mkbTreeViewModel.MKBTree.First(x => x.IsSelected) : mkbTreeViewModel.MKBTree.SelectMany(x => x.Children).First(x => x.IsSelected);
                MKB = selectedMKB.Code;
            }
        }
        #endregion
    }
}
