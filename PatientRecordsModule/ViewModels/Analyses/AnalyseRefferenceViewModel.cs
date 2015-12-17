using Core.Data;
using Core.Data.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseRefferenceViewModel : BindableBase
    {
        public AnalyseRefferenceViewModel()
        {
            Genders = new ObservableCollectionEx<FieldValue>();
            Genders.Add(new FieldValue() { Value = 1, Field = "Мужчины" });
            Genders.Add(new FieldValue() { Value = 0, Field = "Женщины" });
        }       

        #region Properties

        public ObservableCollectionEx<FieldValue> Genders { get; private set; }

        private int selectedGenderId;
        public int SelectedGenderId
        {
            get { return selectedGenderId; }
            set { SetProperty(ref selectedGenderId, value); }
        }
      
        private int ageFrom;
        public int AgeFrom
        {
            get { return ageFrom; }
            set { SetProperty(ref ageFrom, value); }
        }

        private int? ageTo;
        public int? AgeTo
        {
            get { return ageTo; }
            set { SetProperty(ref ageTo, value); }
        }

        private double refMin;
        public double RefMin
        {
            get { return refMin; }
            set { SetProperty(ref refMin, value); }
        }

        private double refMax;
        public double RefMax
        {
            get { return refMax; }
            set { SetProperty(ref refMax, value); }
        }

        #endregion       
    }
}
