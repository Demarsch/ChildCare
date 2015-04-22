using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using AdminTools.View;
using AdminTools.ViewModel;
using GalaSoft.MvvmLight;
using log4net;
using Core;

namespace AdminTools.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        public RelayCommand UsersEditorCommand { get; private set; }
        private ISimpleLocator service;

        public MainWindowViewModel(ISimpleLocator service)
        {
            this.service = service;
            this.UsersEditorCommand = new RelayCommand(this.UsersEditor);            
        }

        public void UsersEditor()
        {
            (new UserManagerView() { DataContext = new UserManagerViewModel(this.service) }).ShowDialog();
        } 
    }
}
