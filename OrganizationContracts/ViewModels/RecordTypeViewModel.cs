using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using OrganizationContractsModule.Services;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;

namespace OrganizationContractsModule.ViewModels
{
    public class RecordTypeViewModel : TrackableBindableBase, IDisposable
    {
        public IChangeTracker ChangeTracker { get; private set; }

        public RecordTypeViewModel()
        {
            ChangeTracker = new ChangeTrackerEx<RecordTypeViewModel>(this);
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

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        public void Dispose()
        {
            ChangeTracker.Dispose();
        }
    }
}
