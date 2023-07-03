﻿using DFACore.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantRecordViewModel
    {
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [StringLength(100)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Suffix { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }
        //[Required]
        public string Barangay { get; set; }
        //[Required]
        public string City { get; set; }
        //[Required]
        public string Region { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        //[Required]
        [StringLength(100)]
        [Display(Name = "Nationality")]
        public string Nationality { get; set; }

        [StringLength(100)]
        [Display(Name = "Company / Office")]
        public string CompanyName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Country of Destination")]
        public string CountryDestination { get; set; }

        [StringLength(100)]
        [Display(Name = "Name of Authorized Representative")]
        public string NameOfRepresentative { get; set; }

        [StringLength(100)]
        [Display(Name = "Contact Number")]
        public string RepresentativeContactNumber { get; set; }
        //[Required]
        public string ApostileData { get; set; }
       
        [Display(Name = "Processing Site")]
        public string ProcessingSite { get; set; }
        
        [Display(Name = "Address")]
        public string ProcessingSiteAddress { get; set; }

        public string ScheduleDate { get; set; }

        public string ApplicationCode { get; set; }
        public string Fees { get; set; }
        //public DateTime? DateOfBirth { get; set; }

        //public List<ApostilleDocumentModel> Documents { get; set; }
    }
}
