namespace Core.Data
{
    public partial class Gender
    {
        public static readonly int MaleGenderId = 1;

        public static readonly int FemaleGenderId = 2;

        public bool IsMale { get { return Id == MaleGenderId; } }

        public bool IsFemale { get { return Id == FemaleGenderId; } }

        public bool IsUnknown { get { return !IsMale && !IsFemale; } }
    }
}
