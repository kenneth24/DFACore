using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class Holiday
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}
