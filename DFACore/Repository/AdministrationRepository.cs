using DFACore.Data;
using DFACore.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public IQueryable<AdminApplicantRecordViewModel> GetAllApplicantRecord(string sortOrder, string searchString)
        {
            //var result = _context.ApplicantRecords.Select(a => a);
            var applicants = from ar in _context.ApplicantRecords
                             join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                             select new AdminApplicantRecordViewModel { 
                                 ApplicationCode = ar.ApplicationCode,
                                 ScheduleDate = ar.ScheduleDate,
                                 FirstName = ar.FirstName,
                                 MiddleName = ar.MiddleName,
                                 LastName = ar.LastName,
                                 ContactNumber = ar.ContactNumber,
                                 NameOfRepresentative = ar.NameOfRepresentative,
                                 RepresentativeContactNumber = ar.RepresentativeContactNumber,
                                 ProcessingSite = ar.ProcessingSite,
                                 ApostileData = ar.ApostileData,
                                 CountryDestination = ar.CountryDestination,
                                 Email = u.Email,
                             };
            if (!String.IsNullOrEmpty(searchString))
            {
                applicants = applicants.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.ApplicationCode.Contains(searchString)
                                       || s.Email.Contains(searchString));
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
                    applicants = applicants.OrderByDescending(s => s.ScheduleDate);
                    break;
            }
            var result = applicants.AsNoTracking();

            return result;
        }

        public IQueryable<AdminApplicantRecordViewModel> GetAllApplicantRecordByBranch(long? branchId, string sortOrder, string searchString)
        {
            //var result = _context.ApplicantRecords.Select(a => a);
            var applicants = from ar in _context.ApplicantRecords where ar.BranchId == branchId
                             join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                             select new AdminApplicantRecordViewModel
                             {
                                 ApplicationCode = ar.ApplicationCode,
                                 ScheduleDate = ar.ScheduleDate,
                                 FirstName = ar.FirstName,
                                 MiddleName = ar.MiddleName,
                                 LastName = ar.LastName,
                                 ContactNumber = ar.ContactNumber,
                                 NameOfRepresentative = ar.NameOfRepresentative,
                                 RepresentativeContactNumber = ar.RepresentativeContactNumber,
                                 ProcessingSite = ar.ProcessingSite,
                                 ApostileData = ar.ApostileData,
                                 CountryDestination = ar.CountryDestination,
                                 Email = u.Email,
                             };
            if (!String.IsNullOrEmpty(searchString))
            {
                applicants = applicants.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.ApplicationCode.Contains(searchString)
                                       || s.Email.Contains(searchString));
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
                    applicants = applicants.OrderByDescending(s => s.ScheduleDate);
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
                StartTime = branch.StartTime,
                EndTime = branch.EndTime,
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
                    StartTime = branch.StartTime,
                    EndTime = branch.EndTime
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

        public IQueryable<UserAccountViewModel> UserList(string sortOrder, string searchString)
        {
            var raw = _context.Set<UserAccountViewModel>().FromSqlRaw(
                "select u.Id, u.Email, u.FirstName, u.MiddleName, u.LastName, u.Suffix, u.Gender, " +
                "u.DateOfBirth, u.CreatedDate from AspNetUsers u " +
                "where u.Type = 0 and u.LockoutEnd is null");

            if (!String.IsNullOrEmpty(searchString))
            {
                raw = raw.Where(s => s.Email.Contains(searchString)
                                || s.FirstName.Contains(searchString)
                                || s.LastName.Contains(searchString));
                                
            }
            switch (sortOrder)
            {
                case "name_desc":
                    raw = raw.OrderByDescending(s => s.Id);
                    break;
                default:
                    raw = raw.OrderByDescending(s => s.CreatedDate);
                    break;
            }
            var result = raw.AsNoTracking();

            //var result = raw.Select(a => a.ScheduleDate).ToList();
            return result;
        }

        public IQueryable<UserAccountViewModel> Blacklist(string sortOrder, string searchString)
        {
            var raw = _context.Set<UserAccountViewModel>().FromSqlRaw(
                "select u.Id, u.Email, u.FirstName, u.MiddleName, u.LastName, u.Suffix, u.Gender, " +
                "u.DateOfBirth, u.CreatedDate from AspNetUsers u " +
                "where u.Type = 0 and u.LockoutEnd is not null");

            if (!String.IsNullOrEmpty(searchString))
            {
                raw = raw.Where(s => s.Email.Contains(searchString)
                                || s.FirstName.Contains(searchString)
                                || s.LastName.Contains(searchString));

            }
            switch (sortOrder)
            {
                case "name_desc":
                    raw = raw.OrderByDescending(s => s.Id);
                    break;
                default:
                    raw = raw.OrderByDescending(s => s.CreatedDate);
                    break;
            }
            var result = raw.AsNoTracking();

            //var result = raw.Select(a => a.ScheduleDate).ToList();
            return result;
        }

        public async Task<bool> AddToBlackList(string userId)
        {
            try
            {
                var user = _context.Users.Where(a => a.Id == userId).FirstOrDefault();

                user.LockoutEnd = DateTime.Now.AddYears(50);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

        }

        public ApplicationUser EditAccount(string userId)
        {
            try
            {
                var user = _context.Users.Where(a => a.Id == userId).AsNoTracking().FirstOrDefault();
                return user;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Unable to delete. Try again, and if the problem persists, see your system administrator.");
            }
        }

        public async Task<UpdateAccountViewModel> EditAccount(UpdateAccountViewModel model)
        {
            try
            {
                var raw = _context.Users.Where(a => a.Id == model.Id).FirstOrDefault();

                raw.FirstName = model.FirstName;
                raw.MiddleName = model.MiddleName;
                raw.LastName = model.LastName;
                raw.Suffix = model.Suffix;
                raw.PhoneNumber = model.PhoneNumber;
                raw.Gender = model.Gender;
                raw.DateOfBirth = model.DateOfBirth;
                
                _context.Users.Update(raw);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Unable to delete. Try again, and if the problem persists, see your system administrator.");
            }
        }

        public async Task<bool> DeleteAccount(string userId)
        {
            try
            {
                var user = _context.Users.Where(a => a.Id == userId).AsNoTracking().FirstOrDefault();

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Unable to delete. Try again, and if the problem persists, see your system administrator.");
            }
        }

        public List<AdminAccountViewModel> AccountListByBranch(long? branchId)
        {
            var raw = _context.Set<AdminAccountViewModel>().FromSqlRaw(
                "select u.Id, u.Email, u.FirstName, u.MiddleName, u.LastName, u.Suffix, u.Gender, " +
                "u.DateOfBirth, u.CreatedDate, r.[Name] as Roles, b.BranchName as Branch from AspNetUsers u " +
                "inner join AspNetUserRoles ur on u.Id = ur.UserId " +
                "inner join AspNetRoles r on ur.RoleId = r.Id " +
                "inner join Branches b on u.BranchId = b.Id " +
                "where u.Type = 1 and u.BranchId = {0}", branchId).ToList();

            //var result = raw.Select(a => a.ScheduleDate).ToList();
            return raw;
        }

        public IQueryable<Holiday> GetHoliday(string sortOrder, string searchString)
        {
            IQueryable<Holiday> holidays;
            if (!String.IsNullOrEmpty(searchString))
            {
                holidays = _context.Holidays.Where(s => s.BranchId.Equals(Convert.ToInt64(searchString)));
            }
            else
            { 
                holidays = _context.Holidays.Where(a => a.BranchId == 0 && a.Date.Year == DateTime.Now.Year);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    holidays = holidays.OrderByDescending(s => s.Date);
                    break;
                default:
                    holidays = holidays.OrderByDescending(s => s.Date);
                    break;
            }
            var result = holidays.AsNoTracking();

            return result;
        }

        public string GetBranchName(long branchId)
        {
            var branch = _context.Branches.Where(a => a.Id == branchId).FirstOrDefault();
            return branch.BranchName;
        }

        public Holiday AddHoliday(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            _context.SaveChanges();
            return holiday;
        }

        public bool DeleteHoliday(long holidayId)
        {
            var raw = _context.Holidays.Where(a => a.Id == holidayId).FirstOrDefault();
            _context.Holidays.Remove(raw);
            _context.SaveChanges();
            return true;
        }


        public IQueryable<ActivityLog> GetActivityLogs(string sortOrder, string searchString)
        {
            var activityLogs = from s in _context.ActivityLogs where s.UserId != "1"
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                activityLogs = activityLogs.Where(s => s.Email.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    activityLogs = activityLogs.OrderByDescending(s => s.CreatedDate);
                    break;
                default:
                    activityLogs = activityLogs.OrderByDescending(s => s.CreatedDate);
                    break;
            }
            var result = activityLogs.AsNoTracking();

            return result;
        }

        public IQueryable<ActivityLog> GetActivityLogs(DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);
            var activityLogs = from s in _context.ActivityLogs
                               where s.UserId != "1" && (s.CreatedDate >= dateFrom && s.CreatedDate < dateTo)
                               select s;
            
            var result = activityLogs.AsNoTracking();

            return result;
        }

        public Price GetPrice()
        {
            var price = _context.Prices.Select(a => a).FirstOrDefault();
            return price;
        }

        public Price UpdatePrice(Price price)
        {
            if (price == null)
            {
                throw new Exception("Value cannot be null.");
            }
            _context.Prices.Update(price);
            _context.SaveChanges();
            return price;
        }

        public Notice GetNotice()
        {
            var notice = _context.Notices.Select(a => a).FirstOrDefault();
            return notice;
        }

        public Notice UpdateNotice(Notice notice)
        {
            if (notice == null)
            {
                throw new Exception("Value cannot be null.");
            }
            _context.Notices.Update(notice);
            _context.SaveChanges();
            return notice;
        }

        public IEnumerable<ExportTemplate> ExportApplicantRecord(DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);

            var raw = from ar in _context.ApplicantRecords where ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo
                             join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                             select new AdminApplicantRecordViewModel
                             {
                                 ApplicationCode = ar.ApplicationCode,
                                 ScheduleDate = ar.ScheduleDate,
                                 FirstName = ar.FirstName,
                                 MiddleName = ar.MiddleName,
                                 LastName = ar.LastName,
                                 ContactNumber = ar.ContactNumber,
                                 NameOfRepresentative = ar.NameOfRepresentative,
                                 RepresentativeContactNumber = ar.RepresentativeContactNumber,
                                 ProcessingSite = ar.ProcessingSite,
                                 Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                                 Email = u.Email,
                                 Suffix = ar.Suffix
                             };

            //var raw = applicants.ToList();

            var applicants = raw.AsEnumerable().Select(ar => new ExportTemplate
            {
                AppointmentCode = ar.ApplicationCode,
                ScheduleDate = ar.ScheduleDate,
                FirstName = ar.FirstName,
                MiddleName = ar.MiddleName,
                LastName = ar.LastName,
                Suffix = ar.Suffix,
                ContactNumber = ar.ContactNumber,
                NameOfRepresentative = ar.NameOfRepresentative,
                RepresentativeContactNumber = ar.RepresentativeContactNumber,
                ConsularOffice = ar.ProcessingSite,
                Documents = string.Join(Environment.NewLine, ar.Data.Select(d => d.Name)),
                Quantity = string.Join(Environment.NewLine, ar.Data.Select(d => d.Quantity)),
                Transaction = string.Join(Environment.NewLine, ar.Data.Select(d => d.Transaction)),
                TotalDocuments = ar.Data.Select(d => d.Quantity).Sum(),
                Email =ar.Email,
            });
            

            return applicants.OrderBy(a => a.ScheduleDate);
        }

        public IEnumerable<ExportTemplate> ExportApplicantRecordByBranch(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);

            var raw = from ar in _context.ApplicantRecords
                      where ar.BranchId == branchId && (ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo)
                      join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                      select new AdminApplicantRecordViewModel
                      {
                          ApplicationCode = ar.ApplicationCode,
                          ScheduleDate = ar.ScheduleDate,
                          FirstName = ar.FirstName,
                          MiddleName = ar.MiddleName,
                          LastName = ar.LastName,
                          ContactNumber = ar.ContactNumber,
                          NameOfRepresentative = ar.NameOfRepresentative,
                          RepresentativeContactNumber = ar.RepresentativeContactNumber,
                          ProcessingSite = ar.ProcessingSite,
                          Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                          Email = u.Email,
                      };

            //var raw = applicants.ToList();

            var applicants = raw.AsEnumerable().Select(ar => new ExportTemplate
            {
                AppointmentCode = ar.ApplicationCode,
                ScheduleDate = ar.ScheduleDate,
                FirstName = ar.FirstName,
                MiddleName = ar.MiddleName,
                LastName = ar.LastName,
                ContactNumber = ar.ContactNumber,
                NameOfRepresentative = ar.NameOfRepresentative,
                RepresentativeContactNumber = ar.RepresentativeContactNumber,
                ConsularOffice = ar.ProcessingSite,
                Documents = string.Join(Environment.NewLine, ar.Data.Select(d => d.Name)),
                Quantity = string.Join(Environment.NewLine, ar.Data.Select(d => d.Quantity)),
                Transaction = string.Join(Environment.NewLine, ar.Data.Select(d => d.Transaction)),
                TotalDocuments = ar.Data.Select(d => d.Quantity).Sum(),
                Email = ar.Email,
            });


            return applicants.OrderBy(a => a.ScheduleDate);
        }
        public IEnumerable<ExportTemplate> ExportAppointmentToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);
            IQueryable<AdminApplicantRecordViewModel> raw;

            if (branchId == default)
            {
                raw = from ar in _context.ApplicantRecords
                      where ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo
                      join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                      select new AdminApplicantRecordViewModel
                      {
                          ApplicationCode = ar.ApplicationCode,
                          ScheduleDate = ar.ScheduleDate,
                          FirstName = ar.FirstName,
                          MiddleName = ar.MiddleName,
                          LastName = ar.LastName,
                          ContactNumber = ar.ContactNumber,
                          NameOfRepresentative = ar.NameOfRepresentative,
                          RepresentativeContactNumber = ar.RepresentativeContactNumber,
                          ProcessingSite = ar.ProcessingSite,
                          Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                          Email = u.Email,
                          Suffix = ar.Suffix,
                          CountryDestination = ar.CountryDestination
                      };
            }
            else
            {
                raw = from ar in _context.ApplicantRecords
                      where ar.BranchId == branchId && (ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo)
                      join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                      select new AdminApplicantRecordViewModel
                      {
                          ApplicationCode = ar.ApplicationCode,
                          ScheduleDate = ar.ScheduleDate,
                          FirstName = ar.FirstName,
                          MiddleName = ar.MiddleName,
                          LastName = ar.LastName,
                          ContactNumber = ar.ContactNumber,
                          NameOfRepresentative = ar.NameOfRepresentative,
                          RepresentativeContactNumber = ar.RepresentativeContactNumber,
                          ProcessingSite = ar.ProcessingSite,
                          Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                          Email = u.Email,
                          Suffix = ar.Suffix,
                          CountryDestination = ar.CountryDestination
                      };
            }

            

            //var raw = applicants.ToList();

            var applicants = raw.AsEnumerable().Select(ar => new ExportTemplate
            {
                AppointmentCode = ar.ApplicationCode,
                ScheduleDate = ar.ScheduleDate,
                FirstName = ar.FirstName,
                MiddleName = ar.MiddleName,
                LastName = ar.LastName,
                Suffix = ar.Suffix,
                ContactNumber = ar.ContactNumber,
                NameOfRepresentative = ar.NameOfRepresentative,
                RepresentativeContactNumber = ar.RepresentativeContactNumber,
                ConsularOffice = ar.ProcessingSite,
                Documents = string.Join("<br>", ar.Data.Select(d => d.Name)),
                Quantity = string.Join("<br>", ar.Data.Select(d => d.Quantity)),
                Transaction = string.Join("<br>", ar.Data.Select(d => d.Transaction)),
                TotalDocuments = ar.Data.Select(d => d.Quantity).Sum(),
                Email = ar.Email,
                CountryDestination = ar.CountryDestination
            });

            return applicants.OrderBy(a => a.ScheduleDate);
        }

        public IEnumerable<ExportTemplate> ExportAttendanceToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);

            IQueryable<AdminApplicantRecordViewModel> raw;

            if (branchId == default)
            {
                raw = from ar in _context.ApplicantRecords
                      where ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo
                      join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                      join a in _context.tbl_attendance on ar.ApplicationCode equals a.applicationcode into attended
                      from attendance in attended.DefaultIfEmpty()
                      select new AdminApplicantRecordViewModel
                      {
                          ApplicationCode = ar.ApplicationCode,
                          ScheduleDate = ar.ScheduleDate,
                          FirstName = ar.FirstName,
                          MiddleName = ar.MiddleName,
                          LastName = ar.LastName,
                          ContactNumber = ar.ContactNumber,
                          NameOfRepresentative = ar.NameOfRepresentative,
                          RepresentativeContactNumber = ar.RepresentativeContactNumber,
                          ProcessingSite = ar.ProcessingSite,
                          Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                          Email = u.Email,
                          Suffix = ar.Suffix,
                          CountryDestination = ar.CountryDestination,
                          Attendance = attendance.attendance
                      };
            }

            else
            {
                raw = from ar in _context.ApplicantRecords
                      where ar.BranchId == branchId && (ar.ScheduleDate >= dateFrom && ar.ScheduleDate < dateTo)
                      join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                      join a in _context.tbl_attendance on ar.ApplicationCode equals a.applicationcode into attended
                      from attendance in attended.DefaultIfEmpty()
                      select new AdminApplicantRecordViewModel
                      {
                          ApplicationCode = ar.ApplicationCode,
                          ScheduleDate = ar.ScheduleDate,
                          FirstName = ar.FirstName,
                          MiddleName = ar.MiddleName,
                          LastName = ar.LastName,
                          ContactNumber = ar.ContactNumber,
                          NameOfRepresentative = ar.NameOfRepresentative,
                          RepresentativeContactNumber = ar.RepresentativeContactNumber,
                          ProcessingSite = ar.ProcessingSite,
                          Data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(ar.ApostileData),
                          Email = u.Email,
                          Suffix = ar.Suffix,
                          CountryDestination = ar.CountryDestination,
                          Attendance = attendance.attendance
                      };
            }

            

            //var raw = applicants.ToList();

            var applicants = raw.AsEnumerable().Select(ar => new ExportTemplate
            {
                AppointmentCode = ar.ApplicationCode,
                ScheduleDate = ar.ScheduleDate,
                FirstName = ar.FirstName,
                MiddleName = ar.MiddleName,
                LastName = ar.LastName,
                Suffix = ar.Suffix,
                ContactNumber = ar.ContactNumber,
                NameOfRepresentative = ar.NameOfRepresentative,
                RepresentativeContactNumber = ar.RepresentativeContactNumber,
                ConsularOffice = ar.ProcessingSite,
                Documents = string.Join("<br>", ar.Data.Select(d => d.Name)),
                Quantity = string.Join("<br>", ar.Data.Select(d => d.Quantity)),
                Transaction = string.Join("<br>", ar.Data.Select(d => d.Transaction)),
                TotalDocuments = ar.Data.Select(d => d.Quantity).Sum(),
                Email = ar.Email,
                CountryDestination = ar.CountryDestination,
                Attendance = !string.IsNullOrEmpty(ar.Attendance) ? "Yes" : "No"
            });

            //var test = applicants.Where(a => a.Attendance == "No");
            return applicants.OrderBy(a => a.ScheduleDate);
        }


    }
}



//improve calendar date range when deleteing unavailable