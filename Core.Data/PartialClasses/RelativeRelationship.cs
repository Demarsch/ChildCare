namespace Core.Data
{
    public partial class RelativeRelationship
    {
        public override string ToString()
        {
            return string.Format("{0} - {1}", Id, Name);
        }
    }
}
