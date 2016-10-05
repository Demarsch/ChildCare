using Core.Data;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleModule.ViewModels
{
    public class MultiAssignRecordTypeViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly ILog log;
        #endregion

        #region Constructors
        public MultiAssignRecordTypeViewModel(ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
        }
        #endregion

        #region Properties
        public int RecordTypeId { get; private set; }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { SetProperty(ref recordTypeName, value); }
        }

        #endregion

        #region Methods
        public void Initialize(RecordType recordType)
        {
            RecordTypeId = recordType.Id;
            RecordTypeName = recordType.Name;
        }

        public void Dispose()
        {

        }
        #endregion
    }
}
