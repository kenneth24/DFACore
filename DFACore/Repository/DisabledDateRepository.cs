using DFACore.Data;
using DFACore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class DisabledDateRepository
    {
        private ApplicationDbContext _context;
        public DisabledDateRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(DisabledDate disabledDate)
        {
            disabledDate.CreatedDate = DateTime.UtcNow;
            disabledDate.IsActive = true;
            _context.DisabledDates.Add(disabledDate);
            _context.SaveChanges();
            return true;
        }

        public bool Update(DisabledDate disabledDate)
        {
            if (disabledDate.Id.Equals(default))
                throw new Exception("Id cannot be null");
            _context.DisabledDates.Update(disabledDate);
            _context.SaveChanges();
            return true;
        }

        public bool Delete(long id)
        {
            if (id.Equals(default))
                throw new Exception("Id cannot be null");
            var disabledDate = _context.DisabledDates.Find(id);
            if (disabledDate is null)
                throw new Exception("Invalid operation.");
            disabledDate.IsActive = false;
            _context.DisabledDates.Update(disabledDate);
            _context.SaveChanges();
            return true;

        }
    }
}
