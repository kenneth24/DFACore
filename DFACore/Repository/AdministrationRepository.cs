using DFACore.Data;
using DFACore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DCore = System.Linq.Dynamic.Core;

namespace DFACore.Repository
{
    public class AdministrationRepository
    {
        private ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        public AdministrationRepository(ApplicationDbContext context,
            IEmailService emailService,
            IWebHostEnvironment env)
        {
            _context = context;
            _emailService = emailService;
            _env = env;
        }

        public ApplicantRecord GetApplicantRecord(string applicationCode)
        {
            var applicant = _context.ApplicantRecords.Where(a => a.ApplicationCode == applicationCode).FirstOrDefault();
            return applicant;
        }

        public ApplicantRecord GetApplicantRecord(Guid id, string applicationCode)
        {
            var applicant = _context.ApplicantRecords.Where(a => a.CreatedBy == id && a.ApplicationCode == applicationCode).FirstOrDefault();
            return applicant;
        }

        public IQueryable<ApplicantRecord> GetCancelledApplicantRecord(Guid id, string applicationCode)
        {
            IQueryable<ApplicantRecord> raw;

            var str = $"select top 1 Id, Title, FirstName, MiddleName, LastName, Suffix, [Address], Nationality, ContactNumber, " +
                "CompanyName, CountryDestination, NameOfRepresentative, RepresentativeContactNumber, ApostileData, " +
                "ProcessingSite, ProcessingSiteAddress, ScheduleDate, ApplicationCode, Fees, DateCreated, CreatedBy, " +
                "[Type], DateOfBirth, BranchId, TotalApostile " +
                $"from ApplicantRecords_backup where ApplicationCode = '{applicationCode}' and CreatedBy = '{id}'";

            raw = _context.Set<ApplicantRecord>().FromSqlRaw(str);

            return raw;
        }

        public int GetCount()
        {
            var count = _context.ApplicantRecords.Count();
            return count;
        }

        public IQueryable<ApplicantModel> GetAllApplicantRecordForDT(int skip, int take, string searchText = "")
        {
            IQueryable<ApplicantModel> raw;
            string str = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                str = $"select ar.Id, ar.ApplicationCode, ar.ScheduleDate, ar.DateCreated, ar.FirstName, IsNull(ar.MiddleName, '') MiddleName, ar.LastName, IsNull(ar.Suffix, '') Suffix, IsNull(ar.ContactNumber, '') ContactNumber, IsNull(ar.NameOfRepresentative, '') NameOfRepresentative, IsNull(ar.RepresentativeContactNumber, '') RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, ad.* " +
                    "into #tmp " +
                 "from ApplicantRecords ar " +
                 "inner join AspNetUsers u on ar.CreatedBy = u.Id  " +
                 "cross apply OpenJson(ar.apostiledata, N'$')  " +
                 "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                 $"where ar.LastName = '{searchText}' or u.Email = '{searchText}' or ar.ApplicationCode = '{searchText}'; " +
                 "select * from #tmp " +
                     "order by ScheduleDate desc " +
                     $"OFFSET     {skip} ROWS " +
                     $"FETCH NEXT {take} ROWS ONLY; " +
                     "drop table #tmp";
            }
            else
            {
                str = $"select ar.Id, ar.ApplicationCode, ar.ScheduleDate, ar.DateCreated, ar.FirstName, IsNull(ar.MiddleName, '') MiddleName, ar.LastName, IsNull(ar.Suffix, '') Suffix, IsNull(ar.ContactNumber, '') ContactNumber, IsNull(ar.NameOfRepresentative, '') NameOfRepresentative, IsNull(ar.RepresentativeContactNumber, '') RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, ad.* " +
                     "from ApplicantRecords ar " +
                     "inner join AspNetUsers u on ar.CreatedBy = u.Id  " +
                     "cross apply OpenJson(ar.apostiledata, N'$')  " +
                     "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                     "order by ar.ScheduleDate desc " +
                     $"OFFSET {skip} ROWS " +
                     $"FETCH NEXT {take} ROWS ONLY; ";
            }

            raw = _context.Set<ApplicantModel>().FromSqlRaw(str);


            return raw;
        }

        public int GetAllApplicantRecordCountForDT(string searchText = "")
        {
            IEnumerable<ApplicantModelCount> raw;
            string str = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                str = $"select count(1) [Count] from (" +
                    "select ar.ApplicationCode " +
                    "from ApplicantRecords ar inner join AspNetUsers u on ar.CreatedBy = u.Id  cross apply OpenJson(ar.apostiledata, N'$')  WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                    $"where ar.LastName = '{searchText}' or u.Email = '{searchText}' or ar.ApplicationCode = '{searchText}' " +
                    "group by applicationcode) a ";
            }
            else
            {
                str = $"select count(1) [Count] from (" +
                    "select ar.ApplicationCode " +
                    "from ApplicantRecords ar inner join AspNetUsers u on ar.CreatedBy = u.Id  cross apply OpenJson(ar.apostiledata, N'$')  WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                    "group by applicationcode) a ";
            }

            raw = _context.Set<ApplicantModelCount>().FromSqlRaw(str);

            return raw.FirstOrDefault().Count;
        }


        public IQueryable<AdminApplicantRecordViewModel> GetAllApplicantRecord(string sortOrder, string searchString)
        {
            //var result = _context.ApplicantRecords.Select(a => a);
            var applicants = from ar in _context.ApplicantRecords
                             join u in _context.Users on ar.CreatedBy.ToString() equals u.Id
                             select new AdminApplicantRecordViewModel
                             {
                                 Id = ar.Id,
                                 ApplicationCode = ar.ApplicationCode,
                                 ScheduleDate = ar.ScheduleDate,
                                 DateCreated = ar.DateCreated,
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
            var applicants = from ar in _context.ApplicantRecords
                             where ar.BranchId == branchId
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

        public Branch GetBranch(string branchName)
        {
            var branch = _context.Branches.Where(a => a.BranchName == branchName).Include(a => a.ScheduleCapacities).FirstOrDefault();
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
                ScheduleCapacities = branch.ScheduleCapacities.Select(sc => new ScheduleCapacity
                {
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

        public ApplicationUser GetAccount(string userId)
        {
            var user = _context.Users.Where(a => a.Id == userId).AsNoTracking().FirstOrDefault();
            return user;
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
                holidays = _context.Holidays.Where(a => a.BranchId.Equals(Convert.ToInt64(searchString)) && a.Date.Year == DateTime.Now.Year && a.Type != "UnAvailableDay");
            }
            else
            {
                holidays = _context.Holidays.Where(a => a.BranchId == 0 && a.Date.Year == DateTime.Now.Year && a.Type != "UnAvailableDay");
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

        public IQueryable<Holiday> GetExceptionDay(string sortOrder, string searchString)
        {
            IQueryable<Holiday> holidays;
            if (!String.IsNullOrEmpty(searchString))
            {
                holidays = _context.Holidays.Where(a => a.BranchId.Equals(Convert.ToInt64(searchString)) && a.Date.Year >= DateTime.Now.Year && a.Type == "UnAvailableDay");
            }
            else
            {
                holidays = _context.Holidays.Where(a => a.BranchId == 0 && a.Date.Year >= DateTime.Now.Year && a.Type == "UnAvailableDay");
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

        public bool UnAvailableDay(long branchId, DateTime start, DateTime end)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var holiday = new Holiday
                {
                    Title = "UnAvailableDay",
                    BranchId = branchId,
                    Date = date,
                    Type = "UnAvailableDay"
                };
                _context.Holidays.Add(holiday);
                _context.SaveChanges();
            }
            //var days = Enumerable.Range(0, 1 + end.Subtract(start).Days)
            //  .Select(offset => start.AddDays(offset))
            //  .ToArray();

            return true;
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
            var activityLogs = from s in _context.ActivityLogs
                               where s.UserId != "1"
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

        public IEnumerable<Notice> GetNotice()
        {
            var notice = _context.Notices.AsEnumerable();
            return notice;
        }
        public Notice GetNotice(long id)
        {
            var notice = _context.Notices.Where(a => a.Id == id).FirstOrDefault();
            return notice;
        }

        public Notice UpdateNotice(long id, Notice notice) //Id 1 for announcement, 2 for Declaration, 3 for Terms and conditions
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

            var raw = from ar in _context.ApplicantRecords
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
                Email = ar.Email,
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

        public IEnumerable<ExportTemplate> ExportUnAttendanceToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);
            IEnumerable<AttendanceModel> raw;
            if (branchId == default)
            {
                raw = _context.Set<AttendanceModel>().FromSqlRaw($"select ar.ApplicationCode, ar.ScheduleDate, ar.FirstName, ar.MiddleName, ar.LastName, ar.Suffix, ar.ContactNumber, " +
                "ar.NameOfRepresentative, ar.RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, " +
                "IsNull(a.attendance, 'No') attendance, ad.* " +
                "from ApplicantRecords ar " +
                "inner join AspNetUsers u on ar.CreatedBy = u.Id " +
                "left join tbl_attendance a on ar.ApplicationCode = a.applicationcode " +
                "cross apply OpenJson(ar.apostiledata, N'$') " +
                "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                "where ar.ScheduleDate between {0} and {1} and a.attendance is null", dateFrom, dateTo).AsEnumerable();
            }
            else
            {
                raw = _context.Set<AttendanceModel>().FromSqlRaw($"select ar.ApplicationCode, ar.ScheduleDate, ar.FirstName, ar.MiddleName, ar.LastName, ar.Suffix, ar.ContactNumber, " +
                "ar.NameOfRepresentative, ar.RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, " +
                "IsNull(a.attendance, 'No') attendance, ad.* " +
                "from ApplicantRecords ar " +
                "inner join AspNetUsers u on ar.CreatedBy = u.Id " +
                "left join tbl_attendance a on ar.ApplicationCode = a.applicationcode " +
                "cross apply OpenJson(ar.apostiledata, N'$') " +
                "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                "where ar.ScheduleDate between {0} and {1} and ar.BranchId={2} and a.attendance is null", dateFrom, dateTo, branchId).AsEnumerable();
            }

            var applicants = raw.GroupBy(a => new
            {
                a.ApplicationCode,
                a.ScheduleDate,
                a.FirstName,
                a.MiddleName,
                a.LastName,
                a.Suffix,
                a.ContactNumber,
                a.NameOfRepresentative,
                a.RepresentativeContactNumber,
                a.ProcessingSite,
                a.Email,
                a.CountryDestination,
                a.Attendance
            }).Select(gcs => new ExportTemplate
            {
                AppointmentCode = gcs.Key.ApplicationCode,
                ScheduleDate = gcs.Key.ScheduleDate,
                FirstName = gcs.Key.FirstName,
                MiddleName = gcs.Key.MiddleName,
                LastName = gcs.Key.LastName,
                Suffix = gcs.Key.Suffix,
                ContactNumber = gcs.Key.ContactNumber,
                NameOfRepresentative = gcs.Key.NameOfRepresentative,
                RepresentativeContactNumber = gcs.Key.RepresentativeContactNumber,
                ConsularOffice = gcs.Key.ProcessingSite,
                Documents = string.Join("<br>", gcs.Select(a => a.DocumentName)),
                Quantity = string.Join("<br>", gcs.Select(d => d.Quantity)),
                Transaction = string.Join("<br>", gcs.Select(d => d.Transaction)),
                TotalDocuments = gcs.Select(d => d.Quantity).Sum(),
                Email = gcs.Key.Email,
                CountryDestination = gcs.Key.CountryDestination,
                Attendance = gcs.Key.Attendance
            }).OrderBy(a => a.ScheduleDate).ToList();

            return applicants;
        }

        public IEnumerable<ExportTemplate> ExportCancelledAppointmentToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1).AddSeconds(-1);

            IEnumerable<CancelAppointmentModel> raw;
            if (branchId == default)
            {
                raw = _context.Set<CancelAppointmentModel>().FromSqlRaw($"select ar.ApplicationCode, ar.ScheduleDate, ar.FirstName, ar.MiddleName, ar.LastName, ar.Suffix, ar.ContactNumber, " +
                "ar.NameOfRepresentative, ar.RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, ad.* " +
                "from ApplicantRecords_backup ar " +
                "inner join AspNetUsers u on ar.CreatedBy = u.Id " +
                "cross apply OpenJson(ar.apostiledata, N'$') " +
                "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                "where ar.ScheduleDate between {0} and {1}", dateFrom, dateTo).AsEnumerable();
            }
            else
            {
                raw = _context.Set<CancelAppointmentModel>().FromSqlRaw($"select ar.ApplicationCode, ar.ScheduleDate, ar.FirstName, ar.MiddleName, ar.LastName, ar.Suffix, ar.ContactNumber, " +
                "ar.NameOfRepresentative, ar.RepresentativeContactNumber, ar.ProcessingSite, u.Email, ar.CountryDestination, ad.* " +
                "from ApplicantRecords_backup ar " +
                "inner join AspNetUsers u on ar.CreatedBy = u.Id " +
                "cross apply OpenJson(ar.apostiledata, N'$') " +
                "WITH (DocumentName VARCHAR(200) N'$.Name', Quantity Int N'$.Quantity', [Transaction] VARCHAR(200) N'$.Transaction') AS ad " +
                "where ar.ScheduleDate between {0} and {1} and ar.BranchId={2}", dateFrom, dateTo, branchId).AsEnumerable();
            }


            var applicants = raw.GroupBy(a => new
            {
                a.ApplicationCode,
                a.ScheduleDate,
                a.FirstName,
                a.MiddleName,
                a.LastName,
                a.Suffix,
                a.ContactNumber,
                a.NameOfRepresentative,
                a.RepresentativeContactNumber,
                a.ProcessingSite,
                a.Email,
                a.CountryDestination
            }).Select(gcs => new ExportTemplate
            {
                AppointmentCode = gcs.Key.ApplicationCode,
                ScheduleDate = gcs.Key.ScheduleDate,
                FirstName = gcs.Key.FirstName,
                MiddleName = gcs.Key.MiddleName,
                LastName = gcs.Key.LastName,
                Suffix = gcs.Key.Suffix,
                ContactNumber = gcs.Key.ContactNumber,
                NameOfRepresentative = gcs.Key.NameOfRepresentative,
                RepresentativeContactNumber = gcs.Key.RepresentativeContactNumber,
                ConsularOffice = gcs.Key.ProcessingSite,
                Documents = string.Join("<br>", gcs.Select(a => a.DocumentName)),
                Quantity = string.Join("<br>", gcs.Select(d => d.Quantity)),
                Transaction = string.Join("<br>", gcs.Select(d => d.Transaction)),
                TotalDocuments = gcs.Select(d => d.Quantity).Sum(),
                Email = gcs.Key.Email,
                CountryDestination = gcs.Key.CountryDestination
            })
            .OrderBy(a => a.ScheduleDate)
            .ToList();

            return applicants;
        }

        public ApplicantRecord ResendApplication(long id)
        {
            var applicant = _context.ApplicantRecords.Where(a => a.Id == id).AsNoTracking().FirstOrDefault();
            return applicant;
        }

        public byte[] GenerateQRCode(string plainText)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return BitmapToBytesCode(qrCodeImage);
        }

        private static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public async Task<bool> CancelApplication(string applicationCode)
        {
            var commandText = "INSERT INTO ApplicantRecords_backup (Title, FirstName,MiddleName, LastName, Suffix, [Address], Nationality, ContactNumber, " +
                    "CompanyName, CountryDestination, NameOfRepresentative, RepresentativeContactNumber, ApostileData, ProcessingSite, " +
                    "ProcessingSiteAddress, ScheduleDate, ApplicationCode, Fees, DateCreated, CreatedBy, [Type], DateOfBirth, BranchId, TotalApostile, deleted_time, deleted_by) " +
                    "SELECT Title, FirstName,MiddleName, LastName, Suffix, [Address], Nationality, ContactNumber, " +
                    "CompanyName, CountryDestination, NameOfRepresentative, RepresentativeContactNumber, ApostileData, ProcessingSite, " +
                    $"ProcessingSiteAddress, ScheduleDate, ApplicationCode, Fees, DateCreated, CreatedBy, [Type], DateOfBirth, BranchId, TotalApostile, GETDATE(), 'test' FROM ApplicantRecords WHERE ApplicationCode in ({applicationCode}); ";

            _context.Database.ExecuteSqlRaw(commandText);

            string[] codes = applicationCode.Split(',');

            foreach (var code in codes)
            {
                var appCode = code.Replace("'", "");
                var applicant = _context.ApplicantRecords.Where(a => a.ApplicationCode == appCode).FirstOrDefault();
                var email = _context.Users.Where(a => a.Id == applicant.CreatedBy.ToString()).FirstOrDefault().Email;
                await _emailService.SendAsync(email, "Appointment Cancellation", HtmlTemplate(email, applicant), true);
            };


            commandText = $"DELETE ApplicantRecords WHERE ApplicationCode in ({applicationCode}); ";
            _context.Database.ExecuteSqlRaw(commandText);

            return true;
        }

        public string HtmlTemplate(string email, ApplicantRecord applicant)
        {
            using (StreamReader SourceReader = File.OpenText(_env.WebRootFileProvider.GetFileInfo("cancelappointment.html")?.PhysicalPath))
            {
                var str = SourceReader.ReadToEnd();
                str = str.Replace("@@EMAIL", email);
                str = str.Replace("@@DATENOW", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                str = str.Replace("@@APPOINTMENTCODE", applicant.ApplicationCode);
                str = str.Replace("@@APPOINTMENTDATE", applicant.ScheduleDate.ToString("MM/dd/yyyy"));
                str = str.Replace("@@APPOINTMENTTIME", applicant.ScheduleDate.ToString("hh:mm tt"));
                str = str.Replace("@@PROCESSINGSITE", applicant.ProcessingSite);
                str = str.Replace("@@PROCESSINGADDRESS", applicant.ProcessingSiteAddress);
                str = str.Replace("@@DOCUMENTOWNER", $"{applicant.FirstName} {applicant.LastName}");
                str = str.Replace("@@COUNTRYOFDESTINATION", applicant.CountryDestination);
                return str;
            }

        }


        public List<DocumentStatistic> GetDocumentCountByDay(long branchId, DateTime startDate)
        {
            var raw = _context.Set<DocumentStatistic>().FromSqlRaw($"exec GetAvailableTimeByDay {branchId}, '{startDate.ToString("yyyy-MM-dd")}'")
                .ToList();

            return raw;
        }


        public bool AddActivityLog(ActivityLog activityLog)
        {
            activityLog.CreatedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(activityLog.IpAddress) || activityLog.IpAddress != "::1")
            {
                var getUserCountryByIp = GetUserCountryByIp(activityLog.IpAddress);
                activityLog.City = getUserCountryByIp.city;
                activityLog.Region = getUserCountryByIp.region;
                activityLog.Country = getUserCountryByIp.country;
            }
            _context.ActivityLogs.Add(activityLog);
            _context.SaveChanges();
            return true;
        }

        public IpInfo GetUserCountryByIp(string ip)
        {
            //string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
            IpInfo ipInfo = new IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
                //RegionInfo myRI1 = new RegionInfo(ipInfo.Country);
                //ipInfo.Country = myRI1.EnglishName;
            }
            catch (Exception)
            {
                ipInfo.country = null;
            }

            return ipInfo;
        }


        public async Task<bool> ResendCancellationEmail(ApplicantRecord record)
        {
            var email = _context.Users.Where(a => a.Id == record.CreatedBy.ToString()).FirstOrDefault().Email;
            await _emailService.SendAsync(email, "Appointment Cancellation", HtmlTemplate(email, record), true);

            return true;
        }

    }
}



//improve calendar date range when deleteing unavailable