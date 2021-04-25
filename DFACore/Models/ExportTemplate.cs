using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ExportTemplate
    {
        public string AppointmentCode { get; set; }
        public DateTime ScheduleDate { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }

        public string ContactNumber { get; set; }
        public string NameOfRepresentative { get; set; }
        public string RepresentativeContactNumber { get; set; }
        public string ConsularOffice { get; set; }
        public string Documents { get; set; }
        public string Quantity { get; set; }
        public string Transaction { get; set; }
        public string CountryDestination { get; set; }

        public string Email { get; set; }
        public int TotalDocuments { get; set; }
        public string Attendance { get; set; }

    }
}
