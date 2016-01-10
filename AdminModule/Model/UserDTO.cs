﻿using System;

namespace AdminModule.Model
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public int PersonId { get; set; }

        public DateTime ActiveFrom { get; set; }

        public DateTime ActiveTo { get; set; }

        public string Sid { get; set; }

        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public byte[] PhotoData { get; set; }

        public bool IsMale { get; set; }
    }
}
