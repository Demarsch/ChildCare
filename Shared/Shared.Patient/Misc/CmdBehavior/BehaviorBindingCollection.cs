using System.Windows;

namespace Shared.Patient.Misc
{
    #region Namespace

    

    #endregion

    /// <summary>
    /// Represents collection to store the list of behaviors, inherits freezable so that it gets inheritance context for data-binding to work.
    /// </summary>
    public class BehaviorBindingCollection : FreezableCollection<BehaviorBinding>
    {
        /// <summary>
        /// Gets or sets the owner of the binding
        /// </summary>
        public DependencyObject Owner
        {
            get;
            set;
        }
    }
}
