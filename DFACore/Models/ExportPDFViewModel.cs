﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Models
{
    public class ExportPDFViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public IEnumerable<ExportTemplate> ExportTemplates { get; set; }
        public IEnumerable<ActivityLog> ActivityLogs { get; set; }
        public int Sum { get; set; }
    }
}
