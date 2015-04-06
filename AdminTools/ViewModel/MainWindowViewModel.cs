using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.Command;
using AdminTools.View;
using AdminTools.ViewModel;

namespace AdminTools.ViewModel
{
    class MainWindowViewModel : FailableViewModel
    {
        public RelayCommand UsersEditorCommand { get; private set; }

        public MainWindowViewModel()
        {
            this.UsersEditorCommand = new RelayCommand(this.UsersEditor);
        }

        public void UsersEditor()
        {
            (new UserManagerView() { DataContext = new UserManagerViewModel() }).ShowDialog();
        } 
    }
}
