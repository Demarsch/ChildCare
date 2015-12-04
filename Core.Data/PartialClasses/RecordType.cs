using Core.Misc;

namespace Core.Data
{
    public partial class RecordType : IHierarchyItem
    {
        IHierarchyItem IHierarchyItem.Parent { get { return RecordType1; } }
    }
}
