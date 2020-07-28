using DFACore.Data;
using DFACore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class ActivityLogRepository
    {
        private ApplicationDbContext _context;
        public ActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ActivityLog activityLog)
        {
            activityLog.CreatedDate = DateTime.UtcNow;
            _context.ActivityLogs.Add(activityLog);
            _context.SaveChanges();
            return true;
        }
    }
}
