using Core.Extensions;

namespace Core.Data
{
    public partial class RecordContract
    {
        public string DisplayName
        {
            get
            {
                return (this.Number.HasValue ? "№" + this.Number.ToSafeString() + " - " : string.Empty) + this.ContractName;
            }
        }
    }
}
