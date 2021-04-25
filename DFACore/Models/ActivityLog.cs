using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ActivityLog
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public string DeviceType { get; set; }
        public string Remarks { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
}
