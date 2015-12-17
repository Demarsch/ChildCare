using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Wpf.Misc
{
    public class BeforeSelectionChangedEventData
    {
        private readonly IList<BeforeSelectionChangedEventDataItem> actionsToPerform = new List<BeforeSelectionChangedEventDataItem>();

        public int NewId { get; private set; }

        public BeforeSelectionChangedEventData(int newId)
        {
            NewId = newId;
        }

        public bool IsCancelled { get; set; }

        public IEnumerable<BeforeSelectionChangedEventDataItem> ActionsToPerform { get { return actionsToPerform; } }

        public void AddActionToPerform(Func<Task<bool>> action, Action onFail = null)
        {
            actionsToPerform.Add(new BeforeSelectionChangedEventDataItem(action, onFail));
        }
    }

    public class BeforeSelectionChangedEventDataItem
    {
        public Func<Task<bool>> Action { get; private set; }

        public Action OnFail { get; private set; }

        public BeforeSelectionChangedEventDataItem(Func<Task<bool>> action, Action onFail = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Action = action;
            OnFail = onFail;
        }
    }
}
