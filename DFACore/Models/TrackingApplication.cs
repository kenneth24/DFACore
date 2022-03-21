using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class TrackingApplication
    {
        public string ApplicationCode { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ProcessingSite { get; set; }
        public string ProcessingSiteAddress { get; set; }
        public string COContactNumber { get; set; }
        public string COEmail { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string OwnerContactNumber { get; set; }
        public string CountryDestination { get; set; }
        public string ApostileData { get; set; }
        public string Email { get; set; }
        public string Fees { get; set; }
        public bool ReceivingStatus { get; set; }
        public bool AssessmentStatus { get; set; }
        public bool EncodingStatus { get; set; }
        public bool PrintingStatus { get; set; }
        public bool ReleasingStatus { get; set; }
    }
}
