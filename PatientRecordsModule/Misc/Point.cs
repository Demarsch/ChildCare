using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shared.PatientRecords.Misc
{
    public class Point : DependencyObject
    {
        public Point(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }

        public static readonly DependencyProperty _date = DependencyProperty.Register("Date", typeof(DateTime), typeof(Point));        

        public DateTime Date
        {
            get { return (DateTime)GetValue(_date); }
            set { SetValue(_date, value); }
        }

        public static readonly DependencyProperty _value = DependencyProperty.Register("Value", typeof(double), typeof(Point));

        public double Value
        {
            get { return (double)GetValue(_value); }
            set { SetValue(_value, value); }
        }

    }
}
