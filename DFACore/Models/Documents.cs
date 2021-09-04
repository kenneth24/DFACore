using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class Documents
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }

        public List<DocumentInfo> Quantities { get; set; }
        public List<DocumentInfo> Info { get; set; }
    }


}
