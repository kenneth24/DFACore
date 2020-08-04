using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string MiddleName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Suffix { get; set; }
        //[Required]
        public string Address { get; set; }
        //[Required]
        public string Nationality { get; set; }
        //[Required]
        [StringLength(100)]
        public string ContactNumber { get; set; }

        [StringLength(100)]
        public string CompanyName { get; set; }
        [Required]
        [StringLength(100)]
        public string CountryDestination { get; set; }

        [StringLength(100)]
        public string NameOfRepresentative { get; set; }

        [StringLength(100)]
        public string RepresentativeContactNumber { get; set; }
        [Required]
        public string ApostileData { get; set; }

        public string ProcessingSite { get; set; }
        public string ProcessingSiteAddress { get; set; }
        [Required]
        public DateTime ScheduleDate { get; set; }
        //[Required]
        public string ApplicationCode { get; set; }
        public string Fees { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid CreatedBy { get; set; }
        public long Type { get; set; }
        public DateTime DateOfBirth { get; set; }

    }
}
