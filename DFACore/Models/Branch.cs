using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class Branch
    {
        public long Id { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
