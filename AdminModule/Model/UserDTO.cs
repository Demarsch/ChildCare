using System;

namespace AdminModule.Model
{
    public class UserDTO
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public DateTime ActiveFrom { get; set; }

        public DateTime ActiveTo { get; set; }

        public string Sid { get; set; }

        public string Login { get; set; }

        public string FullName { get; set; }

        public byte[] PhotoData { get; set; }
    }
}
