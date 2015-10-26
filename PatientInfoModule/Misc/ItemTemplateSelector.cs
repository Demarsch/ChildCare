using PatientInfoModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PatientInfoModule.Misc
{
    public class ItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (item is ContractItemViewModel)
            {
                if ((item as ContractItemViewModel).IsSection)
                    return element.FindResource("SectionDataTemplate") as DataTemplate;
                else
                    return element.FindResource("ContractItemDataTemplate") as DataTemplate;
            }
            return null;
        }
    }
}
