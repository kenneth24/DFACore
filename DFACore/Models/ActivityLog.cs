using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ActivityLog
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
    }
}
