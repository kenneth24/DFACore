using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantModel
    {
        public string ApplicationCode { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string ContactNumber { get; set; }
        public string NameOfRepresentative { get; set; }
        public string RepresentativeContactNumber { get; set; }
        public string ProcessingSite { get; set; }
        public string Email { get; set; }
        public string CountryDestination { get; set; }
        public string DocumentName { get; set; }
        public int Quantity { get; set; }
        public string Transaction { get; set; }
    }
}
