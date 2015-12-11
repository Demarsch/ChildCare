using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;
using Shared.PatientRecords.ViewModels.RecordTypesProtocolViewModels;
using Shared.PatientRecords.ViewModels;

namespace Shared.PatientRecords.Misc
{
    public class RecordTypeEditorResolver : IRecordTypeEditorResolver
    {
        private readonly Func<DefaultProtocolViewModel> defaultProtocol;

        private readonly Func<VisitProtocolViewModel> visitProtocol;

        private readonly Func<AnalyseProtocolViewModel> analyseProtocol;

        public RecordTypeEditorResolver(Func<DefaultProtocolViewModel> defaultProtocol, Func<VisitProtocolViewModel> visitProtocol, Func<AnalyseProtocolViewModel> analyseProtocol)
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
            this.analyseProtocol = analyseProtocol;
        }

        public IRecordTypeProtocol Resolve(string editor)
        {
            switch (editor)
            {
                case "DefaultProtocol":
                    return defaultProtocol();
                case "VisitProtocol":
                    return visitProtocol();
                case "AnalyseProtocol":
                    return analyseProtocol();
                default:
                    return defaultProtocol();
            }
        }
    }
}
