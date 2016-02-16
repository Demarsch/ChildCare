using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.ViewModels.Common
{
    public class PersonTalonViewModel: BindableBase
    {
        public PersonTalonViewModel()
        {
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private bool? isCompleted;
        public bool? IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        private string talonNumber;
        public string TalonNumber
        {
            get { return talonNumber; }
            set { SetProperty(ref talonNumber, value); }
        }

        private string hospitalisationNumber;
        public string HospitalisationNumber
        {
            get { return hospitalisationNumber; }
            set { SetProperty(ref hospitalisationNumber, value); }
        }

        private string talonColorHex;
        public string TalonColorHex
        {
            get { return talonColorHex; }
            set { SetProperty(ref talonColorHex, value); }
        }

        private string talonDate;
        public string TalonDate
        {
            get { return talonDate; }
            set { SetProperty(ref talonDate, value); }
        }

        private string talonState;
        public string TalonState
        {
            get { return talonState; }
            set { SetProperty(ref talonState, value); }
        }
       
        private string medHelpType;
        public string MedHelpType
        {
            get { return medHelpType; }
            set { SetProperty(ref medHelpType, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetProperty(ref mkb, value); }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }
    }
}
