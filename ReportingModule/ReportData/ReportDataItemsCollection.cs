using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportingModule
{
    public class ReportDataItemsCollection<ReportDataItemType> : IDisposable where ReportDataItemType : IDisposable, new()
    {
        Dictionary<string, ReportDataItemType> items = new Dictionary<string, ReportDataItemType>();

        public ReportDataItemType this[string itemName]
        {
            get
            {
                if (!items.ContainsKey(itemName))
                    items[itemName] = new ReportDataItemType();
                return items[itemName];
            }
            set
            {
                items[itemName] = value;
            }
        }

        public string[] GetUsedNames()
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
