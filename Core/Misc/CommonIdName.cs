namespace Core.Misc
{
    public class CommonIdName
    {
        public int Id { get; set; }

        public string Name { get; set; }

        protected bool Equals(CommonIdName other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((CommonIdName)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
