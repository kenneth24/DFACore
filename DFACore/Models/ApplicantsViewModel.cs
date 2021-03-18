using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantsViewModel
    {
        public ApplicantRecordViewModel Record { get; set; }
        public List<ApplicantRecordViewModel> Records { get; set; }
        public string ScheduleDate { get; set; }
        public int ApplicantCount { get; set; }
        public string Token { get; set; }
    }
}
