using System;
using GalaSoft.MvvmLight;

namespace Core
{
    public class AssignmentDTO : ObservableObject
    {
        public int Id { get; set; }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { Set("RecordTypeName", ref recordTypeName, value); }
        }

        private DateTime assignDateTime;
        public DateTime AssignDateTime
        {
            get { return assignDateTime; }
            set { Set("AssignDateTime", ref assignDateTime, value); }
        }

        private DateTime beginDateTime;
        public DateTime BeginDateTime
        {
            get { return beginDateTime; }
            set { Set("BeginDateTime", ref beginDateTime, value); }
        }

        private DateTime endDateTime;
        public DateTime EndDateTime
        {
            get { return endDateTime; }
            set { Set("EndDateTime", ref endDateTime, value); }
        }

        private double recordTypeCost;
        public double RecordTypeCost
        {
            get { return recordTypeCost; }
            set { Set("RecordTypeCost", ref recordTypeCost, value); }
        }
    }
}
