using System;
using System.ComponentModel.DataAnnotations;

namespace DFACore.Models
{
    public class ApostilleScheduleViewModel
    {
        [Required]
        public string ScheduleDate { get; set; }
    }
}
