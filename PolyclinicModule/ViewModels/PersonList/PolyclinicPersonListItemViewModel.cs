
using System;
using System.Net;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace PolyclinicModule.ViewModels
{
    public class PolyclinicPersonListItemViewModel
    {
        public PolyclinicPersonListItemViewModel()
        {
            cells = new ObservableCollection<string>();
            children = new List<int>();
        }

        public static event Action<PolyclinicPersonListItemViewModel> RowExpanding;
        public static event Action<PolyclinicPersonListItemViewModel> RowCollapsing;

        #region Tree Properties

        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        ObservableCollection<string> cells;
        public ObservableCollection<string> Cells
        {
            get { return cells; }
            set { cells = value; }
        }

        List<int> children;
        public List<int> Children
        {
            get { return children; }
            set { children = value; }
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
                        if (PolyclinicPersonListItemViewModel.RowExpanding != null)
                            PolyclinicPersonListItemViewModel.RowExpanding(this);
                    }
                    else
                    {
                        if (PolyclinicPersonListItemViewModel.RowCollapsing != null)
                            PolyclinicPersonListItemViewModel.RowCollapsing(this);
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

        int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        #endregion

        int personId;
        public int PersonId
        {
            get { return personId; }
            set { personId = value; }
        }

        int? assignmentId;
        public int? AssignmentId
        {
            get { return assignmentId; }
            set { assignmentId = value; }
        }

        int? recordId;
        public int? RecordId
        {
            get { return recordId; }
            set { recordId = value; }
        }

        DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

    }
}
