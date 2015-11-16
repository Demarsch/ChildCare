using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports
{
    public class ReportDataTable : IDisposable
    {
        Dictionary<int, Dictionary<int, object>> table = new Dictionary<int,Dictionary<int,object>>();

        public object this[int columnIndex, int rowIndex]
        {
            get
            {
                if (!table.ContainsKey(rowIndex))
                    table[rowIndex] = new Dictionary<int, object>();
                if (!table[rowIndex].ContainsKey(columnIndex))
                    table[rowIndex][columnIndex] = null;
                return table[rowIndex][columnIndex];
            }
            set
            {
                if (!table.ContainsKey(rowIndex))
                    table[rowIndex] = new Dictionary<int, object>();
                table[rowIndex][columnIndex] = value;
            }
        }

        public void SetRow(int rowIndex, params object[] cellValues)
        {
            if (!table.ContainsKey(rowIndex))
                table[rowIndex] = new Dictionary<int, object>();
            for (int i = 0; i < cellValues.Count(); i++)
                table[rowIndex][i] = cellValues[i];
        }

        public void AddRow(params object[] cellValues)
        {
            SetRow((table.Keys.Count > 0 ? table.Keys.Max() + 1 : 0), cellValues);
        }

        public int[] GetUsedRowIndexes()
        {
            return table.Keys.ToArray();
        }

        public int[] GetUsedColumnIndexes()
        {
            return table.Values.SelectMany(x => x.Keys).Distinct().ToArray();
        }

        public bool IsCellUsed(int rowindex, int colindex)
        {
            return table.ContainsKey(rowindex) && table[rowindex].ContainsKey(colindex);
        }

        public void Clear()
        {
            foreach (var item in table)
                item.Value.Clear();
            table.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
