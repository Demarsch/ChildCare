﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    public partial class PersonTalon
    {
        public string NumberWithDate
        {
            get
            {
                return "(" + this.TalonNumber + (!string.IsNullOrWhiteSpace(this.MKB) ? " - " + this.MKB : string.Empty) + ") от " + this.TalonDateTime.ToShortDateString();
            }
        }
    }
}
