using System;
using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class SiteSelectionViewModel
    {
        [Required(ErrorMessage = "Document Status is required.")]
        public string DocumentStatus { get; set; }
        [Required(ErrorMessage = "Document Type is required.")]
        public string DocumentType { get; set; }
        [Required(ErrorMessage = "Apostille Site is required.")]
        public string ApostileSite { get; set; }
        public bool HasExpidite { get; set; }
    }

}

