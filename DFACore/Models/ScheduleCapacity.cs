using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ScheduleCapacity
    {
        public long Id { get; set; }
        public long BranchId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
    }
}
