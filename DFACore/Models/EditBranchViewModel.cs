using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class EditBranchViewModel
    {
        public long Id { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public string MapAddress { get; set; }
        public string OfficeHours { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public bool HasExpidite { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<ScheduleCapacity> ScheduleCapacities { get; set; }
    }
}
