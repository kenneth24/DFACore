using System;
using System.Collections.Generic;

namespace DFACore.Models
{
    public class MainViewModel
    {

        public string DocumentStatus { get; set; }
        public string DocumentType { get; set; }
        public string ApostileSite { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string ContactNumber { get; set; }
        public string CountryDestination { get; set; }
        public string NameOfRepresentative { get; set; }
        public string RepresentativeContactNumber { get; set; }
        public string ApostileData { get; set; }
        public string ProcessingSite { get; set; }
        public string ProcessingSiteAddress { get; set; }
        public bool HasExpedite { get; set; }
        public string ScheduleDate { get; set; }
        public decimal TotalFees { get; set; }
        public string ApplicationCode { get; set; }
        public ShippingInfoViewModel Shipping { get; set; }
        public List<ApplicantRecordViewModel> Applicants { get; set; }
    }
}
