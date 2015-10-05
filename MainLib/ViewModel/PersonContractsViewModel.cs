using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using MainLib.View;

namespace MainLib.ViewModel
{
    public class PersonContractsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IPersonService personService;
        private int personId;

        public PersonContractsViewModel(IPersonService personService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.log = log;

            this.OpenFileCommand = new RelayCommand(OpenFile);            
        }

        public async void Load(int personId)
        {
            this.personId = personId;
            await LoadDocuments();                        
        }

        private async Task LoadDocuments()
        {   

        }        

        private void OpenFile()
        {
            
        }
                
        private RelayCommand openFileCommand;
        public RelayCommand OpenFileCommand
        {
            get { return openFileCommand; }
            set { Set("OpenFileCommand", ref openFileCommand, value); }
        }             

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { Set("PatientFIO", ref patientFIO, value); }
        }

        private bool allowDocumentsAction;
        public bool AllowDocumentsAction
        {
            get { return allowDocumentsAction; }
            set { Set("AllowDocumentsAction", ref allowDocumentsAction, value); }
        }
    }
}
