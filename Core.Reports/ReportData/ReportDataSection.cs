using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports
{
    public class ReportDataSection : IDisposable
    {
        Dictionary<int, ReportData> items = new Dictionary<int, ReportData>();

        public ReportData this[int sectionIndex]
        {
            get
            {
                if (!items.ContainsKey(sectionIndex))
                    items[sectionIndex] = new ReportData();
                return items[sectionIndex];
            }
            set
            {
                items[sectionIndex] = value;
            }
        }

        public int[] GetUsedIndexes()
        {
            return items.Keys.ToArray();
        }

        public void Clear()
        {
            foreach (var item in items)
                item.Value.Dispose();
            items.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
