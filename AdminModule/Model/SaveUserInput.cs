using System;

namespace AdminModule.Model
{
    public class SaveUserInput
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? IsMale { get; set; }

        public byte[] Photo { get; set; }
    }
}
