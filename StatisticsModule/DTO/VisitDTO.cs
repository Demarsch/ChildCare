using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.DTO
{    
    public class VisitDTO
    {
        public int Id { get; set; }
        public int VisitTemplateId { get; set; }
        public string Name { get; set; }
        public int ContractId { get; set; }
        public int FinancingSourceId { get; set; }
        public string ContractName { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string MKB { get; set; }
        public int UrgentlyId { get; set; }
        public string UrgentlyName { get; set; }
        public double Cost { get; set; }
        public string PaymentType { get; set; }

        public DateTime PersonBirthDate { get; set; }
        public string PatientFIO { get; set; }
        public string RelativeFIO { get; set; }
        public string CardNumber { get; set; }

        public int ExecutionPlaceId { get; set; }
        public string ExecutionPlace { get; set; }
        public string ExecutionPlaceOption { get; set; }
    }
}
