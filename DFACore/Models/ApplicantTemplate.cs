using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantTemplate
    {
        public string ApplicationCode { get; set; }
        public string ScheduleDate { get; set; }
        public string DateCreated { get; set; }
        public string Email { get; set; }
        public string DocumentOwner { get; set; }

        public string ContactNumber { get; set; }
        public string NameOfRepresentative { get; set; }
        public string RepresentativeContactNumber { get; set; }
        public string ConsularOffice { get; set; }
        public string Documents { get; set; }
        public string Quantity { get; set; }
        public string Transaction { get; set; }
        public string CountryDestination { get; set; }
     
    }
}
