using System;
using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class SiteSelectionViewModel
    {
        [Required]
        public string DocumentStatus { get; set; }
        [Required]
        public string DocumentType { get; set; }
        [Required]
        public string ApostileSite { get; set; }
    }

}

