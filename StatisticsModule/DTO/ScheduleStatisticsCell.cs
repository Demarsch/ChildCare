using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.DTO
{
    public class ScheduleStatisticsCell : BindableBase
    {
        public ScheduleStatisticsCell()
        {
        }

        private string columnName;
        public string ColumnName
        {
            get { return columnName; }
            set { SetProperty(ref columnName, value); }
        }

        private string cellValue;
        public string CellValue
        {
            get { return cellValue; }
            set { SetProperty(ref cellValue, value); }
        }

        private object details;
        public object Details
        {
            get { return details; }
            set { SetProperty(ref details, value); }
        }
    }
}
