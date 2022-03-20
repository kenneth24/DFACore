using System;
using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class SiteSelectionViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        public string DocumentStatus { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string DocumentType { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string ApostileSite { get; set; }
        public bool HasExpidite { get; set; }
    }

}

