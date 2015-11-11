using System.ComponentModel;

namespace Core.Misc
{
    public interface IActiveDataErrorInfo : IDataErrorInfo
    {
        bool Validate();

        void CancelValidation();
    }
}
