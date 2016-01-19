namespace AdminModule.Model
{
    public class UserInfo
    {
        public string Login { get; set; }

        public string Sid { get; set; }

        public string FullName { get; set; }

        protected bool Equals(UserInfo other)
        {
            return string.Equals(Login, other.Login) && string.Equals(Sid, other.Sid) && string.Equals(FullName, other.FullName);
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
            return Equals((UserInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Login != null ? Login.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Sid != null ? Sid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FullName != null ? FullName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
