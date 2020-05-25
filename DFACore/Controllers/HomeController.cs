using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DFACore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using DFACore.Repository;

namespace DFACore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicantRecordRepository _applicantRepo;

        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicantRecordRepository applicantRepo)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _applicantRepo = applicantRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ApplicantRecordViewModel record, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var applicantRecord = new ApplicantRecord
            {
                Title = record.Title,
                FirstName = record.FirstName,
                MiddleName = record.MiddleName,
                LastName = record.LastName,
                Suffix = record.Suffix,
                Address = record.Address,
                Nationality = record.Nationality,
                ContactNumber = record.ContactNumber,
                CompanyName = record.CompanyName,
                CountryDestination = record.CountryDestination,
                NameOfRepresentative = record.NameOfRepresentative,
                RepresentativeContactNumber = record.RepresentativeContactNumber,
                ApostileData = record.ApostileData,
                ProcessingSite = record.ProcessingSite,
                ProcessingSiteAddress = record.ProcessingSiteAddress,
                ScheduleDate = DateTime.ParseExact(record.ScheduleDate, "MM/dd/yyyy hh:mm tt",
                                       System.Globalization.CultureInfo.InvariantCulture),
                CreatedBy = new Guid(_userManager.GetUserId(User))
            };


            var result = _applicantRepo.Add(applicantRecord);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            }
            //var name = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            return View();
        }


        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

 
        public ActionResult ValidateScheduleDate(string scheduleDate)
        {
            var date = DateTime.ParseExact(scheduleDate, "MM/dd/yyyy hh:mm tt",
                                       System.Globalization.CultureInfo.InvariantCulture);
            var result = _applicantRepo.ValidateScheduleDate(date);

            return Json(result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public ActionResult Test()
        {
            var result = _applicantRepo.GenerateListOfDates(DateTime.Now);//_applicantRepo.GetUnAvailableDates();
            return Json(result);
        }
    }
}
