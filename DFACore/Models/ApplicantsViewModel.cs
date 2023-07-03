using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ApplicantsViewModel
    {
        public ApplicantRecordViewModel Record { get; set; } = new();
        public ApplicantRecordViewModel AuthRecord { get; set; } = new();
        public List<ApplicantRecordViewModel> Records { get; set; } = new();
        public string ScheduleDate { get; set; }
        public string ProcessingSite { get; set; }
        public string ProcessingSiteAddress { get; set; }
        public int ApplicantCount { get; set; }
        public string Token { get; set; }
    }
}
