﻿using DFACore.Data;
using DFACore.Models;
using DFACore.Models.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class ApplicantRecordRepository : IApplicantRecordRepository
    {
        private ApplicationDbContext _context;
        public ApplicantRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(ApplicantRecord applicantRecord)
        {
            applicantRecord.DateCreated = DateTime.UtcNow;

            _context.ApplicantRecords.Add(applicantRecord);
            _context.SaveChanges();
            return true;
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

        public bool AddRange(IEnumerable<ApplicantRecord> applicantRecords)
        {
            try
            {
                //var newObject = applicantRecords.Select(model => new ApplicantRecord {
                //    FirstName = model.FirstName,
                //    MiddleName = model.MiddleName,
                //    LastName = model.LastName,
                //    Suffix = model.Suffix,
                //    DateOfBirth = model.DateOfBirth,
                //    ContactNumber = model.ContactNumber,
                //    CountryDestination = model.CountryDestination?.ToUpper(),
                //    ApostileData = model.ApostileData,
                //    ProcessingSite = model.ProcessingSite,
                //    ProcessingSiteAddress = model.ProcessingSiteAddress,
                //    ScheduleDate = model.ScheduleDate, 
                //    ApplicationCode = model.ApplicationCode,
                //    CreatedBy = model.CreatedBy,
                //    Fees = model.Fees,
                //    Type = model.Type,
                //    DateCreated = DateTime.UtcNow,
                //});
                //var act = new ActivityLog
                //{
                //    UserId = 1,
                //    IpAddress = applicantRecords.FirstOrDefault().CreatedBy.ToString(),
                //    OS = JsonConvert.SerializeObject(newObject)
                //};

                //AddActivityLog(act);

                _context.ApplicantRecords.AddRange(applicantRecords);
                _context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            //var scheduleTime = new List<DateTime>()
            //{ 
            //    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)
            //};

            //foreach (var applicantRecord in applicantRecords)
            //{
            //    if (applicantRecord.ScheduleDate.TimeOfDay == applicantRecord.ScheduleDate.TimeOfDay)
            //    {

            //    }
            //    ValidateScheduleDate(applicantRecord.ScheduleDate);
            //}
            //applicantRecord.DateCreated = DateTime.UtcNow;


        }

        public bool Delete(long id)
        {
            throw new NotImplementedException();
        }

        public ApplicantRecord Get(long id)
        {
            var applicant = _context.ApplicantRecords.Where(a => a.Id == id).FirstOrDefault();
            return applicant;
        }

        public List<ApplicantRecord> GetByCode(string code)
        {
            var applicant = _context.ApplicantRecords.Where(a => a.ApplicationCode.Contains(code)).ToList();
            return applicant;
        }

        public IEnumerable<ApplicantRecord> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool Update(ApplicantRecord applicantRecord)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public bool ValidateScheduleDate(DateTime date)
        {
            var totalCount = 0;
            var applicantRecords = _context.ApplicantRecords.Where(a => a.ScheduleDate == date).ToList();
            foreach (var applicantRecord in applicantRecords)
            {
                var apostileData = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                totalCount += apostileData.Count;
            }

            int type = 0;

            TimeSpan timeSpan = date.TimeOfDay;
            TimeSpan TodayTime2 = new TimeSpan(8, 0, 0);

            if (timeSpan == new TimeSpan(7, 0, 0))
                type = 1;
            else if (timeSpan == new TimeSpan(8, 0, 0))
                type = 2;
            else if (timeSpan == new TimeSpan(9, 0, 0))
                type = 3;
            else if (timeSpan == new TimeSpan(10, 0, 0))
                type = 4;
            else if (timeSpan == new TimeSpan(11, 0, 0))
                type = 5;
            else if (timeSpan == new TimeSpan(12, 0, 0))
                type = 6;
            else if (timeSpan == new TimeSpan(13, 0, 0))
                type = 7;
            else if (timeSpan == new TimeSpan(14, 0, 0))
                type = 8;
            else if (timeSpan == new TimeSpan(15, 0, 0))
                type = 9;

            //var totalCount = _context.ApplicantRecords.Select(a => a.ScheduleDate).Count() + applicationCount;
            var scheduleCapacity = _context.ScheduleCapacities.Where(c => c.Type.Equals(type)).FirstOrDefault();

            if (scheduleCapacity != null)
            {
                if (totalCount > scheduleCapacity.Capacity)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public bool ValidateScheduleDate(DateTime date, int applicationCount, long branchId)
        {
            if (applicationCount == 0)
                applicationCount = 1;

            if (date < DateTime.UtcNow.ToLocalTime())
            {
                return false;
            }

            var totalCount = applicationCount;
            var applicantRecords = _context.ApplicantRecords.Where(a => a.BranchId == branchId && a.ScheduleDate == date).AsEnumerable();
            if (applicantRecords.Count() != 0)
            {
                foreach (var applicantRecord in applicantRecords)
                {
                    var apostileDataList = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                    foreach (var apostileData in apostileDataList)
                    {
                        totalCount += apostileData.Quantity;
                    }
                }
            }


            //int type = 0;

            //TimeSpan timeSpan = date.TimeOfDay;
            //TimeSpan TodayTime2 = new TimeSpan(8, 0, 0);

            //if (timeSpan == new TimeSpan(7, 0, 0))
            //    type = 1;
            //else if (timeSpan == new TimeSpan(8, 0, 0))
            //    type = 2;
            //else if (timeSpan == new TimeSpan(9, 0, 0))
            //    type = 3;
            //else if (timeSpan == new TimeSpan(10, 0, 0))
            //    type = 4;
            //else if (timeSpan == new TimeSpan(11, 0, 0))
            //    type = 5;
            //else if (timeSpan == new TimeSpan(12, 0, 0))
            //    type = 6;
            //else if (timeSpan == new TimeSpan(13, 0, 0))
            //    type = 7;
            //else if (timeSpan == new TimeSpan(14, 0, 0))
            //    type = 8;
            //else if (timeSpan == new TimeSpan(15, 0, 0))
            //    type = 9;

            //var totalCount = _context.ApplicantRecords.Select(a => a.ScheduleDate).Count() + applicationCount;
            var a = $"{date.ToString("hh tt")}";
            var scheduleCapacity = _context.ScheduleCapacities.Where(c => c.BranchId == branchId && c.Name == $"{date.ToString("hh tt")}").FirstOrDefault();

            if (scheduleCapacity != null)
            {
                if (totalCount > scheduleCapacity.Capacity)
                    return false;
                else
                    return true;
            }
            else
                return false;

        }

        public List<AvailableDAtes> GenerateListOfDates(DateTime start, long branchId, int userType = 0)
        {
            var range = _context.Branches.Where(a => a.Id == branchId).FirstOrDefault();
            var now = DateTime.UtcNow.ToLocalTime();
            //var toCompare = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);
            //var concreteDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            //if (start >= toCompare)
            //    start = start.AddDays(1);

            DateTime end;

            //var end = new DateTime(2021, 06, 01); //start.AddYears(1); //.AddDays(30);

            if (range == null)
            {
                var toCompare = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
                if (start >= toCompare)
                    start = start.AddDays(1);
                end = start.AddMonths(3);
            }
            else
            {
                if (range.StartTime != default)
                {
                    if (range.StartTime < now)
                    {
                        start = now;
                    }
                    else
                    {
                        start = range.StartTime;
                    }
                }
                else
                {
                    start = now;
                }

                //var end = new DateTime(2021, 05, 01); //start.AddYears(1); //.AddDays(30);
                if (range.EndTime != default)
                {
                    end = range.EndTime;
                }
                else
                {
                    end = start.AddMonths(3);
                }
            }

            var dates = new List<AvailableDAtes>();
            var unAvailable = GetUnAvailableDates(branchId, userType);

            //this should be optimize by date between now and end of date you want
            //var holidays = _context.Holidays.Where(h => h.Date.Year == now.Year && (h.BranchId == 0 || h.BranchId == branchId))
            var holidays = _context.Holidays.Where(h => h.Date >= now.Date && (h.BranchId == 0 || h.BranchId == branchId))
                .Select(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day)).ToList();

            unAvailable.AddRange(holidays);

            start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);

            for (var dt = start; dt <= end; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
                {
                    AvailableDAtes av;
                    var isExist = unAvailable.Any(d => d.Date == dt.Date);
                    if (isExist)
                    {
                        av = new AvailableDAtes
                        {
                            title = "Not Available",
                            start = dt.ToString("yyyy-MM-dd"),
                            color = "#ff9f89"
                        };
                    }
                    else
                    {
                        av = new AvailableDAtes
                        {
                            title = "Available",
                            start = dt.ToString("yyyy-MM-dd"),
                            //color = "#257e4a"
                        };
                    }

                    dates.Add(av);
                }
            }

            //var result = dates.Select(a => new AvailableDAtes()
            //{

            //});
            //dates.RemoveAll(x => unAvailable.Contains(DateTime.ParseExact(x.start, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)));

            return dates;
        }

        public List<DateTime> GetUnAvailableDates(long branchId, int userType = 0)
        {
            int type = 0;
            if (userType == 2) type = 1;
            var limitPerDay = _context.ScheduleCapacities.Where(s => s.BranchId == branchId && s.Type == type).Sum(s => s.Capacity);
            //string sql = "Select * from " +
            //    "(Select " +
            //    "count(distinct(Id)) as ApplicantCount, " +
            //    "count(Id) as DocuTypes, " +
            //    "CAST(ScheduleDate as Date) as ScheduleDate, " +
            //    "sum(dataCount.Quantity) as DocuCount " +
            //    "from " +
            //    "(select Id, ScheduleDate, ApostileData " +
            //    "from ApplicantRecords " +
            //    "where BranchId={0} and CAST(ScheduleDate as Date) >= CAST(GETDATE() as DATE)) ApplicantRecords " +
            //    "CROSS APPLY OPENJSON (ApostileData) " +
            //    "WITH " +
            //    "(Quantity int) " +
            //    "AS dataCount " +
            //    "group by " +
            //    "CAST(ScheduleDate as Date)) a " +
            //    "where a.DocuCount >= {1}";
            var raw = _context.Set<UnavailableDate>().FromSqlRaw("EXEC sp_GetUnavailableDates {0}, {1}", branchId, limitPerDay, userType).AsEnumerable();

            var result = raw.Select(a => a.ScheduleDate).ToList();


            return result;
        }

        public List<City> GetCity()
        {
            var cities = new List<City>{
                new City { municipality = "Abra ", city = "Bangued" },
                new City { municipality = "Abra ", city = "Boliney" },
                new City { municipality = "Abra ", city = "Bucay" },
                new City { municipality = "Abra ", city = "Bucloc" },
                new City { municipality = "Abra ", city = "Daguioman" },
                new City { municipality = "Abra ", city = "Danglas" },
                new City { municipality = "Abra ", city = "Dolores" },
                new City { municipality = "Abra ", city = "La Paz" },
                new City { municipality = "Abra ", city = "Lacub" },
                new City { municipality = "Abra ", city = "Lagangilang" },
                new City { municipality = "Abra ", city = "Lagayan" },
                new City { municipality = "Abra ", city = "Langiden" },
                new City { municipality = "Abra ", city = "Licuan-Baay" },
                new City { municipality = "Abra ", city = "Luba" },
                new City { municipality = "Abra ", city = "Malibcong" },
                new City { municipality = "Abra ", city = "Manabo" },
                new City { municipality = "Abra ", city = "Peñarrubia" },
                new City { municipality = "Abra ", city = "Pidigan" },
                new City { municipality = "Abra ", city = "Pilar" },
                new City { municipality = "Abra ", city = "Sallapadan" },
                new City { municipality = "Abra ", city = "San Isidro" },
                new City { municipality = "Abra ", city = "San Juan" },
                new City { municipality = "Abra ", city = "San Quintin" },
                new City { municipality = "Abra ", city = "Tayum" },
                new City { municipality = "Abra ", city = "Tineg" },
                new City { municipality = "Abra ", city = "Tubo" },
                new City { municipality = "Abra ", city = "Villaviciosa" },
                new City { municipality = "Agusan del Norte ", city = "Buenavista" },
                new City { municipality = "Agusan del Norte ", city = "Butuan" },
                new City { municipality = "Agusan del Norte ", city = "Cabadbaran" },
                new City { municipality = "Agusan del Norte ", city = "Carmen" },
                new City { municipality = "Agusan del Norte ", city = "Jabonga" },
                new City { municipality = "Agusan del Norte ", city = "Kitcharao" },
                new City { municipality = "Agusan del Norte ", city = "Las Nieves" },
                new City { municipality = "Agusan del Norte ", city = "Magallanes" },
                new City { municipality = "Agusan del Norte ", city = "Nasipit" },
                new City { municipality = "Agusan del Norte ", city = "Remedios T. Romualdez" },
                new City { municipality = "Agusan del Norte ", city = "Santiago" },
                new City { municipality = "Agusan del Norte ", city = "Tubay" },
                new City { municipality = "Agusan del Sur", city = "Bayugan" },
                new City { municipality = "Agusan del Sur", city = "Bunawan" },
                new City { municipality = "Agusan del Sur", city = "Esperanza" },
                new City { municipality = "Agusan del Sur", city = "La Paz" },
                new City { municipality = "Agusan del Sur", city = "Loreto" },
                new City { municipality = "Agusan del Sur", city = "Prosperidad" },
                new City { municipality = "Agusan del Sur", city = "Rosario" },
                new City { municipality = "Agusan del Sur", city = "San Francisco" },
                new City { municipality = "Agusan del Sur", city = "San Luis" },
                new City { municipality = "Agusan del Sur", city = "Sta. Josefa" },
                new City { municipality = "Agusan del Sur", city = "Sibagat" },
                new City { municipality = "Agusan del Sur", city = "Talacogon" },
                new City { municipality = "Agusan del Sur", city = "Trento" },
                new City { municipality = "Agusan del Sur", city = "Veruela" },
                new City { municipality = "Aklan", city = "Altavas" },
                new City { municipality = "Aklan", city = "Balete" },
                new City { municipality = "Aklan", city = "Banga" },
                new City { municipality = "Aklan", city = "Batan" },
                new City { municipality = "Aklan", city = "Buruanga" },
                new City { municipality = "Aklan", city = "Ibajay" },
                new City { municipality = "Aklan", city = "Kalibo" },
                new City { municipality = "Aklan", city = "Lezo" },
                new City { municipality = "Aklan", city = "Libacao" },
                new City { municipality = "Aklan", city = "Madalag" },
                new City { municipality = "Aklan", city = "Makato" },
                new City { municipality = "Aklan", city = "Malay" },
                new City { municipality = "Aklan", city = "Malinao" },
                new City { municipality = "Aklan", city = "Nabas" },
                new City { municipality = "Aklan", city = "New Washington" },
                new City { municipality = "Aklan", city = "Numancia" },
                new City { municipality = "Aklan", city = "Tangalan" },
                new City { municipality = "Albay", city = "Bacacay" },
                new City { municipality = "Albay", city = "Camalig" },
                new City { municipality = "Albay", city = "Daraga" },
                new City { municipality = "Albay", city = "Guinobatan" },
                new City { municipality = "Albay", city = "Jovellar" },
                new City { municipality = "Albay", city = "Legazpi" },
                new City { municipality = "Albay", city = "Libon" },
                new City { municipality = "Albay", city = "Ligao" },
                new City { municipality = "Albay", city = "Malilipot" },
                new City { municipality = "Albay", city = "Malinao" },
                new City { municipality = "Albay", city = "Manito" },
                new City { municipality = "Albay", city = "Oas" },
                new City { municipality = "Albay", city = "Pio Duran" },
                new City { municipality = "Albay", city = "Polangui" },
                new City { municipality = "Albay", city = "Rapu-Rapu" },
                new City { municipality = "Albay", city = "Sto. Domingo" },
                new City { municipality = "Albay", city = "Tabaco" },
                new City { municipality = "Albay", city = "Tiwi" },
                new City { municipality = "Antique", city = "Anini-y" },
                new City { municipality = "Antique", city = "Barbaza" },
                new City { municipality = "Antique", city = "Belison" },
                new City { municipality = "Antique", city = "Bugasong" },
                new City { municipality = "Antique", city = "Caluya" },
                new City { municipality = "Antique", city = "Culasi" },
                new City { municipality = "Antique", city = "Hamtic" },
                new City { municipality = "Antique", city = "Laua-an" },
                new City { municipality = "Antique", city = "Libertad" },
                new City { municipality = "Antique", city = "Pandan" },
                new City { municipality = "Antique", city = "Patnongon" },
                new City { municipality = "Antique", city = "San Jose de Buenavista" },
                new City { municipality = "Antique", city = "San Remigio" },
                new City { municipality = "Antique", city = "Sebaste" },
                new City { municipality = "Antique", city = "Sibalom" },
                new City { municipality = "Antique", city = "Tibiao" },
                new City { municipality = "Antique", city = "Tobias Fornier" },
                new City { municipality = "Antique", city = "Valderrama" },
                new City { municipality = "Apayao", city = "Calanasan" },
                new City { municipality = "Apayao", city = "Conner" },
                new City { municipality = "Apayao", city = "Flora" },
                new City { municipality = "Apayao", city = "Kabugao" },
                new City { municipality = "Apayao", city = "Luna" },
                new City { municipality = "Apayao", city = "Pudtol" },
                new City { municipality = "Apayao", city = "Sta. Marcela" },
                new City { municipality = "Aurora", city = "Baler" },
                new City { municipality = "Aurora", city = "Casiguran" },
                new City { municipality = "Aurora", city = "Dilasag" },
                new City { municipality = "Aurora", city = "Dinalungan" },
                new City { municipality = "Aurora", city = "Dingalan" },
                new City { municipality = "Aurora", city = "Dipaculao" },
                new City { municipality = "Aurora", city = "Maria Aurora" },
                new City { municipality = "Aurora", city = "San Luis" },
                new City { municipality = "Basilan", city = "Akbar" },
                new City { municipality = "Basilan", city = "Al-Barka" },
                new City { municipality = "Basilan", city = "Hadji Mohammad Ajul" },
                new City { municipality = "Basilan", city = "Hadji Muhtamad" },
                new City { municipality = "Basilan", city = "Isabela City" },
                new City { municipality = "Basilan", city = "Lamitan" },
                new City { municipality = "Basilan", city = "Lantawan" },
                new City { municipality = "Basilan", city = "Maluso" },
                new City { municipality = "Basilan", city = "Sumisip" },
                new City { municipality = "Basilan", city = "Tabuan-Lasa" },
                new City { municipality = "Basilan", city = "Tipo-Tipo" },
                new City { municipality = "Basilan", city = "Tuburan" },
                new City { municipality = "Basilan", city = "Ungkaya Pukan" },
                new City { municipality = "Bataan", city = "Abucay" },
                new City { municipality = "Bataan", city = "Bagac" },
                new City { municipality = "Bataan", city = "Balanga" },
                new City { municipality = "Bataan", city = "Dinalupihan" },
                new City { municipality = "Bataan", city = "Hermosa" },
                new City { municipality = "Bataan", city = "Limay" },
                new City { municipality = "Bataan", city = "Mariveles" },
                new City { municipality = "Bataan", city = "Morong" },
                new City { municipality = "Bataan", city = "Orani" },
                new City { municipality = "Bataan", city = "Orion" },
                new City { municipality = "Bataan", city = "Pilar" },
                new City { municipality = "Bataan", city = "Samal" },
                new City { municipality = "Batanes", city = "Basco" },
                new City { municipality = "Batanes", city = "Itbayat" },
                new City { municipality = "Batanes", city = "Ivana" },
                new City { municipality = "Batanes", city = "Mahatao" },
                new City { municipality = "Batanes", city = "Sabtang" },
                new City { municipality = "Batanes", city = "Uyugan" },
                new City { municipality = "Batangas", city = "Agoncillo" },
                new City { municipality = "Batangas", city = "Alitagtag" },
                new City { municipality = "Batangas", city = "Balayan" },
                new City { municipality = "Batangas", city = "Balete" },
                new City { municipality = "Batangas", city = "Batangas City" },
                new City { municipality = "Batangas", city = "Bauan" },
                new City { municipality = "Batangas", city = "Calaca" },
                new City { municipality = "Batangas", city = "Calatagan" },
                new City { municipality = "Batangas", city = "Cuenca" },
                new City { municipality = "Batangas", city = "Ibaan" },
                new City { municipality = "Batangas", city = "Laurel" },
                new City { municipality = "Batangas", city = "Lemery" },
                new City { municipality = "Batangas", city = "Lian" },
                new City { municipality = "Batangas", city = "Lipa" },
                new City { municipality = "Batangas", city = "Lobo" },
                new City { municipality = "Batangas", city = "Mabini" },
                new City { municipality = "Batangas", city = "Malvar" },
                new City { municipality = "Batangas", city = "Mataas na Kahoy" },
                new City { municipality = "Batangas", city = "Nasugbu" },
                new City { municipality = "Batangas", city = "Padre Garcia" },
                new City { municipality = "Batangas", city = "Rosario" },
                new City { municipality = "Batangas", city = "San Jose" },
                new City { municipality = "Batangas", city = "San Juan" },
                new City { municipality = "Batangas", city = "San Luis" },
                new City { municipality = "Batangas", city = "San Nicolas" },
                new City { municipality = "Batangas", city = "San Pascual" },
                new City { municipality = "Batangas", city = "Sta. Teresita" },
                new City { municipality = "Batangas", city = "Sto. Tomas" },
                new City { municipality = "Batangas", city = "Taal" },
                new City { municipality = "Batangas", city = "Talisay" },
                new City { municipality = "Batangas", city = "Tanauan" },
                new City { municipality = "Batangas", city = "Taysan" },
                new City { municipality = "Batangas", city = "Tingloy" },
                new City { municipality = "Batangas", city = "Tuy" },
                new City { municipality = "Benguet", city = "Atok" },
                new City { municipality = "Benguet", city = "Baguio" },
                new City { municipality = "Benguet", city = "Bakun" },
                new City { municipality = "Benguet", city = "Bokod" },
                new City { municipality = "Benguet", city = "Buguias" },
                new City { municipality = "Benguet", city = "Itogon" },
                new City { municipality = "Benguet", city = "Kabayan" },
                new City { municipality = "Benguet", city = "Kapangan" },
                new City { municipality = "Benguet", city = "Kibungan" },
                new City { municipality = "Benguet", city = "La Trinidad" },
                new City { municipality = "Benguet", city = "Mankayan" },
                new City { municipality = "Benguet", city = "Sablan" },
                new City { municipality = "Benguet", city = "Tuba" },
                new City { municipality = "Benguet", city = "Tublay " },
                new City { municipality = "Biliran", city = "Almeria" },
                new City { municipality = "Biliran", city = "Biliran" },
                new City { municipality = "Biliran", city = "Cabucgayan" },
                new City { municipality = "Biliran", city = "Caibiran" },
                new City { municipality = "Biliran", city = "Culaba" },
                new City { municipality = "Biliran", city = "Kawayan" },
                new City { municipality = "Biliran", city = "Maripipi" },
                new City { municipality = "Biliran", city = "Naval" },
                new City { municipality = "Bohol", city = "Alburquerque" },
                new City { municipality = "Bohol", city = "Alicia" },
                new City { municipality = "Bohol", city = "Anda" },
                new City { municipality = "Bohol", city = "Antequera" },
                new City { municipality = "Bohol", city = "Baclayon" },
                new City { municipality = "Bohol", city = "Balilihan" },
                new City { municipality = "Bohol", city = "Batuan" },
                new City { municipality = "Bohol", city = "Bien Unido" },
                new City { municipality = "Bohol", city = "Bilar" },
                new City { municipality = "Bohol", city = "Buenavista" },
                new City { municipality = "Bohol", city = "Calape" },
                new City { municipality = "Bohol", city = "Candijay" },
                new City { municipality = "Bohol", city = "Carmen" },
                new City { municipality = "Bohol", city = "Catigbian" },
                new City { municipality = "Bohol", city = "Clarin" },
                new City { municipality = "Bohol", city = "Corella" },
                new City { municipality = "Bohol", city = "Cortes" },
                new City { municipality = "Bohol", city = "Dagohoy" },
                new City { municipality = "Bohol", city = "Danao" },
                new City { municipality = "Bohol", city = "Dauis" },
                new City { municipality = "Bohol", city = "Dimiao" },
                new City { municipality = "Bohol", city = "Duero" },
                new City { municipality = "Bohol", city = "Garcia Hernandez" },
                new City { municipality = "Bohol", city = "Getafe" },
                new City { municipality = "Bohol", city = "Guindulman" },
                new City { municipality = "Bohol", city = "Inabanga" },
                new City { municipality = "Bohol", city = "Jagna" },
                new City { municipality = "Bohol", city = "Lila" },
                new City { municipality = "Bohol", city = "Loay" },
                new City { municipality = "Bohol", city = "Loboc" },
                new City { municipality = "Bohol", city = "Loon" },
                new City { municipality = "Bohol", city = "Mabini" },
                new City { municipality = "Bohol", city = "Maribojoc" },
                new City { municipality = "Bohol", city = "Panglao" },
                new City { municipality = "Bohol", city = "Pilar" },
                new City { municipality = "Bohol", city = "Pres. Carlos P. Garcia" },
                new City { municipality = "Bohol", city = "Sagbayan" },
                new City { municipality = "Bohol", city = "San Isidro" },
                new City { municipality = "Bohol", city = "San Miguel" },
                new City { municipality = "Bohol", city = "Sevilla" },
                new City { municipality = "Bohol", city = "Sierra Bullones" },
                new City { municipality = "Bohol", city = "Sikatuna" },
                new City { municipality = "Bohol", city = "Tagbilaran" },
                new City { municipality = "Bohol", city = "Talibon" },
                new City { municipality = "Bohol", city = "Trinidad" },
                new City { municipality = "Bohol", city = "Tubigon" },
                new City { municipality = "Bohol", city = "Ubay" },
                new City { municipality = "Bohol", city = "Valencia" },
                new City { municipality = "Bukidnon", city = "Baungon" },
                new City { municipality = "Bukidnon", city = "Cabanglasan" },
                new City { municipality = "Bukidnon", city = "Damulog" },
                new City { municipality = "Bukidnon", city = "Dangcagan" },
                new City { municipality = "Bukidnon", city = "Don Carlos" },
                new City { municipality = "Bukidnon", city = "Impasugong" },
                new City { municipality = "Bukidnon", city = "Kadingilan" },
                new City { municipality = "Bukidnon", city = "Kalilangan" },
                new City { municipality = "Bukidnon", city = "Kibawe" },
                new City { municipality = "Bukidnon", city = "Kitaotao" },
                new City { municipality = "Bukidnon", city = "Lantapan" },
                new City { municipality = "Bukidnon", city = "Libona" },
                new City { municipality = "Bukidnon", city = "Malaybalay" },
                new City { municipality = "Bukidnon", city = "Malitbog" },
                new City { municipality = "Bukidnon", city = "Manolo Fortich" },
                new City { municipality = "Bukidnon", city = "Maramag" },
                new City { municipality = "Bukidnon", city = "Pangantucan" },
                new City { municipality = "Bukidnon", city = "Quezon" },
                new City { municipality = "Bukidnon", city = "San Fernando" },
                new City { municipality = "Bukidnon", city = "Sumilao" },
                new City { municipality = "Bukidnon", city = "Talakag" },
                new City { municipality = "Bukidnon", city = "Valencia" },
                new City { municipality = "Bulacan", city = "Angat" },
                new City { municipality = "Bulacan", city = "Balagtas" },
                new City { municipality = "Bulacan", city = "Baliuag" },
                new City { municipality = "Bulacan", city = "Bocaue" },
                new City { municipality = "Bulacan", city = "Bulakan" },
                new City { municipality = "Bulacan", city = "Bustos" },
                new City { municipality = "Bulacan", city = "Calumpit" },
                new City { municipality = "Bulacan", city = "Doña Remedios Trinidad" },
                new City { municipality = "Bulacan", city = "Guiguinto" },
                new City { municipality = "Bulacan", city = "Hagonoy" },
                new City { municipality = "Bulacan", city = "Malolos" },
                new City { municipality = "Bulacan", city = "Marilao" },
                new City { municipality = "Bulacan", city = "Meycauayan" },
                new City { municipality = "Bulacan", city = "Norzagaray" },
                new City { municipality = "Bulacan", city = "Obando" },
                new City { municipality = "Bulacan", city = "Pandi" },
                new City { municipality = "Bulacan", city = "Paombong" },
                new City { municipality = "Bulacan", city = "Plaridel" },
                new City { municipality = "Bulacan", city = "Pulilan" },
                new City { municipality = "Bulacan", city = "San Ildefonso" },
                new City { municipality = "Bulacan", city = "San Jose del Monte" },
                new City { municipality = "Bulacan", city = "San Miguel" },
                new City { municipality = "Bulacan", city = "San Rafael" },
                new City { municipality = "Bulacan", city = "Sta. Maria " },
                new City { municipality = "Cagayan", city = "Abulug" },
                new City { municipality = "Cagayan", city = "Alcala" },
                new City { municipality = "Cagayan", city = "Allacapan" },
                new City { municipality = "Cagayan", city = "Amulung" },
                new City { municipality = "Cagayan", city = "Aparri" },
                new City { municipality = "Cagayan", city = "Baggao" },
                new City { municipality = "Cagayan", city = "Ballesteros" },
                new City { municipality = "Cagayan", city = "Buguey" },
                new City { municipality = "Cagayan", city = "Calayan" },
                new City { municipality = "Cagayan", city = "Camalaniugan" },
                new City { municipality = "Cagayan", city = "Claveria" },
                new City { municipality = "Cagayan", city = "Enrile" },
                new City { municipality = "Cagayan", city = "Gattaran" },
                new City { municipality = "Cagayan", city = "Gonzaga" },
                new City { municipality = "Cagayan", city = "Iguig" },
                new City { municipality = "Cagayan", city = "Lal-lo" },
                new City { municipality = "Cagayan", city = "Lasam" },
                new City { municipality = "Cagayan", city = "Pamplona" },
                new City { municipality = "Cagayan", city = "Peñablanca" },
                new City { municipality = "Cagayan", city = "Piat" },
                new City { municipality = "Cagayan", city = "Rizal" },
                new City { municipality = "Cagayan", city = "Sanchez-Mira" },
                new City { municipality = "Cagayan", city = "Sta. Ana" },
                new City { municipality = "Cagayan", city = "Sta. Praxedes" },
                new City { municipality = "Cagayan", city = "Sta. Teresita" },
                new City { municipality = "Cagayan", city = "Sto. Niño" },
                new City { municipality = "Cagayan", city = "Solana" },
                new City { municipality = "Cagayan", city = "Tuao" },
                new City { municipality = "Cagayan", city = "Tuguegarao" },
                new City { municipality = "Camarines Norte", city = "Basud" },
                new City { municipality = "Camarines Norte", city = "Capalonga" },
                new City { municipality = "Camarines Norte", city = "Daet" },
                new City { municipality = "Camarines Norte", city = "Jose Panganiban" },
                new City { municipality = "Camarines Norte", city = "Labo" },
                new City { municipality = "Camarines Norte", city = "Mercedes" },
                new City { municipality = "Camarines Norte", city = "Paracale" },
                new City { municipality = "Camarines Norte", city = "San Lorenzo Ruiz" },
                new City { municipality = "Camarines Norte", city = "San Vicente" },
                new City { municipality = "Camarines Norte", city = "Sta. Elena" },
                new City { municipality = "Camarines Norte", city = "Talisay" },
                new City { municipality = "Camarines Norte", city = "Vinzons" },
                new City { municipality = "Camarines Sur", city = "Baao" },
                new City { municipality = "Camarines Sur", city = "Balatan" },
                new City { municipality = "Camarines Sur", city = "Bato" },
                new City { municipality = "Camarines Sur", city = "Bombon" },
                new City { municipality = "Camarines Sur", city = "Buhi" },
                new City { municipality = "Camarines Sur", city = "Bula" },
                new City { municipality = "Camarines Sur", city = "Cabusao" },
                new City { municipality = "Camarines Sur", city = "Calabanga" },
                new City { municipality = "Camarines Sur", city = "Camaligan" },
                new City { municipality = "Camarines Sur", city = "Canaman" },
                new City { municipality = "Camarines Sur", city = "Caramoan" },
                new City { municipality = "Camarines Sur", city = "Del Gallego" },
                new City { municipality = "Camarines Sur", city = "Gainza" },
                new City { municipality = "Camarines Sur", city = "Garchitorena" },
                new City { municipality = "Camarines Sur", city = "Goa" },
                new City { municipality = "Camarines Sur", city = "Iriga" },
                new City { municipality = "Camarines Sur", city = "Lagonoy" },
                new City { municipality = "Camarines Sur", city = "Libmanan" },
                new City { municipality = "Camarines Sur", city = "Lupi" },
                new City { municipality = "Camarines Sur", city = "Magarao" },
                new City { municipality = "Camarines Sur", city = "Milaor" },
                new City { municipality = "Camarines Sur", city = "Minalabac" },
                new City { municipality = "Camarines Sur", city = "Nabua" },
                new City { municipality = "Camarines Sur", city = "Naga" },
                new City { municipality = "Camarines Sur", city = "Ocampo" },
                new City { municipality = "Camarines Sur", city = "Pamplona" },
                new City { municipality = "Camarines Sur", city = "Pasacao" },
                new City { municipality = "Camarines Sur", city = "Pili" },
                new City { municipality = "Camarines Sur", city = "Presentacion" },
                new City { municipality = "Camarines Sur", city = "Ragay" },
                new City { municipality = "Camarines Sur", city = "Sagñay" },
                new City { municipality = "Camarines Sur", city = "San Fernando" },
                new City { municipality = "Camarines Sur", city = "San Jose" },
                new City { municipality = "Camarines Sur", city = "Sipocot" },
                new City { municipality = "Camarines Sur", city = "Siruma" },
                new City { municipality = "Camarines Sur", city = "Tigaon" },
                new City { municipality = "Camarines Sur", city = "Tinambac" },
                new City { municipality = "Camiguin", city = "Catarman" },
                new City { municipality = "Camiguin", city = "Guinsiliban" },
                new City { municipality = "Camiguin", city = "Mahinog" },
                new City { municipality = "Camiguin", city = "Mambajao" },
                new City { municipality = "Camiguin", city = "Sagay" },
                new City { municipality = "Capiz", city = "Cuartero" },
                new City { municipality = "Capiz", city = "Dao" },
                new City { municipality = "Capiz", city = "Dumalag" },
                new City { municipality = "Capiz", city = "Dumarao" },
                new City { municipality = "Capiz", city = "Ivisan" },
                new City { municipality = "Capiz", city = "Jamindan" },
                new City { municipality = "Capiz", city = "Maayon" },
                new City { municipality = "Capiz", city = "Mambusao" },
                new City { municipality = "Capiz", city = "Panay" },
                new City { municipality = "Capiz", city = "Panitan" },
                new City { municipality = "Capiz", city = "Pilar" },
                new City { municipality = "Capiz", city = "Pontevedra" },
                new City { municipality = "Capiz", city = "Pres. Roxas" },
                new City { municipality = "Capiz", city = "Roxas City" },
                new City { municipality = "Capiz", city = "Sapian" },
                new City { municipality = "Capiz", city = "Sigma" },
                new City { municipality = "Capiz", city = "Tapaz" },
                new City { municipality = "Catanduanes", city = "Bagamanoc" },
                new City { municipality = "Catanduanes", city = "Baras" },
                new City { municipality = "Catanduanes", city = "Bato" },
                new City { municipality = "Catanduanes", city = "Caramoran" },
                new City { municipality = "Catanduanes", city = "Gigmoto" },
                new City { municipality = "Catanduanes", city = "Pandan" },
                new City { municipality = "Catanduanes", city = "Panganiban" },
                new City { municipality = "Catanduanes", city = "San Andres" },
                new City { municipality = "Catanduanes", city = "San Miguel" },
                new City { municipality = "Catanduanes", city = "Viga" },
                new City { municipality = "Catanduanes", city = "Virac" },
                new City { municipality = "Cavite", city = "Alfonso" },
                new City { municipality = "Cavite", city = "Amadeo" },
                new City { municipality = "Cavite", city = "Bacoor" },
                new City { municipality = "Cavite", city = "Carmona" },
                new City { municipality = "Cavite", city = "Cavite City" },
                new City { municipality = "Cavite", city = "Dasmariñas" },
                new City { municipality = "Cavite", city = "Gen. Emilio Aguinaldo" },
                new City { municipality = "Cavite", city = "Gen. Mariano Alvarez" },
                new City { municipality = "Cavite", city = "Gen. Trias" },
                new City { municipality = "Cavite", city = "Imus" },
                new City { municipality = "Cavite", city = "Indang" },
                new City { municipality = "Cavite", city = "Kawit" },
                new City { municipality = "Cavite", city = "Magallanes" },
                new City { municipality = "Cavite", city = "Maragondon" },
                new City { municipality = "Cavite", city = "Mendez" },
                new City { municipality = "Cavite", city = "Naic" },
                new City { municipality = "Cavite", city = "Noveleta" },
                new City { municipality = "Cavite", city = "Rosario" },
                new City { municipality = "Cavite", city = "Silang" },
                new City { municipality = "Cavite", city = "Tagaytay" },
                new City { municipality = "Cavite", city = "Tanza" },
                new City { municipality = "Cavite", city = "Ternate" },
                new City { municipality = "Cavite", city = "Trece Martires" },
                new City { municipality = "Cebu", city = "Alcantara" },
                new City { municipality = "Cebu", city = "Alcoy" },
                new City { municipality = "Cebu", city = "Alegria" },
                new City { municipality = "Cebu", city = "Aloguinsan" },
                new City { municipality = "Cebu", city = "Argao" },
                new City { municipality = "Cebu", city = "Asturias" },
                new City { municipality = "Cebu", city = "Badian" },
                new City { municipality = "Cebu", city = "Balamban" },
                new City { municipality = "Cebu", city = "Bantayan" },
                new City { municipality = "Cebu", city = "Barili" },
                new City { municipality = "Cebu", city = "Bogo" },
                new City { municipality = "Cebu", city = "Boljoon" },
                new City { municipality = "Cebu", city = "Borbon" },
                new City { municipality = "Cebu", city = "Carcar" },
                new City { municipality = "Cebu", city = "Carmen" },
                new City { municipality = "Cebu", city = "Catmon" },
                new City { municipality = "Cebu", city = "Cebu City" },
                new City { municipality = "Cebu", city = "Compostela" },
                new City { municipality = "Cebu", city = "Consolacion" },
                new City { municipality = "Cebu", city = "Cordova" },
                new City { municipality = "Cebu", city = "Daanbantayan" },
                new City { municipality = "Cebu", city = "Dalaguete" },
                new City { municipality = "Cebu", city = "Danao" },
                new City { municipality = "Cebu", city = "Dumanjug" },
                new City { municipality = "Cebu", city = "Ginatilan" },
                new City { municipality = "Cebu", city = "Lapu-Lapu City" },
                new City { municipality = "Cebu", city = "Liloan" },
                new City { municipality = "Cebu", city = "Madridejos" },
                new City { municipality = "Cebu", city = "Malabuyoc" },
                new City { municipality = "Cebu", city = "Mandaue City" },
                new City { municipality = "Cebu", city = "Medellin" },
                new City { municipality = "Cebu", city = "Minglanilla" },
                new City { municipality = "Cebu", city = "Moalboal" },
                new City { municipality = "Cebu", city = "Naga" },
                new City { municipality = "Cebu", city = "Oslob" },
                new City { municipality = "Cebu", city = "Pilar" },
                new City { municipality = "Cebu", city = "Pinamungajan" },
                new City { municipality = "Cebu", city = "Poro" },
                new City { municipality = "Cebu", city = "Ronda" },
                new City { municipality = "Cebu", city = "Samboan" },
                new City { municipality = "Cebu", city = "San Fernando" },
                new City { municipality = "Cebu", city = "San Francisco" },
                new City { municipality = "Cebu", city = "San Remigio" },
                new City { municipality = "Cebu", city = "Sta. Fe" },
                new City { municipality = "Cebu", city = "Santander" },
                new City { municipality = "Cebu", city = "Sibonga" },
                new City { municipality = "Cebu", city = "Sogod" },
                new City { municipality = "Cebu", city = "Tabogon" },
                new City { municipality = "Cebu", city = "Tabuelan" },
                new City { municipality = "Cebu", city = "Talisay" },
                new City { municipality = "Cebu", city = "Toledo" },
                new City { municipality = "Cebu", city = "Tuburan" },
                new City { municipality = "Cebu", city = "Tudela" },
                new City { municipality = "Cotabato", city = "Alamada" },
                new City { municipality = "Cotabato", city = "Aleosan" },
                new City { municipality = "Cotabato", city = "Antipas" },
                new City { municipality = "Cotabato", city = "Arakan" },
                new City { municipality = "Cotabato", city = "Banisilan" },
                new City { municipality = "Cotabato", city = "Carmen" },
                new City { municipality = "Cotabato", city = "Kabacan" },
                new City { municipality = "Cotabato", city = "Kidapawan" },
                new City { municipality = "Cotabato", city = "Libungan" },
                new City { municipality = "Cotabato", city = "M'lang" },
                new City { municipality = "Cotabato", city = "Magpet" },
                new City { municipality = "Cotabato", city = "Makilala" },
                new City { municipality = "Cotabato", city = "Matalam" },
                new City { municipality = "Cotabato", city = "Midsayap" },
                new City { municipality = "Cotabato", city = "Pigcawayan" },
                new City { municipality = "Cotabato", city = "Pikit" },
                new City { municipality = "Cotabato", city = "Pres. Roxas" },
                new City { municipality = "Cotabato", city = "Tulunan" },
                new City { municipality = "Davao de Oro", city = "Compostela" },
                new City { municipality = "Davao de Oro", city = "Laak" },
                new City { municipality = "Davao de Oro", city = "Mabini" },
                new City { municipality = "Davao de Oro", city = "Maco" },
                new City { municipality = "Davao de Oro", city = "Maragusan" },
                new City { municipality = "Davao de Oro", city = "Mawab" },
                new City { municipality = "Davao de Oro", city = "Monkayo" },
                new City { municipality = "Davao de Oro", city = "Montevista" },
                new City { municipality = "Davao de Oro", city = "Nabunturan" },
                new City { municipality = "Davao de Oro", city = "New Bataan" },
                new City { municipality = "Davao de Oro", city = "Pantukan" },
                new City { municipality = "Davao del Norte", city = "Asuncion" },
                new City { municipality = "Davao del Norte", city = "Braulio E. Dujali" },
                new City { municipality = "Davao del Norte", city = "Carmen" },
                new City { municipality = "Davao del Norte", city = "Kapalong" },
                new City { municipality = "Davao del Norte", city = "New Corella" },
                new City { municipality = "Davao del Norte", city = "Panabo" },
                new City { municipality = "Davao del Norte", city = "Samal" },
                new City { municipality = "Davao del Norte", city = "San Isidro" },
                new City { municipality = "Davao del Norte", city = "Santo Tomas" },
                new City { municipality = "Davao del Norte", city = "Tagum" },
                new City { municipality = "Davao del Norte", city = "Talaingod" },
                new City { municipality = "Davao del Sur", city = "Bansalan" },
                new City { municipality = "Davao del Sur", city = "Davao City" },
                new City { municipality = "Davao del Sur", city = "Digos" },
                new City { municipality = "Davao del Sur", city = "Hagonoy" },
                new City { municipality = "Davao del Sur", city = "Kiblawan" },
                new City { municipality = "Davao del Sur", city = "Magsaysay" },
                new City { municipality = "Davao del Sur", city = "Malalag" },
                new City { municipality = "Davao del Sur", city = "Matanao" },
                new City { municipality = "Davao del Sur", city = "Padada" },
                new City { municipality = "Davao del Sur", city = "Sta. Cruz" },
                new City { municipality = "Davao del Sur", city = "Sulop" },
                new City { municipality = "Davao Occidental", city = "Don Marcelino" },
                new City { municipality = "Davao Occidental", city = "Jose Abad Santos" },
                new City { municipality = "Davao Occidental", city = "Malita" },
                new City { municipality = "Davao Occidental", city = "Sta. Maria" },
                new City { municipality = "Davao Occidental", city = "Sarangani" },
                new City { municipality = "Davao Oriental", city = "Baganga" },
                new City { municipality = "Davao Oriental", city = "Banaybanay" },
                new City { municipality = "Davao Oriental", city = "Boston" },
                new City { municipality = "Davao Oriental", city = "Caraga" },
                new City { municipality = "Davao Oriental", city = "Cateel" },
                new City { municipality = "Davao Oriental", city = "Gov. Generoso" },
                new City { municipality = "Davao Oriental", city = "Lupon" },
                new City { municipality = "Davao Oriental", city = "Manay" },
                new City { municipality = "Davao Oriental", city = "Mati" },
                new City { municipality = "Davao Oriental", city = "San Isidro" },
                new City { municipality = "Davao Oriental", city = "Tarragona" },
                new City { municipality = "Dinagat Islands", city = "Basilisa" },
                new City { municipality = "Dinagat Islands", city = "Cagdianao" },
                new City { municipality = "Dinagat Islands", city = "Dinagat" },
                new City { municipality = "Dinagat Islands", city = "Libjo" },
                new City { municipality = "Dinagat Islands", city = "Loreto" },
                new City { municipality = "Dinagat Islands", city = "San Jose" },
                new City { municipality = "Dinagat Islands", city = "Tubajon" },
                new City { municipality = "Eastern Samar", city = "Arteche" },
                new City { municipality = "Eastern Samar", city = "Balangiga" },
                new City { municipality = "Eastern Samar", city = "Balangkayan" },
                new City { municipality = "Eastern Samar", city = "Borongan" },
                new City { municipality = "Eastern Samar", city = "Can-avid" },
                new City { municipality = "Eastern Samar", city = "Dolores" },
                new City { municipality = "Eastern Samar", city = "Gen. MacArthur" },
                new City { municipality = "Eastern Samar", city = "Giporlos" },
                new City { municipality = "Eastern Samar", city = "Guiuan" },
                new City { municipality = "Eastern Samar", city = "Hernani" },
                new City { municipality = "Eastern Samar", city = "Jipapad" },
                new City { municipality = "Eastern Samar", city = "Lawaan" },
                new City { municipality = "Eastern Samar", city = "Llorente" },
                new City { municipality = "Eastern Samar", city = "Maslog" },
                new City { municipality = "Eastern Samar", city = "Maydolong" },
                new City { municipality = "Eastern Samar", city = "Mercedes" },
                new City { municipality = "Eastern Samar", city = "Oras" },
                new City { municipality = "Eastern Samar", city = "Quinapondan" },
                new City { municipality = "Eastern Samar", city = "Salcedo" },
                new City { municipality = "Eastern Samar", city = "San Julian" },
                new City { municipality = "Eastern Samar", city = "San Policarpo" },
                new City { municipality = "Eastern Samar", city = "Sulat" },
                new City { municipality = "Eastern Samar", city = "Taft " },
                new City { municipality = "Guimaras", city = "Buenavista" },
                new City { municipality = "Guimaras", city = "Jordan" },
                new City { municipality = "Guimaras", city = "Nueva Valencia" },
                new City { municipality = "Guimaras", city = "San Lorenzo" },
                new City { municipality = "Guimaras", city = "Sibunag" },
                new City { municipality = "Ifugao", city = "Aguinaldo" },
                new City { municipality = "Ifugao", city = "Alfonso Lista" },
                new City { municipality = "Ifugao", city = "Asipulo" },
                new City { municipality = "Ifugao", city = "Banaue" },
                new City { municipality = "Ifugao", city = "Hingyon" },
                new City { municipality = "Ifugao", city = "Hungduan" },
                new City { municipality = "Ifugao", city = "Kiangan" },
                new City { municipality = "Ifugao", city = "Lagawe" },
                new City { municipality = "Ifugao", city = "Lamut" },
                new City { municipality = "Ifugao", city = "Mayoyao" },
                new City { municipality = "Ifugao", city = "Tinoc" },
                new City { municipality = "Ilocos Norte", city = "Adams" },
                new City { municipality = "Ilocos Norte", city = "Bacarra" },
                new City { municipality = "Ilocos Norte", city = "Badoc" },
                new City { municipality = "Ilocos Norte", city = "Bangui" },
                new City { municipality = "Ilocos Norte", city = "Banna" },
                new City { municipality = "Ilocos Norte", city = "Batac" },
                new City { municipality = "Ilocos Norte", city = "Burgos" },
                new City { municipality = "Ilocos Norte", city = "Carasi" },
                new City { municipality = "Ilocos Norte", city = "Currimao" },
                new City { municipality = "Ilocos Norte", city = "Dingras" },
                new City { municipality = "Ilocos Norte", city = "Dumalneg" },
                new City { municipality = "Ilocos Norte", city = "Laoag" },
                new City { municipality = "Ilocos Norte", city = "Marcos" },
                new City { municipality = "Ilocos Norte", city = "Nueva Era" },
                new City { municipality = "Ilocos Norte", city = "Pagudpud" },
                new City { municipality = "Ilocos Norte", city = "Paoay" },
                new City { municipality = "Ilocos Norte", city = "Pasuquin" },
                new City { municipality = "Ilocos Norte", city = "Piddig" },
                new City { municipality = "Ilocos Norte", city = "Pinili" },
                new City { municipality = "Ilocos Norte", city = "San Nicolas" },
                new City { municipality = "Ilocos Norte", city = "Sarrat" },
                new City { municipality = "Ilocos Norte", city = "Solsona" },
                new City { municipality = "Ilocos Norte", city = "Vintar" },
                new City { municipality = "Ilocos Sur", city = "Alilem" },
                new City { municipality = "Ilocos Sur", city = "Banayoyo" },
                new City { municipality = "Ilocos Sur", city = "Bantay" },
                new City { municipality = "Ilocos Sur", city = "Burgos" },
                new City { municipality = "Ilocos Sur", city = "Cabugao" },
                new City { municipality = "Ilocos Sur", city = "Candon" },
                new City { municipality = "Ilocos Sur", city = "Caoayan" },
                new City { municipality = "Ilocos Sur", city = "Cervantes" },
                new City { municipality = "Ilocos Sur", city = "Galimuyod" },
                new City { municipality = "Ilocos Sur", city = "Gregorio del Pilar" },
                new City { municipality = "Ilocos Sur", city = "Lidlidda" },
                new City { municipality = "Ilocos Sur", city = "Magsingal" },
                new City { municipality = "Ilocos Sur", city = "Nagbukel" },
                new City { municipality = "Ilocos Sur", city = "Narvacan" },
                new City { municipality = "Ilocos Sur", city = "Quirino" },
                new City { municipality = "Ilocos Sur", city = "Salcedo" },
                new City { municipality = "Ilocos Sur", city = "San Emilio" },
                new City { municipality = "Ilocos Sur", city = "San Esteban" },
                new City { municipality = "Ilocos Sur", city = "San Ildefonso" },
                new City { municipality = "Ilocos Sur", city = "San Juan" },
                new City { municipality = "Ilocos Sur", city = "San Vicente" },
                new City { municipality = "Ilocos Sur", city = "Santa" },
                new City { municipality = "Ilocos Sur", city = "Sta. Catalina" },
                new City { municipality = "Ilocos Sur", city = "Sta. Cruz" },
                new City { municipality = "Ilocos Sur", city = "Sta. Lucia" },
                new City { municipality = "Ilocos Sur", city = "Sta. Maria" },
                new City { municipality = "Ilocos Sur", city = "Santiago" },
                new City { municipality = "Ilocos Sur", city = "Sto. Domingo" },
                new City { municipality = "Ilocos Sur", city = "Sigay" },
                new City { municipality = "Ilocos Sur", city = "Sinait" },
                new City { municipality = "Ilocos Sur", city = "Sugpon" },
                new City { municipality = "Ilocos Sur", city = "Suyo" },
                new City { municipality = "Ilocos Sur", city = "Tagudin" },
                new City { municipality = "Ilocos Sur", city = "Vigan" },
                new City { municipality = "Iloilo", city = "Ajuy" },
                new City { municipality = "Iloilo", city = "Alimodian" },
                new City { municipality = "Iloilo", city = "Anilao" },
                new City { municipality = "Iloilo", city = "Badiangan" },
                new City { municipality = "Iloilo", city = "Balasan" },
                new City { municipality = "Iloilo", city = "Banate" },
                new City { municipality = "Iloilo", city = "Barotac Nuevo" },
                new City { municipality = "Iloilo", city = "Barotac Viejo" },
                new City { municipality = "Iloilo", city = "Batad" },
                new City { municipality = "Iloilo", city = "Bingawan" },
                new City { municipality = "Iloilo", city = "Cabatuan" },
                new City { municipality = "Iloilo", city = "Calinog" },
                new City { municipality = "Iloilo", city = "Carles" },
                new City { municipality = "Iloilo", city = "Concepcion" },
                new City { municipality = "Iloilo", city = "Dingle" },
                new City { municipality = "Iloilo", city = "Dueñas" },
                new City { municipality = "Iloilo", city = "Dumangas" },
                new City { municipality = "Iloilo", city = "Estancia" },
                new City { municipality = "Iloilo", city = "Guimbal" },
                new City { municipality = "Iloilo", city = "Igbaras" },
                new City { municipality = "Iloilo", city = "Iloilo City" },
                new City { municipality = "Iloilo", city = "Janiuay" },
                new City { municipality = "Iloilo", city = "Lambunao" },
                new City { municipality = "Iloilo", city = "Leganes" },
                new City { municipality = "Iloilo", city = "Lemery" },
                new City { municipality = "Iloilo", city = "Leon" },
                new City { municipality = "Iloilo", city = "Maasin" },
                new City { municipality = "Iloilo", city = "Miagao" },
                new City { municipality = "Iloilo", city = "Mina" },
                new City { municipality = "Iloilo", city = "New Lucena" },
                new City { municipality = "Iloilo", city = "Oton" },
                new City { municipality = "Iloilo", city = "Passi" },
                new City { municipality = "Iloilo", city = "Pavia" },
                new City { municipality = "Iloilo", city = "Pototan" },
                new City { municipality = "Iloilo", city = "San Dionisio" },
                new City { municipality = "Iloilo", city = "San Enrique" },
                new City { municipality = "Iloilo", city = "San Joaquin" },
                new City { municipality = "Iloilo", city = "San Miguel" },
                new City { municipality = "Iloilo", city = "San Rafael" },
                new City { municipality = "Iloilo", city = "Sta. Barbara" },
                new City { municipality = "Iloilo", city = "Sara" },
                new City { municipality = "Iloilo", city = "Tigbauan" },
                new City { municipality = "Iloilo", city = "Tubungan" },
                new City { municipality = "Iloilo", city = "Zarraga" },
                new City { municipality = "Isabela", city = "Alicia" },
                new City { municipality = "Isabela", city = "Angadanan" },
                new City { municipality = "Isabela", city = "Aurora" },
                new City { municipality = "Isabela", city = "Benito Soliven" },
                new City { municipality = "Isabela", city = "Burgos" },
                new City { municipality = "Isabela", city = "Cabagan" },
                new City { municipality = "Isabela", city = "Cabatuan" },
                new City { municipality = "Isabela", city = "Cauayan" },
                new City { municipality = "Isabela", city = "Cordon" },
                new City { municipality = "Isabela", city = "Delfin Albano" },
                new City { municipality = "Isabela", city = "Dinapigue" },
                new City { municipality = "Isabela", city = "Divilacan" },
                new City { municipality = "Isabela", city = "Echague" },
                new City { municipality = "Isabela", city = "Gamu" },
                new City { municipality = "Isabela", city = "Ilagan" },
                new City { municipality = "Isabela", city = "Jones" },
                new City { municipality = "Isabela", city = "Luna" },
                new City { municipality = "Isabela", city = "Maconacon" },
                new City { municipality = "Isabela", city = "Mallig" },
                new City { municipality = "Isabela", city = "Naguilian" },
                new City { municipality = "Isabela", city = "Palanan" },
                new City { municipality = "Isabela", city = "Quezon" },
                new City { municipality = "Isabela", city = "Quirino" },
                new City { municipality = "Isabela", city = "Ramon" },
                new City { municipality = "Isabela", city = "Reina Mercedes" },
                new City { municipality = "Isabela", city = "Roxas" },
                new City { municipality = "Isabela", city = "San Agustin" },
                new City { municipality = "Isabela", city = "San Guillermo" },
                new City { municipality = "Isabela", city = "San Isidro" },
                new City { municipality = "Isabela", city = "San Manuel" },
                new City { municipality = "Isabela", city = "San Mariano" },
                new City { municipality = "Isabela", city = "San Mateo" },
                new City { municipality = "Isabela", city = "San Pablo" },
                new City { municipality = "Isabela", city = "Sta. Maria" },
                new City { municipality = "Isabela", city = "Santiago City" },
                new City { municipality = "Isabela", city = "Santo Tomas" },
                new City { municipality = "Isabela", city = "Tumauini" },
                new City { municipality = "Kalinga", city = "Balbalan" },
                new City { municipality = "Kalinga", city = "Lubuagan" },
                new City { municipality = "Kalinga", city = "Pasil" },
                new City { municipality = "Kalinga", city = "Pinukpuk" },
                new City { municipality = "Kalinga", city = "Rizal" },
                new City { municipality = "Kalinga", city = "Tabuk" },
                new City { municipality = "Kalinga", city = "Tanudan" },
                new City { municipality = "Kalinga", city = "Tinglayan" },
                new City { municipality = "La Union", city = "Agoo" },
                new City { municipality = "La Union", city = "Aringay" },
                new City { municipality = "La Union", city = "Bacnotan" },
                new City { municipality = "La Union", city = "Bagulin" },
                new City { municipality = "La Union", city = "Balaoan" },
                new City { municipality = "La Union", city = "Bangar" },
                new City { municipality = "La Union", city = "Bauang" },
                new City { municipality = "La Union", city = "Burgos" },
                new City { municipality = "La Union", city = "Caba" },
                new City { municipality = "La Union", city = "Luna" },
                new City { municipality = "La Union", city = "Naguilian" },
                new City { municipality = "La Union", city = "Pugo" },
                new City { municipality = "La Union", city = "Rosario" },
                new City { municipality = "La Union", city = "San Fernando" },
                new City { municipality = "La Union", city = "San Gabriel" },
                new City { municipality = "La Union", city = "San Juan" },
                new City { municipality = "La Union", city = "Sto. Tomas" },
                new City { municipality = "La Union", city = "Santol" },
                new City { municipality = "La Union", city = "Sudipen" },
                new City { municipality = "La Union", city = "Tubao" },
                new City { municipality = "Laguna", city = "Alaminos" },
                new City { municipality = "Laguna", city = "Bay" },
                new City { municipality = "Laguna", city = "Biñan" },
                new City { municipality = "Laguna", city = "Cabuyao" },
                new City { municipality = "Laguna", city = "Calamba" },
                new City { municipality = "Laguna", city = "Calauan" },
                new City { municipality = "Laguna", city = "Cavinti" },
                new City { municipality = "Laguna", city = "Famy" },
                new City { municipality = "Laguna", city = "Kalayaan" },
                new City { municipality = "Laguna", city = "Liliw" },
                new City { municipality = "Laguna", city = "Los Baños" },
                new City { municipality = "Laguna", city = "Luisiana" },
                new City { municipality = "Laguna", city = "Lumban" },
                new City { municipality = "Laguna", city = "Mabitac" },
                new City { municipality = "Laguna", city = "Magdalena" },
                new City { municipality = "Laguna", city = "Majayjay" },
                new City { municipality = "Laguna", city = "Nagcarlan" },
                new City { municipality = "Laguna", city = "Paete" },
                new City { municipality = "Laguna", city = "Pagsanjan" },
                new City { municipality = "Laguna", city = "Pakil" },
                new City { municipality = "Laguna", city = "Pangil" },
                new City { municipality = "Laguna", city = "Pila" },
                new City { municipality = "Laguna", city = "Rizal" },
                new City { municipality = "Laguna", city = "San Pablo" },
                new City { municipality = "Laguna", city = "San Pedro" },
                new City { municipality = "Laguna", city = "Sta. Cruz" },
                new City { municipality = "Laguna", city = "Sta. Maria" },
                new City { municipality = "Laguna", city = "Sta. Rosa" },
                new City { municipality = "Laguna", city = "Siniloan" },
                new City { municipality = "Laguna", city = "Victoria" },
                new City { municipality = "Lanao del Norte", city = "Bacolod" },
                new City { municipality = "Lanao del Norte", city = "Baloi" },
                new City { municipality = "Lanao del Norte", city = "Baroy" },
                new City { municipality = "Lanao del Norte", city = "Iligan" },
                new City { municipality = "Lanao del Norte", city = "Kapatagan" },
                new City { municipality = "Lanao del Norte", city = "Kauswagan" },
                new City { municipality = "Lanao del Norte", city = "Kolambugan" },
                new City { municipality = "Lanao del Norte", city = "Lala" },
                new City { municipality = "Lanao del Norte", city = "Linamon" },
                new City { municipality = "Lanao del Norte", city = "Magsaysay" },
                new City { municipality = "Lanao del Norte", city = "Maigo" },
                new City { municipality = "Lanao del Norte", city = "Matungao" },
                new City { municipality = "Lanao del Norte", city = "Munai" },
                new City { municipality = "Lanao del Norte", city = "Nunungan" },
                new City { municipality = "Lanao del Norte", city = "Pantao Ragat" },
                new City { municipality = "Lanao del Norte", city = "Pantar" },
                new City { municipality = "Lanao del Norte", city = "Poona Piagapo" },
                new City { municipality = "Lanao del Norte", city = "Salvador" },
                new City { municipality = "Lanao del Norte", city = "Sapad" },
                new City { municipality = "Lanao del Norte", city = "Sultan Naga Dimaporo" },
                new City { municipality = "Lanao del Norte", city = "Tagoloan" },
                new City { municipality = "Lanao del Norte", city = "Tangcal" },
                new City { municipality = "Lanao del Norte", city = "Tubod" },
                new City { municipality = "Lanao del Sur", city = "Amai Manabilang" },
                new City { municipality = "Lanao del Sur", city = "Bacolod-Kalawi" },
                new City { municipality = "Lanao del Sur", city = "Balabagan" },
                new City { municipality = "Lanao del Sur", city = "Balindong" },
                new City { municipality = "Lanao del Sur", city = "Bayang" },
                new City { municipality = "Lanao del Sur", city = "Binidayan" },
                new City { municipality = "Lanao del Sur", city = "Buadiposo-Buntong" },
                new City { municipality = "Lanao del Sur", city = "Bubong" },
                new City { municipality = "Lanao del Sur", city = "Butig" },
                new City { municipality = "Lanao del Sur", city = "Calanogas" },
                new City { municipality = "Lanao del Sur", city = "Ditsaan-Ramain" },
                new City { municipality = "Lanao del Sur", city = "Ganassi" },
                new City { municipality = "Lanao del Sur", city = "Kapai" },
                new City { municipality = "Lanao del Sur", city = "Kapatagan" },
                new City { municipality = "Lanao del Sur", city = "Lumba-Bayabao" },
                new City { municipality = "Lanao del Sur", city = "Lumbaca-Unayan" },
                new City { municipality = "Lanao del Sur", city = "Lumbatan" },
                new City { municipality = "Lanao del Sur", city = "Lumbayanague" },
                new City { municipality = "Lanao del Sur", city = "Madalum" },
                new City { municipality = "Lanao del Sur", city = "Madamba" },
                new City { municipality = "Lanao del Sur", city = "Maguing" },
                new City { municipality = "Lanao del Sur", city = "Malabang" },
                new City { municipality = "Lanao del Sur", city = "Marantao" },
                new City { municipality = "Lanao del Sur", city = "Marawi" },
                new City { municipality = "Lanao del Sur", city = "Marogong" },
                new City { municipality = "Lanao del Sur", city = "Masiu" },
                new City { municipality = "Lanao del Sur", city = "Mulondo" },
                new City { municipality = "Lanao del Sur", city = "Pagayawan" },
                new City { municipality = "Lanao del Sur", city = "Piagapo" },
                new City { municipality = "Lanao del Sur", city = "Picong" },
                new City { municipality = "Lanao del Sur", city = "Poona Bayabao" },
                new City { municipality = "Lanao del Sur", city = "Pualas" },
                new City { municipality = "Lanao del Sur", city = "Saguiaran" },
                new City { municipality = "Lanao del Sur", city = "Sultan Dumalondong" },
                new City { municipality = "Lanao del Sur", city = "Tagoloan II" },
                new City { municipality = "Lanao del Sur", city = "Tamparan" },
                new City { municipality = "Lanao del Sur", city = "Taraka" },
                new City { municipality = "Lanao del Sur", city = "Tubaran" },
                new City { municipality = "Lanao del Sur", city = "Tugaya" },
                new City { municipality = "Lanao del Sur", city = "Wao" },
                new City { municipality = "Leyte", city = "Abuyog" },
                new City { municipality = "Leyte", city = "Alangalang" },
                new City { municipality = "Leyte", city = "Albuera" },
                new City { municipality = "Leyte", city = "Babatngon" },
                new City { municipality = "Leyte", city = "Barugo" },
                new City { municipality = "Leyte", city = "Bato" },
                new City { municipality = "Leyte", city = "Baybay" },
                new City { municipality = "Leyte", city = "Burauen" },
                new City { municipality = "Leyte", city = "Calubian" },
                new City { municipality = "Leyte", city = "Capoocan" },
                new City { municipality = "Leyte", city = "Carigara" },
                new City { municipality = "Leyte", city = "Dagami" },
                new City { municipality = "Leyte", city = "Dulag" },
                new City { municipality = "Leyte", city = "Hilongos" },
                new City { municipality = "Leyte", city = "Hindang" },
                new City { municipality = "Leyte", city = "Inopacan" },
                new City { municipality = "Leyte", city = "Isabel" },
                new City { municipality = "Leyte", city = "Jaro" },
                new City { municipality = "Leyte", city = "Javier" },
                new City { municipality = "Leyte", city = "Julita" },
                new City { municipality = "Leyte", city = "Kananga" },
                new City { municipality = "Leyte", city = "La Paz" },
                new City { municipality = "Leyte", city = "Leyte" },
                new City { municipality = "Leyte", city = "MacArthur" },
                new City { municipality = "Leyte", city = "Mahaplag" },
                new City { municipality = "Leyte", city = "Matag-ob" },
                new City { municipality = "Leyte", city = "Matalom" },
                new City { municipality = "Leyte", city = "Mayorga" },
                new City { municipality = "Leyte", city = "Merida" },
                new City { municipality = "Leyte", city = "Ormoc" },
                new City { municipality = "Leyte", city = "Palo" },
                new City { municipality = "Leyte", city = "Palompon" },
                new City { municipality = "Leyte", city = "Pastrana" },
                new City { municipality = "Leyte", city = "San Isidro" },
                new City { municipality = "Leyte", city = "San Miguel" },
                new City { municipality = "Leyte", city = "Sta. Fe" },
                new City { municipality = "Leyte", city = "Tabango" },
                new City { municipality = "Leyte", city = "Tabontabon" },
                new City { municipality = "Leyte", city = "Tacloban" },
                new City { municipality = "Leyte", city = "Tanauan" },
                new City { municipality = "Leyte", city = "Tolosa" },
                new City { municipality = "Leyte", city = "Tunga" },
                new City { municipality = "Leyte", city = "Villaba" },
                new City { municipality = "Maguindanao", city = "Ampatuan" },
                new City { municipality = "Maguindanao", city = "Barira" },
                new City { municipality = "Maguindanao", city = "Buldon" },
                new City { municipality = "Maguindanao", city = "Buluan" },
                new City { municipality = "Maguindanao", city = "Cotabato City" },
                new City { municipality = "Maguindanao", city = "Datu Abdullah Sangki" },
                new City { municipality = "Maguindanao", city = "Datu Anggal Midtimbang" },
                new City { municipality = "Maguindanao", city = "Datu Blah T. Sinsuat" },
                new City { municipality = "Maguindanao", city = "Datu Hoffer Ampatuan" },
                new City { municipality = "Maguindanao", city = "Datu Montawal" },
                new City { municipality = "Maguindanao", city = "Datu Odin Sinsuat" },
                new City { municipality = "Maguindanao", city = "Datu Paglas" },
                new City { municipality = "Maguindanao", city = "Datu Piang" },
                new City { municipality = "Maguindanao", city = "Datu Salibo" },
                new City { municipality = "Maguindanao", city = "Datu Saudi-Ampatuan" },
                new City { municipality = "Maguindanao", city = "Datu Unsay" },
                new City { municipality = "Maguindanao", city = "Gen. Salipada K. Pendatun" },
                new City { municipality = "Maguindanao", city = "Guindulungan" },
                new City { municipality = "Maguindanao", city = "Kabuntalan" },
                new City { municipality = "Maguindanao", city = "Mamasapano" },
                new City { municipality = "Maguindanao", city = "Mangudadatu" },
                new City { municipality = "Maguindanao", city = "Matanog" },
                new City { municipality = "Maguindanao", city = "Northern Kabuntalan" },
                new City { municipality = "Maguindanao", city = "Pagalungan" },
                new City { municipality = "Maguindanao", city = "Paglat" },
                new City { municipality = "Maguindanao", city = "Pandag" },
                new City { municipality = "Maguindanao", city = "Parang" },
                new City { municipality = "Maguindanao", city = "Rajah Buayan" },
                new City { municipality = "Maguindanao", city = "Shariff Aguak" },
                new City { municipality = "Maguindanao", city = "Shariff Saydona Mustapha" },
                new City { municipality = "Maguindanao", city = "South Upi" },
                new City { municipality = "Maguindanao", city = "Sultan Kudarat" },
                new City { municipality = "Maguindanao", city = "Sultan Mastura" },
                new City { municipality = "Maguindanao", city = "Sultan sa Barongis" },
                new City { municipality = "Maguindanao", city = "Sultan Sumagka" },
                new City { municipality = "Maguindanao", city = "Talayan" },
                new City { municipality = "Maguindanao", city = "Upi" },
                new City { municipality = "Marinduque", city = "Boac" },
                new City { municipality = "Marinduque", city = "Buenavista" },
                new City { municipality = "Marinduque", city = "Gasan" },
                new City { municipality = "Marinduque", city = "Mogpog" },
                new City { municipality = "Marinduque", city = "Sta. Cruz" },
                new City { municipality = "Marinduque", city = "Torrijos" },
                new City { municipality = "Masbate", city = "Aroroy" },
                new City { municipality = "Masbate", city = "Baleno" },
                new City { municipality = "Masbate", city = "Balud" },
                new City { municipality = "Masbate", city = "Batuan" },
                new City { municipality = "Masbate", city = "Cataingan" },
                new City { municipality = "Masbate", city = "Cawayan" },
                new City { municipality = "Masbate", city = "Claveria" },
                new City { municipality = "Masbate", city = "Dimasalang" },
                new City { municipality = "Masbate", city = "Esperanza" },
                new City { municipality = "Masbate", city = "Mandaon" },
                new City { municipality = "Masbate", city = "Masbate City" },
                new City { municipality = "Masbate", city = "Milagros" },
                new City { municipality = "Masbate", city = "Mobo" },
                new City { municipality = "Masbate", city = "Monreal" },
                new City { municipality = "Masbate", city = "Palanas" },
                new City { municipality = "Masbate", city = "Pio V. Corpuz" },
                new City { municipality = "Masbate", city = "Placer" },
                new City { municipality = "Masbate", city = "San Fernando" },
                new City { municipality = "Masbate", city = "San Jacinto" },
                new City { municipality = "Masbate", city = "San Pascual" },
                new City { municipality = "Masbate", city = "Uson" },
                new City { municipality = "Misamis Occidental", city = "Aloran" },
                new City { municipality = "Misamis Occidental", city = "Baliangao" },
                new City { municipality = "Misamis Occidental", city = "Bonifacio" },
                new City { municipality = "Misamis Occidental", city = "Calamba" },
                new City { municipality = "Misamis Occidental", city = "Clarin" },
                new City { municipality = "Misamis Occidental", city = "Concepcion" },
                new City { municipality = "Misamis Occidental", city = "Don Victoriano Chiongbian" },
                new City { municipality = "Misamis Occidental", city = "Jimenez" },
                new City { municipality = "Misamis Occidental", city = "Lopez Jaena" },
                new City { municipality = "Misamis Occidental", city = "Oroquieta" },
                new City { municipality = "Misamis Occidental", city = "Ozamiz" },
                new City { municipality = "Misamis Occidental", city = "Panaon" },
                new City { municipality = "Misamis Occidental", city = "Plaridel" },
                new City { municipality = "Misamis Occidental", city = "Sapang Dalaga" },
                new City { municipality = "Misamis Occidental", city = "Sinacaban" },
                new City { municipality = "Misamis Occidental", city = "Tangub" },
                new City { municipality = "Misamis Occidental", city = "Tudela " },
                new City { municipality = "Misamis Oriental", city = "Alubijid" },
                new City { municipality = "Misamis Oriental", city = "Balingasag" },
                new City { municipality = "Misamis Oriental", city = "Balingoan" },
                new City { municipality = "Misamis Oriental", city = "Binuangan" },
                new City { municipality = "Misamis Oriental", city = "Cagayan de Oro" },
                new City { municipality = "Misamis Oriental", city = "Claveria" },
                new City { municipality = "Misamis Oriental", city = "El Salvador" },
                new City { municipality = "Misamis Oriental", city = "Gingoog" },
                new City { municipality = "Misamis Oriental", city = "Gitagum" },
                new City { municipality = "Misamis Oriental", city = "Initao" },
                new City { municipality = "Misamis Oriental", city = "Jasaan" },
                new City { municipality = "Misamis Oriental", city = "Kinoguitan" },
                new City { municipality = "Misamis Oriental", city = "Lagonglong" },
                new City { municipality = "Misamis Oriental", city = "Laguindingan" },
                new City { municipality = "Misamis Oriental", city = "Libertad" },
                new City { municipality = "Misamis Oriental", city = "Lugait" },
                new City { municipality = "Misamis Oriental", city = "Magsaysay" },
                new City { municipality = "Misamis Oriental", city = "Manticao" },
                new City { municipality = "Misamis Oriental", city = "Medina" },
                new City { municipality = "Misamis Oriental", city = "Naawan" },
                new City { municipality = "Misamis Oriental", city = "Opol" },
                new City { municipality = "Misamis Oriental", city = "Salay" },
                new City { municipality = "Misamis Oriental", city = "Sugbongcogon" },
                new City { municipality = "Misamis Oriental", city = "Tagoloan" },
                new City { municipality = "Misamis Oriental", city = "Talisayan" },
                new City { municipality = "Misamis Oriental", city = "Villanueva" },
                new City { municipality = "Mountain Province", city = "Barlig" },
                new City { municipality = "Mountain Province", city = "Bauko" },
                new City { municipality = "Mountain Province", city = "Besao" },
                new City { municipality = "Mountain Province", city = "Bontoc" },
                new City { municipality = "Mountain Province", city = "Natonin" },
                new City { municipality = "Mountain Province", city = "Paracelis" },
                new City { municipality = "Mountain Province", city = "Sabangan" },
                new City { municipality = "Mountain Province", city = "Sadanga" },
                new City { municipality = "Mountain Province", city = "Sagada" },
                new City { municipality = "Mountain Province", city = "Tadian" },
                new City { municipality = "NCR", city = "Caloocan" },
                new City { municipality = "NCR", city = "Las Piñas" },
                new City { municipality = "NCR", city = "Makati" },
                new City { municipality = "NCR", city = "Malabon" },
                new City { municipality = "NCR", city = "Mandaluyong" },
                new City { municipality = "NCR", city = "Manila" },
                new City { municipality = "NCR", city = "Marikina" },
                new City { municipality = "NCR", city = "Muntinlupa" },
                new City { municipality = "NCR", city = "Navotas" },
                new City { municipality = "NCR", city = "Parañaque" },
                new City { municipality = "NCR", city = "Pasay" },
                new City { municipality = "NCR", city = "Pasig" },
                new City { municipality = "NCR", city = "Pateros" },
                new City { municipality = "NCR", city = "Quezon City" },
                new City { municipality = "NCR", city = "San Juan" },
                new City { municipality = "NCR", city = "Taguig" },
                new City { municipality = "NCR", city = "Valenzuela" },
                new City { municipality = "Negros Occidental", city = "Bacolod" },
                new City { municipality = "Negros Occidental", city = "Bago" },
                new City { municipality = "Negros Occidental", city = "Binalbagan" },
                new City { municipality = "Negros Occidental", city = "Cadiz" },
                new City { municipality = "Negros Occidental", city = "Calatrava" },
                new City { municipality = "Negros Occidental", city = "Candoni" },
                new City { municipality = "Negros Occidental", city = "Cauayan" },
                new City { municipality = "Negros Occidental", city = "Enrique B. Magalona" },
                new City { municipality = "Negros Occidental", city = "Escalante" },
                new City { municipality = "Negros Occidental", city = "Himamaylan" },
                new City { municipality = "Negros Occidental", city = "Hinigaran" },
                new City { municipality = "Negros Occidental", city = "Hinoba-an" },
                new City { municipality = "Negros Occidental", city = "Ilog" },
                new City { municipality = "Negros Occidental", city = "Isabela" },
                new City { municipality = "Negros Occidental", city = "Kabankalan" },
                new City { municipality = "Negros Occidental", city = "La Carlota" },
                new City { municipality = "Negros Occidental", city = "La Castellana" },
                new City { municipality = "Negros Occidental", city = "Manapla" },
                new City { municipality = "Negros Occidental", city = "Moises Padilla" },
                new City { municipality = "Negros Occidental", city = "Murcia" },
                new City { municipality = "Negros Occidental", city = "Pontevedra" },
                new City { municipality = "Negros Occidental", city = "Pulupandan" },
                new City { municipality = "Negros Occidental", city = "Sagay" },
                new City { municipality = "Negros Occidental", city = "Salvador Benedicto" },
                new City { municipality = "Negros Occidental", city = "San Carlos" },
                new City { municipality = "Negros Occidental", city = "San Enrique" },
                new City { municipality = "Negros Occidental", city = "Silay" },
                new City { municipality = "Negros Occidental", city = "Sipalay" },
                new City { municipality = "Negros Occidental", city = "Talisay" },
                new City { municipality = "Negros Occidental", city = "Toboso" },
                new City { municipality = "Negros Occidental", city = "Valladolid" },
                new City { municipality = "Negros Occidental", city = "Victorias" },
                new City { municipality = "Negros Oriental", city = "Amlan" },
                new City { municipality = "Negros Oriental", city = "Ayungon" },
                new City { municipality = "Negros Oriental", city = "Bacong" },
                new City { municipality = "Negros Oriental", city = "Bais" },
                new City { municipality = "Negros Oriental", city = "Basay" },
                new City { municipality = "Negros Oriental", city = "Bayawan" },
                new City { municipality = "Negros Oriental", city = "Bindoy" },
                new City { municipality = "Negros Oriental", city = "Canlaon" },
                new City { municipality = "Negros Oriental", city = "Dauin" },
                new City { municipality = "Negros Oriental", city = "Dumaguete" },
                new City { municipality = "Negros Oriental", city = "Guihulngan" },
                new City { municipality = "Negros Oriental", city = "Jimalalud" },
                new City { municipality = "Negros Oriental", city = "La Libertad" },
                new City { municipality = "Negros Oriental", city = "Mabinay" },
                new City { municipality = "Negros Oriental", city = "Manjuyod" },
                new City { municipality = "Negros Oriental", city = "Pamplona" },
                new City { municipality = "Negros Oriental", city = "San Jose" },
                new City { municipality = "Negros Oriental", city = "Sta. Catalina" },
                new City { municipality = "Negros Oriental", city = "Siaton" },
                new City { municipality = "Negros Oriental", city = "Sibulan" },
                new City { municipality = "Negros Oriental", city = "Tanjay" },
                new City { municipality = "Negros Oriental", city = "Tayasan" },
                new City { municipality = "Negros Oriental", city = "Valencia" },
                new City { municipality = "Negros Oriental", city = "Vallehermoso" },
                new City { municipality = "Negros Oriental", city = "Zamboanguita" },
                new City { municipality = "Northern Samar", city = "Allen" },
                new City { municipality = "Northern Samar", city = "Biri" },
                new City { municipality = "Northern Samar", city = "Bobon" },
                new City { municipality = "Northern Samar", city = "Capul" },
                new City { municipality = "Northern Samar", city = "Catarman" },
                new City { municipality = "Northern Samar", city = "Catubig" },
                new City { municipality = "Northern Samar", city = "Gamay" },
                new City { municipality = "Northern Samar", city = "Laoang" },
                new City { municipality = "Northern Samar", city = "Lapinig" },
                new City { municipality = "Northern Samar", city = "Las Navas" },
                new City { municipality = "Northern Samar", city = "Lavezares" },
                new City { municipality = "Northern Samar", city = "Lope de Vega" },
                new City { municipality = "Northern Samar", city = "Mapanas" },
                new City { municipality = "Northern Samar", city = "Mondragon" },
                new City { municipality = "Northern Samar", city = "Palapag" },
                new City { municipality = "Northern Samar", city = "Pambujan" },
                new City { municipality = "Northern Samar", city = "Rosario" },
                new City { municipality = "Northern Samar", city = "San Antonio" },
                new City { municipality = "Northern Samar", city = "San Isidro" },
                new City { municipality = "Northern Samar", city = "San Jose" },
                new City { municipality = "Northern Samar", city = "San Roque" },
                new City { municipality = "Northern Samar", city = "San Vicente" },
                new City { municipality = "Northern Samar", city = "Silvino Lobos" },
                new City { municipality = "Northern Samar", city = "Victoria" },
                new City { municipality = "Nueva Ecija", city = "Aliaga" },
                new City { municipality = "Nueva Ecija", city = "Bongabon" },
                new City { municipality = "Nueva Ecija", city = "Cabanatuan" },
                new City { municipality = "Nueva Ecija", city = "Cabiao" },
                new City { municipality = "Nueva Ecija", city = "Carranglan" },
                new City { municipality = "Nueva Ecija", city = "Cuyapo" },
                new City { municipality = "Nueva Ecija", city = "Gabaldon" },
                new City { municipality = "Nueva Ecija", city = "Gapan" },
                new City { municipality = "Nueva Ecija", city = "Gen. Mamerto Natividad" },
                new City { municipality = "Nueva Ecija", city = "Gen. Tinio" },
                new City { municipality = "Nueva Ecija", city = "Guimba" },
                new City { municipality = "Nueva Ecija", city = "Jaen" },
                new City { municipality = "Nueva Ecija", city = "Laur" },
                new City { municipality = "Nueva Ecija", city = "Licab" },
                new City { municipality = "Nueva Ecija", city = "Llanera" },
                new City { municipality = "Nueva Ecija", city = "Lupao" },
                new City { municipality = "Nueva Ecija", city = "Muñoz" },
                new City { municipality = "Nueva Ecija", city = "Nampicuan" },
                new City { municipality = "Nueva Ecija", city = "Palayan" },
                new City { municipality = "Nueva Ecija", city = "Pantabangan" },
                new City { municipality = "Nueva Ecija", city = "Peñaranda" },
                new City { municipality = "Nueva Ecija", city = "Quezon" },
                new City { municipality = "Nueva Ecija", city = "Rizal" },
                new City { municipality = "Nueva Ecija", city = "San Antonio" },
                new City { municipality = "Nueva Ecija", city = "San Isidro" },
                new City { municipality = "Nueva Ecija", city = "San Jose" },
                new City { municipality = "Nueva Ecija", city = "San Leonardo" },
                new City { municipality = "Nueva Ecija", city = "Sta. Rosa" },
                new City { municipality = "Nueva Ecija", city = "Sto. Domingo" },
                new City { municipality = "Nueva Ecija", city = "Talavera" },
                new City { municipality = "Nueva Ecija", city = "Talugtug" },
                new City { municipality = "Nueva Ecija", city = "Zaragoza" },
                new City { municipality = "Nueva Vizcaya", city = "Alfonso Castañeda" },
                new City { municipality = "Nueva Vizcaya", city = "Ambaguio" },
                new City { municipality = "Nueva Vizcaya", city = "Aritao" },
                new City { municipality = "Nueva Vizcaya", city = "Bagabag" },
                new City { municipality = "Nueva Vizcaya", city = "Bambang" },
                new City { municipality = "Nueva Vizcaya", city = "Bayombong" },
                new City { municipality = "Nueva Vizcaya", city = "Diadi" },
                new City { municipality = "Nueva Vizcaya", city = "Dupax del Norte" },
                new City { municipality = "Nueva Vizcaya", city = "Dupax del Sur" },
                new City { municipality = "Nueva Vizcaya", city = "Kasibu" },
                new City { municipality = "Nueva Vizcaya", city = "Kayapa" },
                new City { municipality = "Nueva Vizcaya", city = "Quezon" },
                new City { municipality = "Nueva Vizcaya", city = "Sta. Fe" },
                new City { municipality = "Nueva Vizcaya", city = "Solano" },
                new City { municipality = "Nueva Vizcaya", city = "Villaverde" },

                new City { municipality = "Occidental Mindoro", city = "Abra de Ilog" },
                new City { municipality = "Occidental Mindoro", city = "Calintaan" },
                new City { municipality = "Occidental Mindoro", city = "Looc" },
                new City { municipality = "Occidental Mindoro", city = "Lubang" },
                new City { municipality = "Occidental Mindoro", city = "Magsaysay" },
                new City { municipality = "Occidental Mindoro", city = "Mamburao" },
                new City { municipality = "Occidental Mindoro", city = "Paluan" },
                new City { municipality = "Occidental Mindoro", city = "Rizal" },
                new City { municipality = "Occidental Mindoro", city = "Sablayan" },
                new City { municipality = "Occidental Mindoro", city = "San Jose" },
                new City { municipality = "Occidental Mindoro", city = "Sta. Cruz" },
                new City { municipality = "Oriental Mindoro", city = "Baco" },
                new City { municipality = "Oriental Mindoro", city = "Bansud" },
                new City { municipality = "Oriental Mindoro", city = "Bongabong" },
                new City { municipality = "Oriental Mindoro", city = "Bulalacao" },
                new City { municipality = "Oriental Mindoro", city = "Calapan" },
                new City { municipality = "Oriental Mindoro", city = "Gloria" },
                new City { municipality = "Oriental Mindoro", city = "Mansalay" },
                new City { municipality = "Oriental Mindoro", city = "Naujan" },
                new City { municipality = "Oriental Mindoro", city = "Pinamalayan" },
                new City { municipality = "Oriental Mindoro", city = "Pola" },
                new City { municipality = "Oriental Mindoro", city = "Puerto Galera" },
                new City { municipality = "Oriental Mindoro", city = "Roxas" },
                new City { municipality = "Oriental Mindoro", city = "San Teodoro" },
                new City { municipality = "Oriental Mindoro", city = "Socorro" },
                new City { municipality = "Oriental Mindoro", city = "Victoria" },
                new City { municipality = "Palawan", city = "Aborlan" },
                new City { municipality = "Palawan", city = "Agutaya" },
                new City { municipality = "Palawan", city = "Araceli" },
                new City { municipality = "Palawan", city = "Balabac" },
                new City { municipality = "Palawan", city = "Bataraza" },
                new City { municipality = "Palawan", city = "Brooke's Point" },
                new City { municipality = "Palawan", city = "Busuanga" },
                new City { municipality = "Palawan", city = "Cagayancillo" },
                new City { municipality = "Palawan", city = "Coron" },
                new City { municipality = "Palawan", city = "Culion" },
                new City { municipality = "Palawan", city = "Cuyo" },
                new City { municipality = "Palawan", city = "Dumaran" },
                new City { municipality = "Palawan", city = "El Nido" },
                new City { municipality = "Palawan", city = "Kalayaan" },
                new City { municipality = "Palawan", city = "Linapacan" },
                new City { municipality = "Palawan", city = "Magsaysay" },
                new City { municipality = "Palawan", city = "Narra" },
                new City { municipality = "Palawan", city = "Puerto Princesa" },
                new City { municipality = "Palawan", city = "Quezon" },
                new City { municipality = "Palawan", city = "Rizal" },
                new City { municipality = "Palawan", city = "Roxas" },
                new City { municipality = "Palawan", city = "San Vicente" },
                new City { municipality = "Palawan", city = "Sofronio Española" },
                new City { municipality = "Palawan", city = "Taytay" },
                new City { municipality = "Pampanga", city = "Angeles" },
                new City { municipality = "Pampanga", city = "Apalit" },
                new City { municipality = "Pampanga", city = "Arayat" },
                new City { municipality = "Pampanga", city = "Bacolor" },
                new City { municipality = "Pampanga", city = "Candaba" },
                new City { municipality = "Pampanga", city = "Floridablanca" },
                new City { municipality = "Pampanga", city = "Guagua" },
                new City { municipality = "Pampanga", city = "Lubao" },
                new City { municipality = "Pampanga", city = "Mabalacat" },
                new City { municipality = "Pampanga", city = "Macabebe" },
                new City { municipality = "Pampanga", city = "Magalang" },
                new City { municipality = "Pampanga", city = "Masantol" },
                new City { municipality = "Pampanga", city = "Mexico" },
                new City { municipality = "Pampanga", city = "Minalin" },
                new City { municipality = "Pampanga", city = "Porac" },
                new City { municipality = "Pampanga", city = "San Fernando" },
                new City { municipality = "Pampanga", city = "San Luis" },
                new City { municipality = "Pampanga", city = "San Simon" },
                new City { municipality = "Pampanga", city = "Sta. Ana" },
                new City { municipality = "Pampanga", city = "Sta. Rita" },
                new City { municipality = "Pampanga", city = "Sto. Tomas" },
                new City { municipality = "Pampanga", city = "Sasmuan" },
                new City { municipality = "Pangasinan", city = "Agno" },
                new City { municipality = "Pangasinan", city = "Aguilar" },
                new City { municipality = "Pangasinan", city = "Alaminos" },
                new City { municipality = "Pangasinan", city = "Alcala" },
                new City { municipality = "Pangasinan", city = "Anda" },
                new City { municipality = "Pangasinan", city = "Asingan" },
                new City { municipality = "Pangasinan", city = "Balungao" },
                new City { municipality = "Pangasinan", city = "Bani" },
                new City { municipality = "Pangasinan", city = "Basista" },
                new City { municipality = "Pangasinan", city = "Bautista" },
                new City { municipality = "Pangasinan", city = "Bayambang" },
                new City { municipality = "Pangasinan", city = "Binalonan" },
                new City { municipality = "Pangasinan", city = "Binmaley" },
                new City { municipality = "Pangasinan", city = "Bolinao" },
                new City { municipality = "Pangasinan", city = "Bugallon" },
                new City { municipality = "Pangasinan", city = "Burgos" },
                new City { municipality = "Pangasinan", city = "Calasiao" },
                new City { municipality = "Pangasinan", city = "Dagupan" },
                new City { municipality = "Pangasinan", city = "Dasol" },
                new City { municipality = "Pangasinan", city = "Infanta" },
                new City { municipality = "Pangasinan", city = "Labrador" },
                new City { municipality = "Pangasinan", city = "Laoac" },
                new City { municipality = "Pangasinan", city = "Lingayen" },
                new City { municipality = "Pangasinan", city = "Mabini" },
                new City { municipality = "Pangasinan", city = "Malasiqui" },
                new City { municipality = "Pangasinan", city = "Manaoag" },
                new City { municipality = "Pangasinan", city = "Mangaldan" },
                new City { municipality = "Pangasinan", city = "Mangatarem" },
                new City { municipality = "Pangasinan", city = "Mapandan" },
                new City { municipality = "Pangasinan", city = "Natividad" },
                new City { municipality = "Pangasinan", city = "Pozorrubio" },
                new City { municipality = "Pangasinan", city = "Rosales" },
                new City { municipality = "Pangasinan", city = "San Carlos" },
                new City { municipality = "Pangasinan", city = "San Fabian" },
                new City { municipality = "Pangasinan", city = "San Jacinto" },
                new City { municipality = "Pangasinan", city = "San Manuel" },
                new City { municipality = "Pangasinan", city = "San Nicolas" },
                new City { municipality = "Pangasinan", city = "San Quintin" },
                new City { municipality = "Pangasinan", city = "Sta. Barbara" },
                new City { municipality = "Pangasinan", city = "Sta. Maria" },
                new City { municipality = "Pangasinan", city = "Sto. Tomas" },
                new City { municipality = "Pangasinan", city = "Sison" },
                new City { municipality = "Pangasinan", city = "Sual" },
                new City { municipality = "Pangasinan", city = "Tayug" },
                new City { municipality = "Pangasinan", city = "Umingan" },
                new City { municipality = "Pangasinan", city = "Urbiztondo" },
                new City { municipality = "Pangasinan", city = "Urdaneta" },
                new City { municipality = "Pangasinan", city = "Villasis" },
                new City { municipality = "Quezon", city = "Agdangan" },
                new City { municipality = "Quezon", city = "Alabat" },
                new City { municipality = "Quezon", city = "Atimonan" },
                new City { municipality = "Quezon", city = "Buenavista" },
                new City { municipality = "Quezon", city = "Burdeos" },
                new City { municipality = "Quezon", city = "Calauag" },
                new City { municipality = "Quezon", city = "Candelaria" },
                new City { municipality = "Quezon", city = "Catanauan" },
                new City { municipality = "Quezon", city = "Dolores" },
                new City { municipality = "Quezon", city = "Gen. Luna" },
                new City { municipality = "Quezon", city = "Gen. Nakar" },
                new City { municipality = "Quezon", city = "Guinayangan" },
                new City { municipality = "Quezon", city = "Gumaca" },
                new City { municipality = "Quezon", city = "Infanta" },
                new City { municipality = "Quezon", city = "Jomalig" },
                new City { municipality = "Quezon", city = "Lopez" },
                new City { municipality = "Quezon", city = "Lucban" },
                new City { municipality = "Quezon", city = "Lucena" },
                new City { municipality = "Quezon", city = "Macalelon" },
                new City { municipality = "Quezon", city = "Mauban" },
                new City { municipality = "Quezon", city = "Mulanay" },
                new City { municipality = "Quezon", city = "Padre Burgos" },
                new City { municipality = "Quezon", city = "Pagbilao" },
                new City { municipality = "Quezon", city = "Panukulan" },
                new City { municipality = "Quezon", city = "Patnanungan" },
                new City { municipality = "Quezon", city = "Perez" },
                new City { municipality = "Quezon", city = "Pitogo" },
                new City { municipality = "Quezon", city = "Plaridel" },
                new City { municipality = "Quezon", city = "Polillo" },
                new City { municipality = "Quezon", city = "Quezon" },
                new City { municipality = "Quezon", city = "Real" },
                new City { municipality = "Quezon", city = "Sampaloc" },
                new City { municipality = "Quezon", city = "San Andres" },
                new City { municipality = "Quezon", city = "San Antonio" },
                new City { municipality = "Quezon", city = "San Francisco" },
                new City { municipality = "Quezon", city = "San Narciso" },
                new City { municipality = "Quezon", city = "Sariaya" },
                new City { municipality = "Quezon", city = "Tagkawayan" },
                new City { municipality = "Quezon", city = "Tayabas" },
                new City { municipality = "Quezon", city = "Tiaong" },
                new City { municipality = "Quezon", city = "Unisan" },
                new City { municipality = "Quirino", city = "Aglipay" },
                new City { municipality = "Quirino", city = "Cabarroguis" },
                new City { municipality = "Quirino", city = "Diffun" },
                new City { municipality = "Quirino", city = "Maddela" },
                new City { municipality = "Quirino", city = "Nagtipunan" },
                new City { municipality = "Quirino", city = "Saguday" },
                new City { municipality = "Rizal", city = "Angono" },
                new City { municipality = "Rizal", city = "Antipolo" },
                new City { municipality = "Rizal", city = "Baras" },
                new City { municipality = "Rizal", city = "Binangonan" },
                new City { municipality = "Rizal", city = "Cainta" },
                new City { municipality = "Rizal", city = "Cardona" },
                new City { municipality = "Rizal", city = "Jalajala" },
                new City { municipality = "Rizal", city = "Morong" },
                new City { municipality = "Rizal", city = "Pililla" },
                new City { municipality = "Rizal", city = "Rodriguez" },
                new City { municipality = "Rizal", city = "San Mateo" },
                new City { municipality = "Rizal", city = "Tanay" },
                new City { municipality = "Rizal", city = "Taytay" },
                new City { municipality = "Rizal", city = "Teresa" },
                new City { municipality = "Romblon", city = "Alcantara" },
                new City { municipality = "Romblon", city = "Banton" },
                new City { municipality = "Romblon", city = "Cajidiocan" },
                new City { municipality = "Romblon", city = "Calatrava" },
                new City { municipality = "Romblon", city = "Concepcion" },
                new City { municipality = "Romblon", city = "Corcuera" },
                new City { municipality = "Romblon", city = "Ferrol" },
                new City { municipality = "Romblon", city = "Looc" },
                new City { municipality = "Romblon", city = "Magdiwang" },
                new City { municipality = "Romblon", city = "Odiongan" },
                new City { municipality = "Romblon", city = "Romblon" },
                new City { municipality = "Romblon", city = "San Agustin" },
                new City { municipality = "Romblon", city = "San Andres" },
                new City { municipality = "Romblon", city = "San Fernando" },
                new City { municipality = "Romblon", city = "San Jose" },
                new City { municipality = "Romblon", city = "Sta. Fe" },
                new City { municipality = "Romblon", city = "Sta. Maria" },
                new City { municipality = "Samar", city = "Almagro" },
                new City { municipality = "Samar", city = "Basey" },
                new City { municipality = "Samar", city = "Calbayog" },
                new City { municipality = "Samar", city = "Calbiga" },
                new City { municipality = "Samar", city = "Catbalogan" },
                new City { municipality = "Samar", city = "Daram" },
                new City { municipality = "Samar", city = "Gandara" },
                new City { municipality = "Samar", city = "Hinabangan" },
                new City { municipality = "Samar", city = "Jiabong" },
                new City { municipality = "Samar", city = "Marabut" },
                new City { municipality = "Samar", city = "Matuguinao" },
                new City { municipality = "Samar", city = "Motiong" },
                new City { municipality = "Samar", city = "Pagsanghan" },
                new City { municipality = "Samar", city = "Paranas" },
                new City { municipality = "Samar", city = "Pinabacdao" },
                new City { municipality = "Samar", city = "San Jorge" },
                new City { municipality = "Samar", city = "San Jose de Buan" },
                new City { municipality = "Samar", city = "San Sebastian" },
                new City { municipality = "Samar", city = "Sta. Margarita" },
                new City { municipality = "Samar", city = "Sta. Rita" },
                new City { municipality = "Samar", city = "Sto. Niño" },
                new City { municipality = "Samar", city = "Tagapul-an" },
                new City { municipality = "Samar", city = "Talalora" },
                new City { municipality = "Samar", city = "Tarangnan" },
                new City { municipality = "Samar", city = "Villareal" },
                new City { municipality = "Samar", city = "Zumarraga" },
                new City { municipality = "Sarangani", city = "Alabel" },
                new City { municipality = "Sarangani", city = "Glan" },
                new City { municipality = "Sarangani", city = "Kiamba" },
                new City { municipality = "Sarangani", city = "Maasim" },
                new City { municipality = "Sarangani", city = "Maitum" },
                new City { municipality = "Sarangani", city = "Malapatan" },
                new City { municipality = "Sarangani", city = "Malungon" },
                new City { municipality = "Siquijor", city = "Enrique Villanueva" },
                new City { municipality = "Siquijor", city = "Larena" },
                new City { municipality = "Siquijor", city = "Lazi" },
                new City { municipality = "Siquijor", city = "Maria" },
                new City { municipality = "Siquijor", city = "San Juan" },
                new City { municipality = "Siquijor", city = "Siquijor" },
                new City { municipality = "Sorsogon", city = "Barcelona" },
                new City { municipality = "Sorsogon", city = "Bulan" },
                new City { municipality = "Sorsogon", city = "Bulusan" },
                new City { municipality = "Sorsogon", city = "Casiguran" },
                new City { municipality = "Sorsogon", city = "Castilla" },
                new City { municipality = "Sorsogon", city = "Donsol" },
                new City { municipality = "Sorsogon", city = "Gubat" },
                new City { municipality = "Sorsogon", city = "Irosin" },
                new City { municipality = "Sorsogon", city = "Juban" },
                new City { municipality = "Sorsogon", city = "Magallanes" },
                new City { municipality = "Sorsogon", city = "Matnog" },
                new City { municipality = "Sorsogon", city = "Pilar" },
                new City { municipality = "Sorsogon", city = "Prieto Diaz" },
                new City { municipality = "Sorsogon", city = "Sta. Magdalena" },
                new City { municipality = "Sorsogon", city = "Sorsogon City" },
                new City { municipality = "South Cotabato", city = "Banga" },
                new City { municipality = "South Cotabato", city = "Gen. Santos" },
                new City { municipality = "South Cotabato", city = "Koronadal" },
                new City { municipality = "South Cotabato", city = "Lake Sebu" },
                new City { municipality = "South Cotabato", city = "Norala" },
                new City { municipality = "South Cotabato", city = "Polomolok" },
                new City { municipality = "South Cotabato", city = "Sto. Niño" },
                new City { municipality = "South Cotabato", city = "Surallah" },
                new City { municipality = "South Cotabato", city = "T'Boli" },
                new City { municipality = "South Cotabato", city = "Tampakan" },
                new City { municipality = "South Cotabato", city = "Tantangan" },
                new City { municipality = "South Cotabato", city = "Tupi" },
                new City { municipality = "Southern Leyte", city = "Anahawan" },
                new City { municipality = "Southern Leyte", city = "Bontoc" },
                new City { municipality = "Southern Leyte", city = "Hinunangan" },
                new City { municipality = "Southern Leyte", city = "Hinundayan" },
                new City { municipality = "Southern Leyte", city = "Libagon" },
                new City { municipality = "Southern Leyte", city = "Liloan" },
                new City { municipality = "Southern Leyte", city = "Limasawa" },
                new City { municipality = "Southern Leyte", city = "Maasin" },
                new City { municipality = "Southern Leyte", city = "Macrohon" },
                new City { municipality = "Southern Leyte", city = "Malitbog" },
                new City { municipality = "Southern Leyte", city = "Padre Burgos" },
                new City { municipality = "Southern Leyte", city = "Pintuyan" },
                new City { municipality = "Southern Leyte", city = "St. Bernard" },
                new City { municipality = "Southern Leyte", city = "San Francisco" },
                new City { municipality = "Southern Leyte", city = "San Juan" },
                new City { municipality = "Southern Leyte", city = "San Ricardo" },
                new City { municipality = "Southern Leyte", city = "Silago" },
                new City { municipality = "Southern Leyte", city = "Sogod" },
                new City { municipality = "Southern Leyte", city = "Tomas Oppus" },
                new City { municipality = "Sultan Kudarat", city = "Bagumbayan" },
                new City { municipality = "Sultan Kudarat", city = "Columbio" },
                new City { municipality = "Sultan Kudarat", city = "Esperanza" },
                new City { municipality = "Sultan Kudarat", city = "Isulan" },
                new City { municipality = "Sultan Kudarat", city = "Kalamansig" },
                new City { municipality = "Sultan Kudarat", city = "Lambayong" },
                new City { municipality = "Sultan Kudarat", city = "Lebak" },
                new City { municipality = "Sultan Kudarat", city = "Lutayan" },
                new City { municipality = "Sultan Kudarat", city = "Palimbang" },
                new City { municipality = "Sultan Kudarat", city = "Pres. Quirino" },
                new City { municipality = "Sultan Kudarat", city = "Sen. Ninoy Aquino" },
                new City { municipality = "Sultan Kudarat", city = "Tacurong" },
                new City { municipality = "Sulu", city = "Banguingui" },
                new City { municipality = "Sulu", city = "Hadji Panglima Tahil" },
                new City { municipality = "Sulu", city = "Indanan" },
                new City { municipality = "Sulu", city = "Jolo" },
                new City { municipality = "Sulu", city = "Kalingalan Caluang" },
                new City { municipality = "Sulu", city = "Lugus" },
                new City { municipality = "Sulu", city = "Luuk" },
                new City { municipality = "Sulu", city = "Maimbung" },
                new City { municipality = "Sulu", city = "Old Panamao" },
                new City { municipality = "Sulu", city = "Omar" },
                new City { municipality = "Sulu", city = "Pandami" },
                new City { municipality = "Sulu", city = "Panglima Estino" },
                new City { municipality = "Sulu", city = "Pangutaran" },
                new City { municipality = "Sulu", city = "Parang" },
                new City { municipality = "Sulu", city = "Pata" },
                new City { municipality = "Sulu", city = "Patikul" },
                new City { municipality = "Sulu", city = "Siasi" },
                new City { municipality = "Sulu", city = "Talipao" },
                new City { municipality = "Sulu", city = "Tapul" },
                new City { municipality = "Surigao del Norte", city = "Alegria" },
                new City { municipality = "Surigao del Norte", city = "Bacuag" },
                new City { municipality = "Surigao del Norte", city = "Burgos" },
                new City { municipality = "Surigao del Norte", city = "Claver" },
                new City { municipality = "Surigao del Norte", city = "Dapa" },
                new City { municipality = "Surigao del Norte", city = "Del Carmen" },
                new City { municipality = "Surigao del Norte", city = "Gen. Luna" },
                new City { municipality = "Surigao del Norte", city = "Gigaquit" },
                new City { municipality = "Surigao del Norte", city = "Mainit" },
                new City { municipality = "Surigao del Norte", city = "Malimono" },
                new City { municipality = "Surigao del Norte", city = "Pilar" },
                new City { municipality = "Surigao del Norte", city = "Placer" },
                new City { municipality = "Surigao del Norte", city = "San Benito" },
                new City { municipality = "Surigao del Norte", city = "San Francisco" },
                new City { municipality = "Surigao del Norte", city = "San Isidro" },
                new City { municipality = "Surigao del Norte", city = "Sta. Monica" },
                new City { municipality = "Surigao del Norte", city = "Sison" },
                new City { municipality = "Surigao del Norte", city = "Socorro" },
                new City { municipality = "Surigao del Norte", city = "Surigao City" },
                new City { municipality = "Surigao del Norte", city = "Tagana-an" },
                new City { municipality = "Surigao del Norte", city = "Tubod" },
                new City { municipality = "Surigao del Sur", city = "Barobo" },
                new City { municipality = "Surigao del Sur", city = "Bayabas" },
                new City { municipality = "Surigao del Sur", city = "Bislig" },
                new City { municipality = "Surigao del Sur", city = "Cagwait" },
                new City { municipality = "Surigao del Sur", city = "Cantilan" },
                new City { municipality = "Surigao del Sur", city = "Carmen" },
                new City { municipality = "Surigao del Sur", city = "Carrascal" },
                new City { municipality = "Surigao del Sur", city = "Cortes" },
                new City { municipality = "Surigao del Sur", city = "Hinatuan" },
                new City { municipality = "Surigao del Sur", city = "Lanuza" },
                new City { municipality = "Surigao del Sur", city = "Lianga" },
                new City { municipality = "Surigao del Sur", city = "Lingig" },
                new City { municipality = "Surigao del Sur", city = "Madrid" },
                new City { municipality = "Surigao del Sur", city = "Marihatag" },
                new City { municipality = "Surigao del Sur", city = "San Agustin" },
                new City { municipality = "Surigao del Sur", city = "San Miguel" },
                new City { municipality = "Surigao del Sur", city = "Tagbina" },
                new City { municipality = "Surigao del Sur", city = "Tago" },
                new City { municipality = "Surigao del Sur", city = "Tandag" },
                new City { municipality = "Tarlac", city = "Anao" },
                new City { municipality = "Tarlac", city = "Bamban" },
                new City { municipality = "Tarlac", city = "Camiling" },
                new City { municipality = "Tarlac", city = "Capas" },
                new City { municipality = "Tarlac", city = "Concepcion" },
                new City { municipality = "Tarlac", city = "Gerona" },
                new City { municipality = "Tarlac", city = "La Paz" },
                new City { municipality = "Tarlac", city = "Mayantoc" },
                new City { municipality = "Tarlac", city = "Paniqui" },
                new City { municipality = "Tarlac", city = "Pura" },
                new City { municipality = "Tarlac", city = "Ramos" },
                new City { municipality = "Tarlac", city = "San Clemente" },
                new City { municipality = "Tarlac", city = "San Jose" },
                new City { municipality = "Tarlac", city = "San Manuel" },
                new City { municipality = "Tarlac", city = "Sta. Ignacia" },
                new City { municipality = "Tarlac", city = "Tarlac City" },
                new City { municipality = "Tarlac", city = "Victoria" },
                new City { municipality = "Tawi-Tawi", city = "Bongao" },
                new City { municipality = "Tawi-Tawi", city = "Languyan" },
                new City { municipality = "Tawi-Tawi", city = "Mapun" },
                new City { municipality = "Tawi-Tawi", city = "Panglima Sugala" },
                new City { municipality = "Tawi-Tawi", city = "Sapa-Sapa" },
                new City { municipality = "Tawi-Tawi", city = "Sibutu" },
                new City { municipality = "Tawi-Tawi", city = "Simunul" },
                new City { municipality = "Tawi-Tawi", city = "Sitangkai" },
                new City { municipality = "Tawi-Tawi", city = "South Ubian" },
                new City { municipality = "Tawi-Tawi", city = "Tandubas" },
                new City { municipality = "Tawi-Tawi", city = "Turtle Islands" },
                new City { municipality = "Zambales", city = "Botolan" },
                new City { municipality = "Zambales", city = "Cabangan" },
                new City { municipality = "Zambales", city = "Candelaria" },
                new City { municipality = "Zambales", city = "Castillejos" },
                new City { municipality = "Zambales", city = "Iba" },
                new City { municipality = "Zambales", city = "Masinloc" },
                new City { municipality = "Zambales", city = "Olongapo" },
                new City { municipality = "Zambales", city = "Palauig" },
                new City { municipality = "Zambales", city = "San Antonio" },
                new City { municipality = "Zambales", city = "San Felipe" },
                new City { municipality = "Zambales", city = "San Marcelino" },
                new City { municipality = "Zambales", city = "San Narciso" },
                new City { municipality = "Zambales", city = "Sta. Cruz" },
                new City { municipality = "Zambales", city = "Subic" },
                new City { municipality = "Zamboanga del Norte", city = "Baliguian" },
                new City { municipality = "Zamboanga del Norte", city = "Dapitan" },
                new City { municipality = "Zamboanga del Norte", city = "Dipolog" },
                new City { municipality = "Zamboanga del Norte", city = "Godod" },
                new City { municipality = "Zamboanga del Norte", city = "Gutalac" },
                new City { municipality = "Zamboanga del Norte", city = "Jose Dalman" },
                new City { municipality = "Zamboanga del Norte", city = "Kalawit" },
                new City { municipality = "Zamboanga del Norte", city = "Katipunan" },
                new City { municipality = "Zamboanga del Norte", city = "La Libertad" },
                new City { municipality = "Zamboanga del Norte", city = "Labason" },
                new City { municipality = "Zamboanga del Norte", city = "Leon B. Postigo" },
                new City { municipality = "Zamboanga del Norte", city = "Liloy" },
                new City { municipality = "Zamboanga del Norte", city = "Manukan" },
                new City { municipality = "Zamboanga del Norte", city = "Mutia" },
                new City { municipality = "Zamboanga del Norte", city = "Piñan" },
                new City { municipality = "Zamboanga del Norte", city = "Polanco" },
                new City { municipality = "Zamboanga del Norte", city = "Pres. Manuel A. Roxas" },
                new City { municipality = "Zamboanga del Norte", city = "Rizal" },
                new City { municipality = "Zamboanga del Norte", city = "Salug" },
                new City { municipality = "Zamboanga del Norte", city = "Sergio Osmeña Sr." },
                new City { municipality = "Zamboanga del Norte", city = "Siayan" },
                new City { municipality = "Zamboanga del Norte", city = "Sibuco" },
                new City { municipality = "Zamboanga del Norte", city = "Sibutad" },
                new City { municipality = "Zamboanga del Norte", city = "Sindangan" },
                new City { municipality = "Zamboanga del Norte", city = "Siocon" },
                new City { municipality = "Zamboanga del Norte", city = "Sirawai" },
                new City { municipality = "Zamboanga del Norte", city = "Tampilisan" },
                new City { municipality = "Zamboanga del Sur", city = "Aurora" },
                new City { municipality = "Zamboanga del Sur", city = "Bayog" },
                new City { municipality = "Zamboanga del Sur", city = "Dimataling" },
                new City { municipality = "Zamboanga del Sur", city = "Dinas" },
                new City { municipality = "Zamboanga del Sur", city = "Dumalinao" },
                new City { municipality = "Zamboanga del Sur", city = "Dumingag" },
                new City { municipality = "Zamboanga del Sur", city = "Guipos" },
                new City { municipality = "Zamboanga del Sur", city = "Josefina" },
                new City { municipality = "Zamboanga del Sur", city = "Kumalarang" },
                new City { municipality = "Zamboanga del Sur", city = "Labangan" },
                new City { municipality = "Zamboanga del Sur", city = "Lakewood" },
                new City { municipality = "Zamboanga del Sur", city = "Lapuyan" },
                new City { municipality = "Zamboanga del Sur", city = "Mahayag" },
                new City { municipality = "Zamboanga del Sur", city = "Margosatubig" },
                new City { municipality = "Zamboanga del Sur", city = "Midsalip" },
                new City { municipality = "Zamboanga del Sur", city = "Molave" },
                new City { municipality = "Zamboanga del Sur", city = "Pagadian" },
                new City { municipality = "Zamboanga del Sur", city = "Pitogo" },
                new City { municipality = "Zamboanga del Sur", city = "Ramon Magsaysay" },
                new City { municipality = "Zamboanga del Sur", city = "San Miguel" },
                new City { municipality = "Zamboanga del Sur", city = "San Pablo" },
                new City { municipality = "Zamboanga del Sur", city = "Sominot" },
                new City { municipality = "Zamboanga del Sur", city = "Tabina" },
                new City { municipality = "Zamboanga del Sur", city = "Tambulig" },
                new City { municipality = "Zamboanga del Sur", city = "Tigbao" },
                new City { municipality = "Zamboanga del Sur", city = "Tukuran" },
                new City { municipality = "Zamboanga del Sur", city = "Vincenzo A. Sagun" },
                new City { municipality = "Zamboanga del Sur", city = "Zamboanga City" },
                new City { municipality = "Zamboanga Sibugay", city = "Alicia" },
                new City { municipality = "Zamboanga Sibugay", city = "Buug" },
                new City { municipality = "Zamboanga Sibugay", city = "Diplahan" },
                new City { municipality = "Zamboanga Sibugay", city = "Imelda" },
                new City { municipality = "Zamboanga Sibugay", city = "Ipil" },
                new City { municipality = "Zamboanga Sibugay", city = "Kabasalan" },
                new City { municipality = "Zamboanga Sibugay", city = "Mabuhay" },
                new City { municipality = "Zamboanga Sibugay", city = "Malangas" },
                new City { municipality = "Zamboanga Sibugay", city = "Naga" },
                new City { municipality = "Zamboanga Sibugay", city = "Olutanga" },
                new City { municipality = "Zamboanga Sibugay", city = "Payao" },
                new City { municipality = "Zamboanga Sibugay", city = "Roseller Lim" },
                new City { municipality = "Zamboanga Sibugay", city = "Siay" },
                new City { municipality = "Zamboanga Sibugay", city = "Talusan" },
                new City { municipality = "Zamboanga Sibugay", city = "Titay" },
                new City { municipality = "Zamboanga Sibugay", city = "Tungawan" }

            };


            return cities;
        }

        public List<Branch> GetBranches()
        {
            var raw = _context.Branches.Where(a => a.IsActive).ToList();

            return raw;
        }

        public BranchModel GetBranch(string branch)
        {
            var isParsable = long.TryParse(branch, out long value);
            var raw = new Branch();
            if (isParsable)
                raw = _context.Branches.Where(a => a.IsActive && a.Id == value).FirstOrDefault();
            else
                raw = _context.Branches.Where(a => a.IsActive && a.BranchName == branch).FirstOrDefault();

            var result = new BranchModel
            {
                Id = raw.Id,
                BranchName = raw.BranchName,
                BranchAddress = raw.BranchAddress,
                MapAddress = raw.MapAddress,
                OfficeHours = raw.OfficeHours,
                ContactNumber = raw.ContactNumber,
                Email = raw.Email,
                HasExpidite = raw.HasExpidite
            };
            return result;
        }

        public BranchModel GetBranch(string branch, int userType = 0)
        {
            var isParsable = long.TryParse(branch, out long value);
            var raw = new Branch();
            if (isParsable)
                raw = _context.Branches.Where(a => a.IsActive && a.Id == value).FirstOrDefault();
            else
                raw = _context.Branches.Where(a => a.IsActive && a.BranchName == branch).FirstOrDefault();

            var availableDates = GenerateListOfDates(DateTime.Now, raw.Id, userType);

            var availTime = _context.ScheduleCapacities.Where(a => a.BranchId == raw.Id && a.Capacity != 0 && a.Type == (userType != 0 ? 1 : 0))
                .OrderBy(a => a.Id)
                .Select(a => new AvailableHour
                {
                    Caption = $"{DateTime.Parse(a.Name).ToString("%h")}-{DateTime.Parse(a.Name).AddHours(1).ToString("h tt")}",
                    Value = $"{DateTime.Parse(a.Name).ToString("hh:mm tt")}"
                }).ToList();

            var result = new BranchModel
            {
                Id = raw.Id,
                BranchName = raw.BranchName,
                BranchAddress = raw.BranchAddress,
                AvailableDates = JsonConvert.SerializeObject(availableDates),
                AvailableHours = availTime, //dates,
                MapAddress = raw.MapAddress,
                OfficeHours = raw.OfficeHours,
                ContactNumber = raw.ContactNumber,
                Email = raw.Email,
                HasExpidite = raw.HasExpidite
            };
            return result;
        }

        public byte[] GenerateQRCode(string plainText)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return BitmapToBytesCode(qrCodeImage);
        }


        public Price GetPrice()
        {
            var price = _context.Prices.Select(a => a).FirstOrDefault();
            return price;
        }

        public Notice GetNotice(long id)
        {
            var notice = _context.Notices.Where(a => a.Id == id).FirstOrDefault();
            return notice;
        }

        private static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public bool CheckIfSchedExistInHoliday(DateTime schedule)
        {
            var isExist = _context.Holidays.Any(a => a.Date.Date == schedule.Date);
            return isExist;
        }


    }
}



// View Maps
// Custom Capacity Per Branch






//select* from
//    (Select
//         count(distinct(Id)) as ApplicantCount,
//		 count(Id) as DocuTypes,
//		 CAST(ScheduleDate as Date) as ScheduleDate,
//		 sum(dataCount.Quantity) as DocuCount

//    from
//        (select Id, ScheduleDate, ApostileData

//            from ApplicantRecords

//            where CAST(ScheduleDate as Date) >= '2020-01-04') ApplicantRecords
//      CROSS APPLY OPENJSON (ApostileData)
//	WITH
//        (Quantity int)

//        AS dataCount

//	group by
//		 CAST(ScheduleDate as Date)) a
//where a.DocuCount > 100