using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DFACore.Data;
using DFACore.Helpers;
using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using shortid;
using shortid.Configuration;
using Shyjus.BrowserDetection;
using UnionBankPayment;
using Wkhtmltopdf.NetCore;

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
        private readonly GoogleCaptchaService _googleCaptchaService;
        private readonly IActionContextAccessor _accessor;
        private readonly IBrowserDetector _browserDetector;
        private readonly DocumentTypes _documentsType;
        private readonly AdministrationRepository _administrationRepository;
        private readonly UnionBankPaymentClient _unionBankPaymentService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicantRecordRepository applicantRepo,
            IMessageService messageService,
            IGeneratePdf generatePdf,
            IWebHostEnvironment env,
            GoogleCaptchaService googleCaptchaService,
            IActionContextAccessor accessor,
            IBrowserDetector browserDetector,
            AdministrationRepository administrationRepository,
            UnionBankPaymentClient unionBankPaymentService,
            IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _applicantRepo = applicantRepo;
            _messageService = messageService;
            _generatePdf = generatePdf;
            _env = env;
            _googleCaptchaService = googleCaptchaService;
            _accessor = accessor;
            _browserDetector = browserDetector;
            _documentsType = new DocumentTypes();
            _administrationRepository = administrationRepository;
            _unionBankPaymentService = unionBankPaymentService;
            _configuration = configuration;
        }
        public IActionResult ApplicantTypeSelection()
        {
            return View();
        }

        public IActionResult Initial()
        {
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Index(int applicantsCount = 0, int id = 0)
        {
            if (applicantsCount > 10)
                applicantsCount = 10;

            var defaultBranch = _applicantRepo.GetBranch("DFA - OCA (ASEANA)");
            //var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now, defaultBranch.Id));

            ViewData["AvailableDates"] = defaultBranch.AvailableDates; //stringify;
            ViewData["ApplicationCode"] = GetApplicantCode();
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            ViewData["ApplicantCount"] = applicantsCount;
            //ViewBag.User = await _userManager.GetUserAsync(HttpContext.User);
            ViewData["DefaultBranch"] = defaultBranch;

            var branches = _applicantRepo.GetBranches();

            if (id == 0)
            {
                branches = branches.Where(a => a.Id == 1).ToList();
            }

            ViewData["Branches"] = branches;

            ViewData["Price"] = _applicantRepo.GetPrice();
            ViewData["TermsAndConditionsMessage"] = _applicantRepo.GetNotice(3);

            var documents = _documentsType.Get();

            if (id == 1)
            {
                var phEmbassy = documents.FirstOrDefault(x => x.Id == "phEmbassy");
                var foreignEmbassy = documents.FirstOrDefault(x => x.Id == "foreignEmbassy");
                documents.Remove(phEmbassy);
                documents.Remove(foreignEmbassy);

            }
            else
            {
                documents = documents.Where(a => (a.Id == "phEmbassy" || a.Id == "foreignEmbassy")).ToList();
            }
            ViewData["DocumentTypes"] = documents;
            ViewBag.Location = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ApplicantsViewModel model, string returnUrl = null)
        {
            //var googleReCaptcha = _googleCaptchaService.VerifyReCaptcha(model.Token);
            //if (!googleReCaptcha.Result.success && googleReCaptcha.Result.score <= 0.5)
            //{
            //    throw new Exception("Invalid attempt.");
            //    //ModelState.AddModelError(string.Empty, "Invalid attempt.");
            //    //return View();
            //}




            var applicantRecords = new List<ApplicantRecord>();
            var attachments = new List<Attachment>();

            bool generatePowerOfAttorney = false;
            bool generateAuthLetter = false;

            var dateTimeSched = DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            var isSchedExist = _applicantRepo.CheckIfSchedExistInHoliday(dateTimeSched);

            if (isSchedExist)
            {
                ViewBag.errorMessage = $"The schedule {model.ScheduleDate} you have selected is not available. Please select another date and time slot. Thank you!";
                return View("Error");
            }

            var branch = _applicantRepo.GetBranch(model.Record.ProcessingSite);

            List<string> apptCode = new List<string>();

            var total = 0;
            if (model.Records == null)
            {
                //if (model.Record.ApostileData == "[]")
                //{
                //    return RedirectToAction("Error");
                //}

                //var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(model.Record.ApostileData);
                //var validate = ValidateScheduleDate3(model.ScheduleDate, data.Count());
                //if (validate)
                //{
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
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
                    ScheduleDate = dateTimeSched, //DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                    ApplicationCode = model.Record.ApplicationCode,
                    CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Record.Fees,
                    Type = 0,
                    DateCreated = DateTime.UtcNow,
                    QRCode = _applicantRepo.GenerateQRCode($"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}" +
                        $"{Environment.NewLine}{model.Record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                        $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                };
                applicantRecords.Add(applicantRecord);
                attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;
                //}
                //else
                //{
                //    return RedirectToAction("Error");
                //}
                var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                total = data.Sum(a => a.Quantity);
                apptCode.Add(model.Record.ApplicationCode);
            }
            else
            {
                foreach (var record in model.Records)
                {
                    //if (record.ApostileData == "[]")
                    //{
                    //    return RedirectToAction("Error");
                    //}

                    //data += JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(record.ApostileData).Count;



                    var applicantRecord = new ApplicantRecord
                    {
                        BranchId = branch != null ? branch.Id : 0,
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
                        ScheduleDate = dateTimeSched,
                        ApplicationCode = record.ApplicationCode, //record.ApplicationCode,
                        CreatedBy = new Guid(_userManager.GetUserId(User)),
                        Fees = record.Fees,
                        Type = 1,
                        DateCreated = DateTime.UtcNow,
                        QRCode = _applicantRepo.GenerateQRCode($"{record.FirstName?.ToUpper()} {record.MiddleName?.ToUpper()} {record.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                    };

                    applicantRecords.Add(applicantRecord);
                    attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                    var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                    if (age < 18)
                        generatePowerOfAttorney = true;

                    generateAuthLetter = true;

                    var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                    total += data.Sum(a => a.Quantity);

                    apptCode.Add(record.ApplicationCode);
                }




            };

            var validate = ValidateScheduleDate3(model.ScheduleDate, total, branch.Id);
            if (!validate)
            {
                ViewBag.errorMessage = $"The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!";
                return View("Error");
            }

            //

            var result = _applicantRepo.AddRange(applicantRecords);
            if (!result)
            {
                Log("Generate appointment but an error occured while saving data.", User.Identity.Name);
                return RedirectToAction("Error");  //ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            }
            //var name = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            //var attachment = new Attachment("DFA-Application.pdf", await GeneratePDF(record), new MimeKit.ContentType("application", "pdf"));

            if (generatePowerOfAttorney)
            {
                attachments.Add(new Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            if (generateAuthLetter)
            {
                attachments.Add(new Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }


            await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File", //$"<p><bold>Download the attachment and present to the selected branch.</bold></p>",
                    HtmlTemplate(),
                    attachments.ToArray());
            ViewData["ApplicantCount"] = model.ApplicantCount;
            Log($"Generate appointment successfully with code of {string.Join(",", apptCode)} .", User.Identity.Name);
            return RedirectToAction("Success");

        }

        //[HttpPost]
        //[
        //AntiForgeryToken]
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


        //public IActionResult Authorized()
        //{
        //    var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
        //    ViewData["AvailableDates"] = stringify;
        //    ViewData["ApplicationCode"] = GetApplicantCode();
        //    ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Authorized(ApplicantsViewModel record, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    return RedirectToAction("Success");
        //}

        public IActionResult PartialApplicant(int i, int id = 0)
        {
            //var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();
            //ViewData[$"ApplicationCode{i}"] = applicationCode + (i + 1);
            ViewBag.Increment = i;

            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            var documents = _documentsType.Get();

            if (main.DocumentStatus == "Abroad")
            {
                ViewBag.Location = 1;
                var phEmbassy = documents.FirstOrDefault(x => x.Id == "phEmbassy");
                var foreignEmbassy = documents.FirstOrDefault(x => x.Id == "foreignEmbassy");
                documents.Remove(phEmbassy);
                documents.Remove(foreignEmbassy);
            }
            else
            {
                ViewBag.Location = 0;
                documents = documents.Where(a => (a.Id == "phEmbassy" || a.Id == "foreignEmbassy")).ToList();
            }

            ViewData["DocumentTypes"] = documents;
            return PartialView("PartialApplicant");
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
            var options = new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 8
            };
            string id = ShortId.Generate(options);

            //int length = 4;
            //var random = new Random();
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            //var result = new string(
            //    Enumerable.Repeat(chars, length)
            //              .Select(s => s[random.Next(s.Length)])
            //              .ToArray());

            //Random r = new Random();
            var date = DateTime.Now;
            var applicantCode = $"{date.ToString("hhmmss")}-{date.ToString("yy").Substring(0, 2)}{id}-{date.ToString("MMdd")}".ToUpper();

            return applicantCode;
        }



        private string AddtnlCode(ApplicantRecord model)
        {
            var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(model.ApostileData);

            var addtnlCode = $"{data.Sum(a => a.Quantity)}-{model.BranchId}{model.ScheduleDate.ToString("HH")}{model.CountryDestination.Length}-" +
                $"{model.DateCreated.ToString("dd")}{model.DateCreated.ToString("MM")}{model.DateCreated.ToString("yy")}" +
                $"{model.DateCreated.ToString("HH")}{model.DateCreated.ToString("mm")}-{model.FirstName.Length}";
            return addtnlCode;
        }

        public async Task<MemoryStream> GeneratePDF(ApplicantRecord model)
        {

            model.AdditionalCode = AddtnlCode(model);
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

        public async Task<MemoryStream> GeneratePowerOfAttorneyPDF(TestData data)
        {
            //var header = _env.WebRootFileProvider.GetFileInfo("header2.html")?.PhysicalPath;
            //var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                //HeaderHtml = header,
                //FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 9,
                    Bottom = 9,
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

            var pdf = await _generatePdf.GetByteArray("Views/PowerOfAttorney.cshtml", data);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return pdfStream;
        }

        public async Task<MemoryStream> GenerateAuthorizationLetterPDF(TestData data)
        {
            //var header = _env.WebRootFileProvider.GetFileInfo("header2.html")?.PhysicalPath;
            //var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                //HeaderHtml = header,
                //FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 15,
                    Bottom = 15,
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

            var pdf = await _generatePdf.GetByteArray("Views/AuthorizationLetter.cshtml", data);
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

        public ActionResult ValidateScheduleDate2(string scheduleDate, int applicationCount, long branchId)
        {

            var date = DateTime.ParseExact(scheduleDate, "MM/dd/yyyy hh:mm tt",
                                       System.Globalization.CultureInfo.InvariantCulture);

            var result = _applicantRepo.ValidateScheduleDate(date, applicationCount, branchId);

            return Json(result);
        }

        public bool ValidateScheduleDate3(string scheduleDate, int applicationCount, long branchId)
        {

            var date = DateTime.ParseExact(scheduleDate, "MM/dd/yyyy hh:mm tt",
                                       System.Globalization.CultureInfo.InvariantCulture);

            var result = _applicantRepo.ValidateScheduleDate(date, applicationCount, branchId);

            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public ActionResult Test()
        {
            //var result = _applicantRepo.GenerateListOfDates(DateTime.Now);//_applicantRepo.GetUnAvailableDates();
            return Json("");
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

        [AllowAnonymous]
        public ActionResult GetBranches()
        {
            var result = _applicantRepo.GetBranches();
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult GetBranch(string branch)
        {
            var result = _applicantRepo.GetBranch(branch);
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult GetAvailableDatesByBranch(string branch)
        {
            //var stringify = JsonConvert.SerializeObject(_applicantRepo.GenerateListOfDates(DateTime.Now));
            return Json("");
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

        [AllowAnonymous]
        public async Task<IActionResult> PDFViewer()
        {

            var options = new ConvertOptions
            {
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 15,
                    Bottom = 15,
                    Right = 15,
                    Left = 15
                }
            };
            _generatePdf.SetConvertOptions(options);

            var data = new TestData
            {
                Text = "This is a test",
                Number = 123456
            };

            var pdf = await _generatePdf.GetByteArray("Views/AuthorizationLetter.cshtml", data);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        public string HtmlTemplate()
        {
            using (StreamReader SourceReader = System.IO.File.OpenText(_env.WebRootFileProvider.GetFileInfo("template.html")?.PhysicalPath))
            {

                return SourceReader.ReadToEnd();
            }

        }

        [AllowAnonymous]
        public string GetIP()
        {
            var ip = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            var ip2 = HttpContext.Connection.RemoteIpAddress.ToString();

            GetUserCountryByIp("136.158.28.217");
            var browser = _browserDetector.Browser;
            return ip;
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

        public void Log(string data, string email = null)
        {
            var ip = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            var browser = _browserDetector.Browser;
            if (browser != null)
            {
                var activity = new ActivityLog
                {
                    CreatedDate = DateTime.Now,
                    IpAddress = ip,
                    Browser = $"{browser.Name} {browser.Version}",
                    OS = browser.OS,
                    DeviceType = browser.DeviceType,
                    Remarks = data,
                    Email = email
                };
                _applicantRepo.AddActivityLog(activity);
            }
        }

        [AllowAnonymous]
        [Route("/")]
        public ActionResult Dashboard()
        {
            return View();
        }

        public IActionResult DocumentLocation()
        {
            return View();
        }

        public List<Documents> GetDocuments()
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            var documents = _documentsType.Get();

            if (main.DocumentStatus == "Abroad")
            {
                ViewBag.Location = 1;
                var phEmbassy = documents.FirstOrDefault(x => x.Id == "phEmbassy");
                var foreignEmbassy = documents.FirstOrDefault(x => x.Id == "foreignEmbassy");
                documents.Remove(phEmbassy);
                documents.Remove(foreignEmbassy);
            }
            else
            {
                ViewBag.Location = 0;
                documents = documents.Where(a => (a.Id == "phEmbassy" || a.Id == "foreignEmbassy")).ToList();
            }

            return documents;
        }
        public Price GetPrices()
        {
            return _applicantRepo.GetPrice();
        }

        [HttpPost]
        public async Task<IActionResult> PostApplication(ApplicantsViewModel model, string returnUrl = null)
        {
            var applicantRecords = new List<ApplicantRecord>();
            var attachments = new List<Attachment>();

            bool generatePowerOfAttorney = false;
            bool generateAuthLetter = false;

            var dateTimeSched = DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            var isSchedExist = _applicantRepo.CheckIfSchedExistInHoliday(dateTimeSched);

            if (isSchedExist)
            {
                ViewBag.errorMessage = $"The schedule {model.ScheduleDate} you have selected is not available. Please select another date and time slot. Thank you!";
                return View("Error");
            }

            var branch = _applicantRepo.GetBranch(model.Record.ProcessingSite);

            List<string> apptCode = new List<string>();

            var total = 0;
            if (model.Records == null || model.Records.Count == 0)
            {
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
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
                    ScheduleDate = dateTimeSched, //DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                    ApplicationCode = model.Record.ApplicationCode,
                    CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Record.Fees,
                    Type = 0,
                    DateCreated = DateTime.Now,
                    QRCode = _applicantRepo.GenerateQRCode($"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}" +
                        $"{Environment.NewLine}{model.Record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                        $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                };
                applicantRecords.Add(applicantRecord);
                attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;

                var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(model.Record.ApostileData);
                total = data.Sum(a => a.Quantity);

                apptCode.Add(model.Record.ApplicationCode);
            }
            else
            {
                foreach (var record in model.Records)
                {
                    var applicantRecord = new ApplicantRecord
                    {
                        BranchId = branch != null ? branch.Id : 0,
                        FirstName = record.FirstName?.ToUpper(),
                        MiddleName = record.MiddleName?.ToUpper(),
                        LastName = record.LastName?.ToUpper(),
                        Suffix = record.Suffix?.ToUpper(),
                        DateOfBirth = record.DateOfBirth,
                        ContactNumber = record.ContactNumber,
                        CountryDestination = record.CountryDestination?.ToUpper(),
                        NameOfRepresentative = $"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}",
                        RepresentativeContactNumber = model.Record.ContactNumber,
                        ApostileData = record.ApostileData,
                        ProcessingSite = model.Record.ProcessingSite?.ToUpper(),
                        ProcessingSiteAddress = model.Record.ProcessingSiteAddress?.ToUpper(),
                        ScheduleDate = dateTimeSched,
                        ApplicationCode = record.ApplicationCode, //record.ApplicationCode,
                        CreatedBy = new Guid(_userManager.GetUserId(User)),
                        Fees = record.Fees,
                        Type = 1,
                        DateCreated = DateTime.Now,
                        QRCode = _applicantRepo.GenerateQRCode($"{record.FirstName?.ToUpper()} {record.MiddleName?.ToUpper()} {record.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                    };

                    applicantRecords.Add(applicantRecord);
                    attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                    var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                    if (age < 18)
                        generatePowerOfAttorney = true;

                    generateAuthLetter = true;

                    var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                    total += data.Sum(a => a.Quantity);

                    apptCode.Add(record.ApplicationCode);
                }
            };

            var validate = ValidateScheduleDate3(model.ScheduleDate, total, branch.Id);
            if (!validate)
            {
                ViewBag.errorMessage = $"The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!";
                return Json(new { Status = "Error", Message = "The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!" });
            }

            var result = _applicantRepo.AddRange(applicantRecords);
            if (!result)
            {
                Log("Generate appointment but an error occured while saving data.", User.Identity.Name);
                return Json(new { Status = "Error", Message = "" });  //ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            }

            if (generatePowerOfAttorney)
            {
                attachments.Add(new Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            if (generateAuthLetter)
            {
                attachments.Add(new Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File", //$"<p><bold>Download the attachment and present to the selected branch.</bold></p>",
                    HtmlTemplate(),
                    attachments.ToArray());
            ViewData["ApplicantCount"] = model.ApplicantCount;
            ViewData["Attachments"] = attachments;
            ViewData["ApptCode"] = apptCode;

            Log($"Generate appointment successfully with code of {string.Join(",", apptCode)} .", User.Identity.Name);
            return Json(new { Status = "Success", Message = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmail(ApplicantsViewModel model)
        {
            var applicantRecords = new List<ApplicantRecord>();
            var attachments = new List<Attachment>();

            bool generatePowerOfAttorney = false;
            bool generateAuthLetter = false;

            var dateTimeSched = DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            var branch = _applicantRepo.GetBranch(model.Record.ProcessingSite);

            List<string> apptCode = new List<string>();

            var total = 0;
            if (model.Records == null || model.Records.Count == 0)
            {
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
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
                    ScheduleDate = dateTimeSched, //DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                    ApplicationCode = model.Record.ApplicationCode,
                    CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Record.Fees,
                    Type = 0,
                    DateCreated = DateTime.UtcNow,
                    QRCode = _applicantRepo.GenerateQRCode($"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}" +
                        $"{Environment.NewLine}{model.Record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                        $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                };
                applicantRecords.Add(applicantRecord);
                attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;
                var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                total = data.Sum(a => a.Quantity);
                apptCode.Add(model.Record.ApplicationCode);
            }
            else
            {
                foreach (var record in model.Records)
                {
                    var applicantRecord = new ApplicantRecord
                    {
                        BranchId = branch != null ? branch.Id : 0,
                        FirstName = record.FirstName?.ToUpper(),
                        MiddleName = record.MiddleName?.ToUpper(),
                        LastName = record.LastName?.ToUpper(),
                        Suffix = record.Suffix?.ToUpper(),
                        DateOfBirth = record.DateOfBirth,
                        ContactNumber = record.ContactNumber,
                        CountryDestination = record.CountryDestination?.ToUpper(),
                        NameOfRepresentative = $"{model.Record.FirstName?.ToUpper()} {model.Record.MiddleName?.ToUpper()} {model.Record.LastName?.ToUpper()}",
                        RepresentativeContactNumber = model.Record.ContactNumber,
                        ApostileData = record.ApostileData,
                        ProcessingSite = model.Record.ProcessingSite?.ToUpper(),
                        ProcessingSiteAddress = model.Record.ProcessingSiteAddress?.ToUpper(),
                        ScheduleDate = dateTimeSched,
                        ApplicationCode = record.ApplicationCode, //record.ApplicationCode,
                        CreatedBy = new Guid(_userManager.GetUserId(User)),
                        Fees = record.Fees,
                        Type = 1,
                        DateCreated = DateTime.UtcNow,
                        QRCode = _applicantRepo.GenerateQRCode($"{record.FirstName?.ToUpper()} {record.MiddleName?.ToUpper()} {record.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.Record.ProcessingSite?.ToUpper()}")
                    };

                    applicantRecords.Add(applicantRecord);
                    attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                    var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                    if (age < 18)
                        generatePowerOfAttorney = true;

                    generateAuthLetter = true;

                    var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                    total += data.Sum(a => a.Quantity);

                    apptCode.Add(record.ApplicationCode);
                }
            };

            var validate = ValidateScheduleDate3(model.ScheduleDate, total, branch.Id);
            if (!validate)
            {
                ViewBag.errorMessage = $"The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!";
                return View("Error");
            }

            //var result = _applicantRepo.AddRange(applicantRecords);
            //if (!result)
            //{
            //    Log("Generate appointment but an error occured while saving data.", User.Identity.Name);
            //    return RedirectToAction("Error");  //ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            //}

            if (generatePowerOfAttorney)
            {
                attachments.Add(new Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            if (generateAuthLetter)
            {
                attachments.Add(new Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File", //$"<p><bold>Download the attachment and present to the selected branch.</bold></p>",
                    HtmlTemplate(),
                    attachments.ToArray());
            ViewData["ApplicantCount"] = model.ApplicantCount;
            ViewData["Attachments"] = attachments;
            ViewData["ApptCode"] = apptCode;

            Log($"Generate appointment successfully with code of {string.Join(",", apptCode)} .", User.Identity.Name);
            return Json("Success");
        }

        [AllowAnonymous]
        public ActionResult LoginOptions()
        {
            return View();
        }

        public ActionResult Cancellation()
        {
            ViewData["NoticeMessage"] = _applicantRepo.GetNotice(1);
            return View();
        }

        [HttpPost]
        public IActionResult ValidateAppointment(string code)
        {
            var createdBy = new Guid(_userManager.GetUserId(User));

            //check here if appointment alr cancelled.

            var cancelled = _administrationRepository.GetCancelledApplicantRecord(createdBy, code).ToList();

            if (cancelled.Any())
            {
                return Json(new { Status = "Error", ErrorMessage = "", Message = $"This appointment code was already cancelled on ", date = $"{cancelled.FirstOrDefault().ScheduleDate.ToString("MMMM dd, yyyy hh:mm tt")}" });
            }

            var record = _administrationRepository.GetApplicantRecord(createdBy, code);
            if (record is null)
            {
                return Json(new { Status = "Error", ErrorMessage = "Appointment not found", Message = "Please double-check the Appointment Code.", date = "" });
            }

            if (record.ScheduleDate.Date == DateTime.Now.Date)
            {
                return Json(new { Status = "Error", ErrorMessage = "Appointments cannot be cancelled on the scheduled date itself. Previous appointments cannot be cancelled anymore.", Message = "", date = "" }); //DateTime.Now.ToShortDateString()
            }

            if (record.ScheduleDate <= DateTime.Now)
            {
                return Json(new { Status = "Error", ErrorMessage = "Appointment has expired.", Message = "Please double-check the Appointment Code.", date = "" });
            }

            return Json(new { Status = "Success", Data = record });

        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(string code)
        {
            var success = await _administrationRepository.CancelApplication($"'{code}'");
            if (success)
            {
                return Json(new { Status = "Success" });
            }
            else
            {
                return Json(new { Status = "Error", ErrorMessage = "There is a problem encountered while processing request. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendCancellationEmail(ApplicantRecord record)
        {
            var success = await _administrationRepository.ResendCancellationEmail(record);
            if (success)
            {
                return Json(new { Status = "Success" });
            }
            else
            {
                return Json(new { Status = "Error", ErrorMessage = "There is a problem encountered while processing request. Please try again." });
            }
        }

        public ActionResult CourierService()
        {
            return View();
        }



        public ActionResult SiteSelection()
        {

            var defaultBranch = _applicantRepo.GetBranch("DFA - OCA (ASEANA)");

            ViewData["AvailableDates"] = defaultBranch.AvailableDates; //stringify;
            ViewData["ApplicationCode"] = GetApplicantCode();
            ViewData["GetMunicipality"] = _applicantRepo.GetCity().Select(a => a.municipality).Distinct().ToList();

            ViewData["DefaultBranch"] = defaultBranch;

            var branches = _applicantRepo.GetBranches();

            ViewData["Branches"] = branches;

            var siteSelection = TempData["Review"] as SiteSelectionViewModel;

            return View(siteSelection);
        }

        public ActionResult PersonalInfo()
        {
            var documents = _documentsType.Get();
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");

            if (main.DocumentStatus == "Abroad")
            {
                ViewBag.Location = 1;
                var phEmbassy = documents.FirstOrDefault(x => x.Id == "phEmbassy");
                var foreignEmbassy = documents.FirstOrDefault(x => x.Id == "foreignEmbassy");
                documents.Remove(phEmbassy);
                documents.Remove(foreignEmbassy);

            }
            else
            {
                ViewBag.Location = 0;
                documents = documents.Where(a => (a.Id == "phEmbassy" || a.Id == "foreignEmbassy")).ToList();
            }

            ViewBag.DocumentType = main.DocumentType;
            ViewData["DocumentTypes"] = documents;

            return View();
        }

        [HttpPost]
        public ActionResult PersonalInfo(SiteSelectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["SiteSelection"] = model;
                return RedirectToAction("SiteSelection");
            }
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            var branches = _applicantRepo.GetBranches();
            var selectedBranch = branches.FirstOrDefault(x => x.Id == long.Parse(model.ApostileSite));

            main.DocumentStatus = model.DocumentStatus;
            main.DocumentType = model.DocumentType;
            main.ApostileSite = model.ApostileSite;
            main.ProcessingSite = selectedBranch.BranchName;
            main.ProcessingSiteAddress = selectedBranch.BranchAddress;

            HttpContext.Session.SetComplexData("Model", main);

            var documents = _documentsType.Get();

            if (model.DocumentStatus == "Abroad")
            {
                ViewBag.Location = 1;
                var phEmbassy = documents.FirstOrDefault(x => x.Id == "phEmbassy");
                var foreignEmbassy = documents.FirstOrDefault(x => x.Id == "foreignEmbassy");
                documents.Remove(phEmbassy);
                documents.Remove(foreignEmbassy);
                ViewData["DocumentTypes"] = documents;
            }
            else
            {
                ViewBag.Location = 0;
                documents = documents.Where(a => (a.Id == "phEmbassy" || a.Id == "foreignEmbassy")).ToList();
                ViewData["DocumentTypes"] = documents;
            }


            return RedirectToAction("PersonalInfo");
        }

        public ActionResult ShippingInformation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ShippingInformation(List<ApplicantRecordViewModel> model)
        {

            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            main.Applicants = model;

            HttpContext.Session.SetComplexData("Model", main);

            return RedirectToAction("ShippingInformation");
        }


        public ActionResult ApostilleSchedule()
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");

            var user = _userManager.Users.Where(a => a.Email == User.Identity.Name).FirstOrDefault();

            // 0 for user, 2 for LRA
            var selectedBranch = _applicantRepo.GetBranch(main.ApostileSite, user.Type);

            ViewData["SelectedBranch"] = selectedBranch;
            ViewData["AvailableDates"] = selectedBranch.AvailableDates;

            return View();
        }

        //[HttpPost]
        //public ActionResult ApostilleSchedule(ShippingInfoViewModel model)
        //{
        //    var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
        //    main.Shipping = model;

        //    //var defaultBranch = _applicantRepo.GetBranch("DFA - OCA (ASEANA)");

        //    //ViewData["DefaultBranch"] = defaultBranch;

        //    HttpContext.Session.SetComplexData("Model", main);

        //    return RedirectToAction("ApostilleSchedule");
        //    //return View();
        //}

        [HttpPost]
        public ActionResult ApostilleSchedule(List<ApplicantRecordViewModel> model)
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            main.Applicants = model;

            if (main.DocumentType == "Authorized")
            {
                main.NameOfRepresentative = model.FirstOrDefault().NameOfRepresentative;
                main.RepresentativeContactNumber = model.FirstOrDefault().RepresentativeContactNumber;
            }

            HttpContext.Session.SetComplexData("Model", main);

            return RedirectToAction("ApostilleSchedule");
            //return View();
        }

        [AllowAnonymous]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ContactUs()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FAQs()
        {
            return View();
        }

        public ActionResult ApplicationSummary()
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            return View(main);
        }

        [HttpPost]
        public ActionResult ApplicationSummary(ApostilleScheduleViewModel model)
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            main.ScheduleDate = model.ScheduleDate;
            HttpContext.Session.SetComplexData("Model", main);


            return RedirectToAction("ApplicationSummary");
        }

        public ActionResult PaymentMethod()
        {
            var url = _configuration.GetConnectionString("ReturnUrl");
            ViewBag.ReturnUrl = url;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PaymentMethod(string returnUrl = null)
        {
            var applicantRecords = new List<ApplicantRecord>();
            var attachments = new List<Attachment>();

            bool generatePowerOfAttorney = false;
            bool generateAuthLetter = false;

            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");

            var dateTimeSched = DateTime.ParseExact(main.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            var isSchedExist = _applicantRepo.CheckIfSchedExistInHoliday(dateTimeSched);

            if (isSchedExist)
            {
                ViewBag.errorMessage = $"The schedule {main.ScheduleDate} you have selected is not available. Please select another date and time slot. Thank you!";
                return View("Error");
            }

            var branch = _applicantRepo.GetBranch(main.ProcessingSite);

            List<string> apptCode = new List<string>();

            var total = 0;
            if (main.DocumentType == "DocumentOwner")
            {
                var appDocOwner = main.Applicants.First();
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
                    FirstName = appDocOwner.FirstName?.ToUpper(),
                    MiddleName = appDocOwner.MiddleName?.ToUpper(),
                    LastName = appDocOwner.LastName?.ToUpper(),
                    Suffix = appDocOwner.Suffix?.ToUpper(),
                    DateOfBirth = appDocOwner.DateOfBirth,
                    ContactNumber = appDocOwner.ContactNumber,
                    CountryDestination = appDocOwner.CountryDestination?.ToUpper(),
                    ApostileData = appDocOwner.ApostileData,
                    ProcessingSite = main.ProcessingSite?.ToUpper(),
                    ProcessingSiteAddress = main.ProcessingSiteAddress?.ToUpper(),
                    ScheduleDate = dateTimeSched, 
                    ApplicationCode = appDocOwner.ApplicationCode,
                    CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = appDocOwner.Fees,
                    Type = 0,
                    DateCreated = DateTime.Now,
                    QRCode = _applicantRepo.GenerateQRCode($"{appDocOwner.FirstName?.ToUpper()} {appDocOwner.MiddleName?.ToUpper()} {appDocOwner.LastName?.ToUpper()}" +
                        $"{Environment.NewLine}{appDocOwner.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                        $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{main.ProcessingSite?.ToUpper()}")
                };
                applicantRecords.Add(applicantRecord);

                //attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;

                var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                total = data.Sum(a => a.Quantity);

                apptCode.Add(applicantRecord.ApplicationCode);
            }
            else
            {
                foreach (var record in main.Applicants)
                {
                    var applicantRecord = new ApplicantRecord
                    {
                        BranchId = branch != null ? branch.Id : 0,
                        FirstName = record.FirstName?.ToUpper(),
                        MiddleName = record.MiddleName?.ToUpper(),
                        LastName = record.LastName?.ToUpper(),
                        Suffix = record.Suffix?.ToUpper(),
                        DateOfBirth = record.DateOfBirth,
                        ContactNumber = record.ContactNumber,
                        CountryDestination = record.CountryDestination?.ToUpper(),
                        NameOfRepresentative = $"{main.Applicants.First().FirstName?.ToUpper()} {main.Applicants.First().MiddleName?.ToUpper()} {main.Applicants.First().LastName?.ToUpper()}",
                        RepresentativeContactNumber = main.Applicants.First().ContactNumber,
                        ApostileData = record.ApostileData,
                        ProcessingSite = main.ProcessingSite?.ToUpper(),
                        ProcessingSiteAddress = main.ProcessingSiteAddress?.ToUpper(),
                        ScheduleDate = dateTimeSched,
                        ApplicationCode = record.ApplicationCode, //record.ApplicationCode,
                        CreatedBy = new Guid(_userManager.GetUserId(User)),
                        Fees = record.Fees,
                        Type = 1,
                        DateCreated = DateTime.Now,
                        QRCode = _applicantRepo.GenerateQRCode($"{record.FirstName?.ToUpper()} {record.MiddleName?.ToUpper()} {record.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{record.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{main.ProcessingSite?.ToUpper()}")
                    };

                    applicantRecords.Add(applicantRecord);
                    //attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                    var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                    if (age < 18)
                        generatePowerOfAttorney = true;

                    generateAuthLetter = true;

                    var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(applicantRecord.ApostileData);
                    total += data.Sum(a => a.Quantity);

                    apptCode.Add(record.ApplicationCode);
                }
            };

            var validate = ValidateScheduleDate3(main.ScheduleDate, total, branch.Id);
            if (!validate)
            {
                ViewBag.errorMessage = $"The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!";
                return Json(new { Status = "Error", Message = "The date and time slot you have selected is already filled-up. Please select another date and time slot. Thank you!" });
            }

            var result = _applicantRepo.AddRange(applicantRecords);
            if (!result)
            {
                Log("Generate appointment but an error occured while saving data.", User.Identity.Name);
                return Json(new { Status = "Error", Message = "" });  //ModelState.AddModelError(string.Empty, "An error has occured while saving the data.");
            }

            if (generatePowerOfAttorney)
            {
                //attachments.Add(new Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            if (generateAuthLetter)
            {
                //attachments.Add(new Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));
            }

            await _messageService.SendEmailAsync(User.Identity.Name, User.Identity.Name, "Application File", //$"<p><bold>Download the attachment and present to the selected branch.</bold></p>",
                    HtmlTemplate());
            //attachments.ToArray());
            //ViewData["ApplicantCount"] = model.ApplicantCount;
            //ViewData["Attachments"] = attachments;
            //ViewData["ApptCode"] = apptCode;

            Log($"Generate appointment successfully with code of {string.Join(",", apptCode)} .", User.Identity.Name);
            return Json(new { Status = "Success", Message = "" });
        }

        public async Task<ActionResult> OrderSummary()
        {
            //this should be success page
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            //var code = HttpContext.Request.Query.TryGetValue("code", out StringValues value); //validate
            //var token = await _unionBankPaymentService.GetAccessToken(value);

            //var model = new CreateCustomerPaymentParameter { SenderRefId = "test005"};
            //var result = await _unionBankPaymentService.CreateCustomerPayment( model, token);
            return View(main);
        }

        public ActionResult PaymentSuccess1()
        {
            return View();
        }

        public ActionResult PaymentFailed()
        {
            return View();
        }






        public ActionResult GetInfo()
        {
            var main = HttpContext.Session.GetComplexData<MainViewModel>("Model");
            var branches = _applicantRepo.GetBranches();
            var branch = branches.FirstOrDefault(x => x.Id == long.Parse(main.ApostileSite));
            main.HasExpedite = branch != null ? branch.HasExpidite : false;
            main.ApplicationCode = GetApplicantCode();
            HttpContext.Session.SetComplexData("Model", main);

            return Json(main);
        }
    }


}
