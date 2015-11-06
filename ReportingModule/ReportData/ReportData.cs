using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportingModule
{
    public class ReportData : IDisposable
    {
        public ReportData()
        {
            Fields = new Dictionary<string, object>();
            Tables = new ReportDataItemsCollection<ReportDataTable>();
            Sections = new ReportDataItemsCollection<ReportDataSection>();
        }

        public Dictionary<string, object> Fields { get; private set; }
        public ReportDataItemsCollection<ReportDataTable> Tables { get; private set; }
        public ReportDataItemsCollection<ReportDataSection> Sections { get; private set; }

        /// <summary>
        /// Simplification of Fields
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public object this[string fieldName]
        {
            get
            {
                return Fields[fieldName];
            }
            set
            {
                Fields[fieldName] = value;
            }
        }

        /// <summary>
        /// Simplification of Sections
        /// </summary>
        /// <param name="sectionName">Name of the section</param>
        /// <param name="sectionIndex">Index of section in section sequence</param>
        /// <returns>Section subreport data</returns>
        public ReportData this[string sectionName, int sectionIndex]
        {
            get
            {
                return Sections[sectionName][sectionIndex];
            }
            set
            {
                Sections[sectionName][sectionIndex] = value;
            }
        }

        /// <summary>
        /// Simplification of Tables
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnIndex">Column index in table</param>
        /// <param name="rowIndex">Row index in table</param>
        /// <returns>Cell value</returns>
        public object this[string tableName, int columnIndex, int rowIndex]
        {
            get
            {
                return Tables[tableName][columnIndex, rowIndex];
            }
            set
            {
                Tables[tableName][columnIndex, rowIndex] = value;
            }
        }

        public string[] GetUsedFields()
        {
            return Fields.Keys.ToArray();
        }
        
        public void ClearAllData()
        {
            Fields.Clear();
            Tables.Clear();
            Sections.Clear();
        }

        public void Dispose()
        {
            ClearAllData();
            if (Tables != null)
            {
                Tables.Dispose();
                Tables = null;
            }
            if (Sections == null)
                return;

            Sections.Dispose();
            Sections = null;
        }
    }
}
