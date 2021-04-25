using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    //Note: This is not included in this system, I just do this because this is existing logs that they have when verifying the records
    public class tbl_attendance
    {
        public long id { get; set; }
        public string applicationcode { get; set; }
        public string attendance { get; set; }
        public DateTime? date { get; set; }
    }
}
