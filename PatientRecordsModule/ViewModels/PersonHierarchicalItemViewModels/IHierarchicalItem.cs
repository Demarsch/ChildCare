using Core.Wpf.Mvvm;
using Shared.PatientRecords.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels
{
    public interface IHierarchicalItem
    {
        DateTime ActualDateTime { get; }

        bool IsSelected { get; set; }

        bool IsExpanded { get; set; }

        PersonRecItem Item { get; }

        ObservableCollectionEx<IHierarchicalItem> Childs { get; }
    }
}
