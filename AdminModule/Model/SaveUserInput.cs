using System;
using Core.Misc;

namespace AdminModule.Model
{
    public class SaveUserInput
    {
        public int PersonId { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? IsMale { get; set; }

        public ValueOf<byte[]> Photo { get; set; }

        public ValueOf<UserInfo> UserInfo { get; set; }
    }
}
