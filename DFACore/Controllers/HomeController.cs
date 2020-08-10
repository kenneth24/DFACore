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
using Newtonsoft.Json;
using Wkhtmltopdf.NetCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DFACore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicantRecordRepository _applicantRepo;
        private readonly IMessageService _messageService;
        private readonly IGeneratePdf _generatePdf;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicantRecordRepository applicantRepo,
            IMessageService messageService,
            IGeneratePdf generatePdf,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _applicantRepo = applicantRepo;
            _messageService = messageService;
            _generatePdf = generatePdf;
            _env = env;
        }

        public async Task<IActionResult> Index(int applicantsCount = 0)
        {
            var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            ViewData["AvailableDates"] = stringify;
            ViewData["ApplicationCode"] = GetApplicantCode();
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            ViewData["ApplicantCount"] = applicantsCount;
            ViewBag.User = await _userManager.GetUserAsync(HttpContext.User);
            return View();
        }
        public IActionResult ApplicantTypeSelection()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ApplicantsViewModel model, string returnUrl = null)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View();
            //}
            var applicantRecords = new List<ApplicantRecord>();
            var attachments = new List<Attachment>();

            if ( model.Records == null)
            {
                var applicantRecord = new ApplicantRecord
                {

                    FirstName = model.Record.FirstName?.ToUpper(),
                    MiddleName = model.Record.MiddleName?.ToUpper(),
                    LastName = model.Record.LastName?.ToUpper(),
                    Suffix = model.Record.Suffix?.ToUpper(),
                    DateOfBirth = model.Record.DateOfBirth,
                    ContactNumber = model.Record.ContactNumber,
                    CountryDestination = model.Record.CountryDestination?.ToUpper(),
                    ApostileData = model.Record.ApostileData,
                    ProcessingSite = model.Record.ProcessingSite?.ToUpper(),
                    ProcessingSiteAddress = model.Record.ProcessingSiteAddress?.ToUpper(),
                    ScheduleDate = DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt",
                                       System.Globalization.CultureInfo.InvariantCulture),
                    ApplicationCode = model.Record.ApplicationCode,
                    CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Record.Fees,
                    Type = 0
                };
                applicantRecords.Add(applicantRecord);
                attachments.Add(new Attachment("DFA-Application.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

            }
            else
            {
                foreach (var record in model.Records)
                {
                    var applicantRecord = new ApplicantRecord
                    {
                        FirstName = record.FirstName?.ToUpper(),
                        MiddleName = record.MiddleName?.ToUpper(),
                        LastName = record.LastName?.ToUpper(),
                        Suffix = record.Suffix?.ToUpper(),
                        DateOfBirth = record.DateOfBirth,
                        //Address = $"{record.Barangay?.ToUpper()} {record.City?.ToUpper()} {record.Region?.ToUpper()} ",
                        //Nationality = record.Nationality?.ToUpper(),
                        ContactNumber = record.ContactNumber,
                        //CompanyName = record.CompanyName?.ToUpper(),
                        CountryDestination = record.CountryDestination?.ToUpper(),
                        NameOfRepresentative = $"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}",
                        RepresentativeContactNumber = model.Record.ContactNumber,
                        ApostileData = record.ApostileData,
                        ProcessingSite = model.Record.ProcessingSite?.ToUpper(),
                        ProcessingSiteAddress = model.Record.ProcessingSiteAddress?.ToUpper(),
                        ScheduleDate = DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt",
                                           System.Globalization.CultureInfo.InvariantCulture),
                        ApplicationCode = record.ApplicationCode, //record.ApplicationCode,
                        CreatedBy = new Guid(_userManager.GetUserId(User)),
                        Fees = record.Fees,
                        Type = 1
                    };

                    applicantRecords.Add(applicantRecord);
                    attachments.Add(new Attachment("DFA-Application.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));
                }
                
            };
            
            var result = _applicantRepo.AddRange(applicantRecords);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            }
            //var name = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            //var attachment = new Attachment("DFA-Application.pdf", await GeneratePDF(record), new MimeKit.ContentType("application", "pdf"));

            await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File",
                    $"Download the attachment and present to the selected branch.",
                    attachments.ToArray());
            ViewData["ApplicantCount"] = model.ApplicantCount;
            return RedirectToAction("Success");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index(IEnumerable<ApplicantRecordViewModel> records, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    var attachments = new List<Attachment>();

        //    foreach (var record in records)
        //    {
        //        var applicantRecord = new ApplicantRecord
        //        {
        //            Title = record.Title?.ToUpper(),
        //            FirstName = record.FirstName?.ToUpper(),
        //            MiddleName = record.MiddleName?.ToUpper(),
        //            LastName = record.LastName?.ToUpper(),
        //            Suffix = record.Suffix?.ToUpper(),
        //            Address = $"{record.Barangay?.ToUpper()} {record.City?.ToUpper()} {record.Region?.ToUpper()} ",
        //            Nationality = record.Nationality?.ToUpper(),
        //            ContactNumber = record.ContactNumber,
        //            CompanyName = record.CompanyName?.ToUpper(),
        //            CountryDestination = record.CountryDestination?.ToUpper(),
        //            NameOfRepresentative = record.NameOfRepresentative?.ToUpper(),
        //            RepresentativeContactNumber = record.RepresentativeContactNumber?.ToUpper(),
        //            ApostileData = record.ApostileData,
        //            ProcessingSite = record.ProcessingSite?.ToUpper(),
        //            ProcessingSiteAddress = record.ProcessingSiteAddress?.ToUpper(),
        //            ScheduleDate = DateTime.ParseExact(record.ScheduleDate, "MM/dd/yyyy hh:mm tt",
        //                               System.Globalization.CultureInfo.InvariantCulture),
        //            ApplicationCode = record.ApplicationCode,
        //            CreatedBy = new Guid(_userManager.GetUserId(User)),
        //            Fees = record.Fees
        //        };

        //        var result = _applicantRepo.Add(applicantRecord);
        //        if (!result)
        //        {
        //            ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
        //        }

        //        attachments.Add(new Attachment("DFA-Application.pdf", await GeneratePDF(record), new MimeKit.ContentType("application", "pdf")));
        //    }

        //    await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File",
        //                $"Download the attachment and present to the selected branch.",
        //                attachments.ToArray());

        //    return RedirectToAction("Success");
        //}


        public IActionResult Authorized()
        {
            var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            ViewData["AvailableDates"] = stringify;
            ViewData["ApplicationCode"] = GetApplicantCode();
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Authorized(ApplicantsViewModel record, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            return RedirectToAction("Success");
        }

        public IActionResult PartialApplicant(int i)
        {
            var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            //ViewData[$"ApplicationCode{i}"] = applicationCode + (i + 1);
            ViewBag.Increment = i;
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Admin()
        {
            return View();
        }

        public IActionResult Success()
        {
            return View();
        }

        public static string GetApplicantCode()
        {
            int length = 4;
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            Random r = new Random();
            var date = DateTime.Now;
            var applicantCode = $"{date.ToString("hhmmss")}-{date.ToString("ddd").Substring(0, 2)}{result}-{date.ToString("MMdd")}".ToUpper();

            return applicantCode;
        }


        public async Task<MemoryStream> GeneratePDF(ApplicantRecord model)
        {
            var header = _env.WebRootFileProvider.GetFileInfo("header2.html")?.PhysicalPath;
            var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                HeaderHtml = header,
                FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 40,
                    Bottom = 20,
                    Right = 15,
                    Left = 15
                }
            };
            _generatePdf.SetConvertOptions(options);

            //var data = new TestData
            //{
            //    Text = "This is a test",
            //    Number = 123456
            //};

            var pdf = await _generatePdf.GetByteArray("Views/TestBootstrapSSL.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return pdfStream;
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

        [AllowAnonymous]
        public ActionResult GetCity(string municipality)
        {
            var result = _applicantRepo.GetCity().Where(a => a.municipality.Equals(municipality)).Select(a => a.city).ToList();
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult GetMunicipality(string city)
        {
            var result = _applicantRepo.GetCity().Where(a => a.city == city).Select(a => a.municipality).ToList();//_applicantRepo.GetUnAvailableDates();
            return Json(result);
        }
        
        public int SetNumberOfApplicants(int applicantsCount)
        {
            //var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            //ViewData["AvailableDates"] = stringify;
            //ViewData["ApplicationCode"] = GetApplicantCode();
            //ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            ViewData["ApplicationCount"] = applicantsCount;
            return (int)ViewData["ApplicationCount"];
        }

    }
}
