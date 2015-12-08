using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;
using Shared.PatientRecords.ViewModels.RecordTypesProtocolViewModels;

namespace Shared.PatientRecords.Misc
{
    public class RecordTypeEditorResolver : IRecordTypeEditorResolver
    {
        private readonly Func<DefaultProtocolViewModel> defaultProtocol;

        private readonly Func<VisitProtocolViewModel> visitProtocol;

        public RecordTypeEditorResolver(Func<DefaultProtocolViewModel> defaultProtocol, Func<VisitProtocolViewModel> visitProtocol)
        {
            if (defaultProtocol == null)
            {
                throw new ArgumentNullException("defaultProtocol");
            }
            if (visitProtocol == null)
            {
                throw new ArgumentNullException("visitProtocol");
            }
            this.visitProtocol = visitProtocol;
            this.defaultProtocol = defaultProtocol;
        }

        public IRecordTypeProtocol Resolve(string editor)
        {
            switch (editor)
            {
                case "DefaultProtocol":
                    return defaultProtocol();
                case "VisitProtocol":
                    return visitProtocol();
                default:
                    return defaultProtocol();
            }
        }
    }
}
