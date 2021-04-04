using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class CalendarRange
    {
        public long Id { get; set; }
        public long BranchAddress { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
