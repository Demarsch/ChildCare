using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.DTO
{
    public class RecordDTO
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ContractId { get; set; }
        public int FinancingSourceId { get; set; }
        public string ContractName { get; set; }
        public DateTime AssignDateTime { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ActualDateTime { get; set; }
        public int RoomId { get; set; }
        public string Room { get; set; }
        public int? MKBId { get; set; }
        public string MKB { get; set; }
        public int UrgentlyId { get; set; }
        public string UrgentlyName { get; set; }
        public bool IsChild { get; set; }
        public double Cost { get; set; }
        public double Price { get; set; }
        public string PaymentType { get; set; }

        public DateTime PersonBirthDate { get; set; }
        public string PatientFIO { get; set; }
        public string RelativeFIO { get; set; }
        public string CardNumber { get; set; }
        public string BranchName { get; set; }
        public string Executor { get; set; }

        public int ExecutionPlaceId { get; set; }
        public string ExecutionPlace { get; set; }
        public string ExecutionPlaceOption { get; set; }

        public bool IsAnalyse { get; set; }

    }
}
