using ClosedXML.Excel;
using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore;

namespace DFACore.Controllers
{
    [Authorize(Roles = "Super Administrator, Administrator")]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly AdministrationRepository _administrationRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IGeneratePdf _generatePdf;

        public AdministrationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            AdministrationRepository administrationRepository,
            IWebHostEnvironment env,
            IGeneratePdf generatePdf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _administrationRepository = administrationRepository;
            _env = env;
            _generatePdf = generatePdf;
        }

        [Authorize(Roles = "Super Administrator, Administrator")]
        [HttpGet]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            ViewBag.Branches = _administrationRepository.GetBranches();

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            IQueryable<AdminApplicantRecordViewModel> applicants;
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                applicants = _administrationRepository.GetAllApplicantRecordByBranch(currentUser.BranchId, sortOrder, searchString);
            }
            else // (await _userManager.IsInRoleAsync(user, "Super Administrator"))
            {
                applicants = _administrationRepository.GetAllApplicantRecord(sortOrder, searchString);
            }

            int pageSize = 10;
            return View(await PaginatedList<AdminApplicantRecordViewModel>.CreateAsync(applicants, pageNumber ?? 1, pageSize));

            //return View(await applicants);
            //return View();
        }


        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            await LogOff();
            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
                return View();

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userManager.Users.Where(a => a.Email == model.Email).FirstOrDefault(); //.FindByEmailAsync(model.Email.Normalize().ToUpperInvariant());


            if (user != null)
            {
                if (user.Type != 1)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    await SendEmailConfirmationTokenAsync(user, "Email Verification");

                    ViewBag.errorMessage = "You must have a confirmed email to log on. "
                                         + "The confirmation token has been resent to your email account.";
                    return View("Error");
                }
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Administration");
            }

            if (result.IsNotAllowed)
            {
                ViewBag.errorMessage = "You must have a confirmed email to log on. "
                              + "The confirmation token has been resent to your email account.";
                return View("Error");
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Administration");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewBag.Roles = _roleManager.Roles;
            ViewBag.Branches = _administrationRepository.GetBranches();
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AdminRegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;


            if (ModelState.IsValid)
            {
                //Copy data from RegisterViewModel to ApplicationUser
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Suffix = model.Suffix,
                    PhoneNumber = model.PhoneNumber,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    UserName = model.Email,
                    Email = model.Email,
                    Type = 1,
                    CreatedDate = DateTime.Now,
                    EmailConfirmed = true,
                    BranchId = model.BranchId
                };

                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    var newAddedUser = _userManager.Users.Where(a => a.Email == model.Email).FirstOrDefault();
                    var role = await _roleManager.FindByIdAsync(model.Role);

                    if (role == null)
                    {
                        ViewBag.errorMessage = "Role cannot be found.";
                        return View("Error");
                    }

                    if (!await _userManager.IsInRoleAsync(newAddedUser, role.Name))
                    {
                        await _userManager.AddToRoleAsync(newAddedUser, role.Name);
                    }
                    string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");
                    return RedirectToAction("VerifyEmail", "Administration", new { email = user.Email });
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                        error.Description = $"Email '{model.Email}' is already taken.";
                    //if (error.Description.EndsWith("is already taken."))
                    //    error.Description = $"Email '{model.Email}' is already taken.";
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public ActionResult VerifyEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                ViewData["Email"] = email;
            }
            return View();

        }

        private async Task<string> SendEmailConfirmationTokenAsync(ApplicationUser user, string subject)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Scheme);
            await _emailService.SendAsync(user.Email, subject, $"Please confirm your account by clicking <a href=\"{callbackUrl}\">here</a>", true);

            return callbackUrl;
        }


        public async Task<ActionResult> Account()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            List<AdminAccountViewModel> accounts;

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                accounts = _administrationRepository.AccountListByBranch(currentUser.BranchId);
            }
            else // (await _userManager.IsInRoleAsync(user, "Super Administrator"))
            {
                accounts = _administrationRepository.AccountList();
            }



            return View(accounts);
        }

        public async Task<ActionResult> UserAccount(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            

            //return View(accounts);

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var accounts = _administrationRepository.UserList(sortOrder, searchString);

            int pageSize = 10;
            return View(await PaginatedList<UserAccountViewModel>.CreateAsync(accounts, pageNumber ?? 1, pageSize));


        }

        public async Task<ActionResult> LogOff(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Branch(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var branches = _administrationRepository.GetBranches(sortOrder, searchString);

            int pageSize = 100;
            return View(await PaginatedList<Branch>.CreateAsync(branches, pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult EditBranch(long branchId)
        {
            var branch = _administrationRepository.GetBranchForEdit(branchId);

            return View(branch);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBranch(EditBranchViewModel branch)
        {

            var result = await _administrationRepository.UpdateBranch(branch);

            return RedirectToAction("Branch");
            //return View(result);
        }


        public async Task<IActionResult> Holiday(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewBag.Branches = _administrationRepository.GetBranches();

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            if (searchString == "0" || searchString is null)
            {
                ViewData["CurrentValue"] = "Applied to All";
            }
            else
            {
                ViewData["CurrentValue"] = _administrationRepository.GetBranchName(Convert.ToInt64(searchString));
            }

            var holidays = _administrationRepository.GetHoliday(sortOrder, searchString);

            int pageSize = 100;
            return View(await PaginatedList<Holiday>.CreateAsync(holidays, pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult AddHoliday()
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddHoliday(Holiday holiday)
        {
            _administrationRepository.AddHoliday(holiday);
            return RedirectToAction("Holiday");
        }

        [HttpGet]
        public IActionResult DeleteHoliday(long holidayId)
        {
            _administrationRepository.DeleteHoliday(holidayId);
            return RedirectToAction("Holiday");
        }


        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public async Task<IActionResult> ActivityLog(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var activityLogs = _administrationRepository.GetActivityLogs(sortOrder, searchString);

            int pageSize = 20;
            return View(await PaginatedList<ActivityLog>.CreateAsync(activityLogs, pageNumber ?? 1, pageSize));
        }


        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult Price()
        {
            var result = _administrationRepository.GetPrice();
            ViewData["UpdatePriceMessage"] = this.TempData["updatePriceTemp"];
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult Price(Price price)
        {
            var result = _administrationRepository.UpdatePrice(price);
            this.TempData["updatePriceTemp"] = "Price successfully saved.";
            return RedirectToAction("Price");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult Notice()
        {
            var result = _administrationRepository.GetNotice();
            ViewData["UpdateNoticeMessage"] = this.TempData["updateNoticeTemp"];
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult Notice(Notice notice)
        {
            var result = _administrationRepository.UpdateNotice(notice);
            this.TempData["updateNoticeTemp"] = "Announcement successfully saved.";
            return RedirectToAction("Notice");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "Super Administrator, Administrator")]
        [HttpPost]
        public async Task<IActionResult> ExportToExcel(DateTime dateFrom, DateTime dateTo)
        {
            //return View();
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[14] {
                                            new DataColumn("AppointmentCode"),
                                            new DataColumn("ScheduleDate"),
                                            new DataColumn("Email"),
                                            new DataColumn("FirstName"),
                                            new DataColumn("MiddleName"),
                                            new DataColumn("LastName"),
                                            new DataColumn("ContactNumber"),
                                            new DataColumn("NameOfRepresentative"),
                                            new DataColumn("RepresentativeContactNumber"),
                                            new DataColumn("ConsularOffice"),
                                            new DataColumn("Documents"),
                                            new DataColumn("Quantity"),
                                            new DataColumn("Transaction"),
                                            new DataColumn("TotalDocuments")

            });

            IEnumerable<ExportTemplate> applicants;
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                applicants = _administrationRepository.ExportApplicantRecordByBranch(currentUser.BranchId.GetValueOrDefault(), dateFrom, dateTo);
            }
            else
            {
                applicants = _administrationRepository.ExportApplicantRecord(dateFrom, dateTo);
            }

            var sum = applicants.Sum(a => a.TotalDocuments);
            //var ieInt = applicants.Select(a => Convert.ToInt32(a.Quantity.Split(System.Environment.NewLine)));
            //var sum = ieInt.Sum();
            foreach (var applicant in applicants)
            {
                dt.Rows.Add(
                        applicant.AppointmentCode,
                        applicant.ScheduleDate,
                        applicant.Email,
                        applicant.FirstName,
                        applicant.MiddleName,
                        applicant.LastName,
                        applicant.ContactNumber,
                        applicant.NameOfRepresentative,
                        applicant.RepresentativeContactNumber,
                        applicant.ConsularOffice,
                        applicant.Documents,
                        applicant.Quantity,
                        applicant.Transaction,
                        applicant.TotalDocuments
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt);


                //wb.Worksheets.Add(dt);
                var x = dt.Rows.Count + 3;
                ws.Cell($"M{x}").Value = "TOTAL DOCUMENTS";
                ws.Cell($"N{x}").Value = $"{sum}";
                ws.Cell($"M{x + 1}").Value = "APPLICANTS COUNT";
                ws.Cell($"N{x + 1}").Value = $"{dt.Rows.Count}";

                ws.Cell($"M{x}").Style.Font.Bold = true;
                ws.Cell($"M{x + 1}").Style.Font.Bold = true;
                ws.Cell($"N{x}").Style.Font.Bold = true;
                ws.Cell($"N{x + 1}").Style.Font.Bold = true;
                //for (int i = 1; i <= dt.Rows.Count + 1; i++)
                //{
                //    for (int j = 1; j <= dt.Columns.Count; j++)
                //    {
                //        ws.Cell(i, j).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                //        ws.Cell(i, j).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                //        ws.Cell(i, j).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                //    }
                //}
                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                                                  //wb.Worksheets.FirstOrDefault().RightToLeft = true;
                                                  //ws.Table(0).ShowAutoFilter = false;


                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                }
            }
        }

        [Authorize(Roles = "Super Administrator, Administrator")]
        [HttpPost]
        public async Task<IActionResult> ExportAppointmentToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
           
            var model = new ExportPDFViewModel();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                //todo
                model.ExportTemplates = _administrationRepository.ExportApplicantRecordByBranch(currentUser.BranchId.GetValueOrDefault(), dateFrom, dateTo);
            }
            else
            {
                model.ExportTemplates = _administrationRepository.ExportAppointmentToPDF(branchId, dateFrom, dateTo);
            }

            var sum = model.ExportTemplates.Sum(a => a.TotalDocuments);
            //ViewBag.Sum = sum;

            
            model.From = dateFrom;
            model.To = dateTo;
            model.Sum = sum;

            var header = _env.WebRootFileProvider.GetFileInfo("header2.html")?.PhysicalPath;
            var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Landscape,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 10,
                    Bottom = 10,
                    Right = 10,
                    Left = 10
                }
            };
            _generatePdf.SetConvertOptions(options);

            //var data = new TestData
            //{
            //    Text = "This is a test",
            //    Number = 123456
            //};

            var pdf = await _generatePdf.GetByteArray("Views/ExportPDF.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            //return pdfStream;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        [Authorize(Roles = "Super Administrator, Administrator")]
        [HttpPost]
        public async Task<IActionResult> ExportAttendanceToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {

            var model = new ExportPDFViewModel();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                //todo
                model.ExportTemplates = _administrationRepository.ExportApplicantRecordByBranch(currentUser.BranchId.GetValueOrDefault(), dateFrom, dateTo);
            }
            else
            {
                model.ExportTemplates = _administrationRepository.ExportAttendanceToPDF(branchId, dateFrom, dateTo);
            }

            var sum = model.ExportTemplates.Sum(a => a.TotalDocuments);
            //ViewBag.Sum = sum;


            model.From = dateFrom;
            model.To = dateTo;
            model.Sum = sum;
            var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Landscape,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 10,
                    Bottom = 10,
                    Right = 10,
                    Left = 10
                }
            };
            _generatePdf.SetConvertOptions(options);

            var pdf = await _generatePdf.GetByteArray("Views/ExportAttendancePDF.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public async Task<IActionResult> ExportLogsToPDF(DateTime dateFrom, DateTime dateTo)
        {

            var model = new ExportPDFViewModel();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            model.ActivityLogs = _administrationRepository.GetActivityLogs(dateFrom, dateTo).AsEnumerable();
            

            //var sum = model.ExportTemplates.Sum(a => a.TotalDocuments);
            //ViewBag.Sum = sum;


            model.From = dateFrom;
            model.To = dateTo;
            //model.Sum = sum;
            var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
            var options = new ConvertOptions
            {
                FooterHtml = footer,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Landscape,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 10,
                    Bottom = 10,
                    Right = 10,
                    Left = 10
                }
            };
            _generatePdf.SetConvertOptions(options);

            var pdf = await _generatePdf.GetByteArray("Views/ExportLogs.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        [HttpGet]
        public IActionResult UploadImage()
        {
            ViewData["UploadImageMessage"] = this.TempData["UploadImageTemp"];
            string wwwRootPath = _env.WebRootPath;
            string path = Path.Combine(wwwRootPath + "/images/documents/");
            ViewData["Path"] = path;
            return View();
        }



        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(ImageModel model)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _env.WebRootPath;
                string fileName = model.Name;

                string path = Path.Combine(wwwRootPath + "/images/documents/", fileName);

                try
                {
                    System.IO.File.Delete(path);
                }
                catch (Exception)
                {

                }


                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                //model.File.SaveAs(path);

                this.TempData["UploadImageTemp"] = $"{fileName} successfully update.";

                return RedirectToAction("UploadImage");
            }

            return View();
        }
    }
}
