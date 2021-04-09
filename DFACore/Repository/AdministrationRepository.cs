using DFACore.Data;
using DFACore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class AdministrationRepository
    {
        private ApplicationDbContext _context;
        public AdministrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<ApplicantRecord> GetAllApplicantRecord(string sortOrder, string searchString)
        {
            //var result = _context.ApplicantRecords.Select(a => a);
            var applicants = from s in _context.ApplicantRecords
                             select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                applicants = applicants.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.ApplicationCode.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    applicants = applicants.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    applicants = applicants.OrderBy(s => s.ScheduleDate);
                    break;
                case "date_desc":
                    applicants = applicants.OrderByDescending(s => s.ScheduleDate);
                    break;
                default:
                    applicants = applicants.OrderBy(s => s.LastName);
                    break;
            }
            var result = applicants.AsNoTracking();

            return result;
        }

        public IQueryable<Branch> GetBranches(string sortOrder, string searchString)
        {
            var branches = from s in _context.Branches
                             select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                branches = branches.Where(s => s.BranchName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    branches = branches.OrderByDescending(s => s.BranchName);
                    break;
                default:
                    branches = branches.OrderBy(s => s.Id);
                    break;
            }
            var result = branches.AsNoTracking();

            return result;
        }

        public IEnumerable<Branch> GetBranches()
        {
            var branches = _context.Branches.Select(a => a);
            return branches;
        }

        public Branch GetBranch(long branchId)
        { 
            var branch = _context.Branches.Where(a => a.Id == branchId).Include(a => a.ScheduleCapacities).FirstOrDefault();
            return branch;
        }

        public EditBranchViewModel GetBranchForEdit(long branchId)
        {
            var branch = GetBranch(branchId);
            var result = new EditBranchViewModel
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                BranchAddress = branch.BranchAddress,
                DateCreated = branch.DateCreated,
                IsActive = branch.IsActive,
                MapAddress = branch.MapAddress,
                ContactNumber = branch.ContactNumber,
                Email = branch.Email,
                OfficeHours = branch.OfficeHours,
                HasExpidite = branch.HasExpidite,
                ScheduleCapacities = branch.ScheduleCapacities.Select(sc => new ScheduleCapacity {
                    Id = sc.Id,
                    Type = sc.Type,
                    Name = sc.Name,
                    Capacity = sc.Capacity,
                    BranchId = sc.BranchId
                }).ToList()
            };
            return result;
        }

        public async Task<EditBranchViewModel> UpdateBranch(EditBranchViewModel branch)
        {
            try
            {
                var rawBranch = new Branch
                {
                    Id = branch.Id,
                    BranchName = branch.BranchName,
                    BranchAddress = branch.BranchAddress,
                    DateCreated = branch.DateCreated,
                    IsActive = branch.IsActive,
                    MapAddress = branch.MapAddress,
                    ContactNumber = branch.ContactNumber,
                    Email = branch.Email,
                    OfficeHours = branch.OfficeHours,
                    HasExpidite = branch.HasExpidite,
                };

                _context.Branches.Update(rawBranch);
                _context.ScheduleCapacities.UpdateRange(branch.ScheduleCapacities);
                await _context.SaveChangesAsync();
                return branch;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            
        }

        public List<AdminAccountViewModel> AccountList()
        {
            var raw = _context.Set<AdminAccountViewModel>().FromSqlRaw(
                "select u.Id, u.Email, u.FirstName, u.MiddleName, u.LastName, u.Suffix, u.Gender, " +
                "u.DateOfBirth, u.CreatedDate, r.[Name] as Roles, b.BranchName as Branch from AspNetUsers u " +
                "inner join AspNetUserRoles ur on u.Id = ur.UserId " +
                "inner join AspNetRoles r on ur.RoleId = r.Id " +
                "inner join Branches b on u.BranchId = b.Id " +
                "where u.Type = 1").ToList();

            //var result = raw.Select(a => a.ScheduleDate).ToList();
            return raw;
        }

    }
}
