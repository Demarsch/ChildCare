﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.Misc
{
    public interface IRecordTypeProtocol
    {
        ProtocolMode CurrentMode { get; set; }

        void PrintProtocol();

        bool SaveProtocol();
    }

    public enum ProtocolMode
    {
        Edit,
        View
    }
}