using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace StatisticsModule.Classes
{
    public class DataGridRowDefinition
    {
        public DataGridRowDefinition()
        {
            cells = new ObservableCollection<string>();
            children = new List<int>();
        }
       
        public static event Action<DataGridRowDefinition> RowExpanding;
        public static event Action<DataGridRowDefinition> RowCollapsing;

        ObservableCollection<string> cells;
        public ObservableCollection<string> Cells
        {
            get { return cells; }
            internal set { cells = value; }
        }

        List<DataGridRowDefinition> details;
        public List<DataGridRowDefinition> Details
        {
            get { return details; }
            set { details = value; }
        }

        List<int> children;
        public List<int> Children
        {
            get { return children; }
            set { children = value; }
        }

        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        int? parentId;
        public int? ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        int level;
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        bool? isExpanded;
        public bool? IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    if (isExpanded.Value)
                    {
                        if (DataGridRowDefinition.RowExpanding != null)
                            DataGridRowDefinition.RowExpanding(this);
                    }
                    else
                    {
                        if (DataGridRowDefinition.RowCollapsing != null)
                            DataGridRowDefinition.RowCollapsing(this);
                    }
                }
            }
        }

        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        bool hasChildren;
        public bool HasChildren
        {
            get { return hasChildren; }
            set { hasChildren = value; }
        }
    }
}
