﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PersonVisitItemsListViewModels
{
    public class VisitDTO
    {
        public int Id { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string FinSource { get; set; }
        public string Name { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
