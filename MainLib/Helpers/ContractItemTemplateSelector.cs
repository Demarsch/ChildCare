using MainLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Core.Helpers
{
    public class ContractItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (item is ContractItemDetailsViewModel)
            {
                if ((item as ContractItemDetailsViewModel).IsSection)
                    return element.FindResource("SectionDataTemplate") as DataTemplate;
                else
                    return element.FindResource("ContractItemDataTemplate") as DataTemplate;
            }
            return null;
        }
    }
}
