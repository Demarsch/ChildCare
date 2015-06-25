using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class PersonDataSaveDTO
    {

        public Person Person { get; set; }

        public IList<PersonName> PersonNames { get; set; }

        public IList<InsuranceDocument> PersonInsuranceDocuments { get; set; }

        public IList<PersonAddress> PersonAddresses { get; set; }

        public IList<PersonIdentityDocument> PersonIdentityDocuments { get; set; }

        public IList<PersonDisability> PersonDisabilities { get; set; }

        public IList<PersonSocialStatus> PersonSocialStatuses { get; set; }

        public int MaritalStatusId { get; set; }

        public int EducationId { get; set; }

        public int HealthGroupId { get; set; }

        public int NationalityId { get; set; }

        public int RelativeToPersonId { get; set; }

        public int RelativeRelationId { get; set; }

        public bool IsRepresentative { get; set; }
    }
}
