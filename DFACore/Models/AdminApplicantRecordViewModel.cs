using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class AdminApplicantRecordViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string ContactNumber { get; set; }
        public string CompanyName { get; set; }
        public string CountryDestination { get; set; }
        public string NameOfRepresentative { get; set; }
        public string RepresentativeContactNumber { get; set; }
        public string ApostileData { get; set; }
        public string ProcessingSite { get; set; }
        public string ProcessingSiteAddress { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ApplicationCode { get; set; }
        public string Fees { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid CreatedBy { get; set; }
        public long Type { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long BranchId { get; set; }
        public string Email { get; set; }

        public List<ApostilleDocumentModel> Data { get; set; }
        public string Attendance { get; set; }
    }
}
