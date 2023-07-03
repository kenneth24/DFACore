using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class UnavailableDate
    {
        public int ApplicantCount { get; set; }
        public int DocuTypes { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int DocuCount { get; set; }
    }
}
