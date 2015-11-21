using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Misc;
using Prism.Mvvm;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorEditRecordTypeViewModel : BindableBase, IDataErrorInfo
    {
        public ScheduleEditorEditRecordTypeViewModel()
        {
            IsChanged = true;
        }

        private bool isChanged;

        public bool IsChanged
        {
            get { return isChanged; }
            set { SetProperty(ref isChanged, value); }
        }

        private int recordTypeId;

        public int RecordTypeId
        {
            get { return recordTypeId; }
            set
            {
                if (SetProperty(ref recordTypeId, value))
                {
                    IsChanged = true;
                };
            }
        }

        private string times;

        public string Times
        {
            get { return times; }
            set
            {
                if (SetProperty(ref times, value))
                {
                    IsChanged = true;
                }
            }
        }

        private static readonly Regex TimeRegex = new Regex(@"\d{1,2}\:\d{2}");

        public IEnumerable<ITimeInterval> TimeIntervals
        {
            get
            {
                var parsedTimes = TimeRegex.Matches(times).Cast<Match>().Select(x => TimeSpan.Parse(x.Value)).ToArray();
                var index = 0;
                var result = new List<ITimeInterval>();
                while (index + 1 < parsedTimes.Length)
                {
                    var startTime = parsedTimes[index];
                    var endTime = parsedTimes[index + 1];
                    if (startTime < endTime)
                    {
                        result.Add(new TimeInterval(startTime, endTime));
                    }
                    index += 2;
                }
                return result;
            }
            set { Times = value == null ? string.Empty : string.Join(", ", value.Select(x => string.Format("{0:hh\\:mm}-{1:hh\\:mm}", x.StartTime, x.EndTime))); }
        }

        public string this[string columnName]
        {
            get
            {
                if (string.CompareOrdinal(columnName, "Times") == 0)
                {
                    return TimeIntervals.Any() ? string.Empty : "Должен быть указан хотя бы один временной промежуток (напр.10:00 - 17:00)";
                }
                return string.Empty;
            }
        }

        public string Error
        {
            get { return this["Times"]; }
        }
    }
}