using Core.Wpf.Mvvm;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.DTO
{
    public class ScheduleStatisticsRow : BindableBase
    {
        private readonly ObservableCollectionEx<ScheduleStatisticsCell> properties = new ObservableCollectionEx<ScheduleStatisticsCell>();

        public ScheduleStatisticsRow(params ScheduleStatisticsCell[] properties)
        {
            foreach (var property in properties)
                Properties.Add(property);
        }

        public ObservableCollectionEx<ScheduleStatisticsCell> Properties
        {
            get { return properties; }
        }
    }
}
