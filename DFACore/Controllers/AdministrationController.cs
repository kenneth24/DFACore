
using ClosedXML.Excel;
using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using Shyjus.BrowserDetection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using UnionBankApi;
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
        private readonly IMessageService _messageService;
        private readonly IActionContextAccessor _accessor;
        private readonly IBrowserDetector _browserDetector;

        private const string PartnerAccountUsername = "partner_sb";
        private const string PartnerAccountPassword = "p@ssw0rd";

        private UnionBankClient _unionBankClient;


        public AdministrationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            AdministrationRepository administrationRepository,
            IWebHostEnvironment env,
            IGeneratePdf generatePdf,
            IMessageService messageService,
            IActionContextAccessor accessor,
            IBrowserDetector browserDetector)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _administrationRepository = administrationRepository;
            _env = env;
            _generatePdf = generatePdf;
            _messageService = messageService;
            _accessor = accessor;
            _browserDetector = browserDetector;


            var unionBankClientConfiguration = new UnionBankClientConfiguration
            {
                BaseUri = "https://api-uat.unionbankph.com/partners/sb",
                ClientId = "fe667681-2650-4c0b-93fb-c365b2cc0953",
                ClientSecret = "J8wF8tY1jM4bN5eN6kO4wM7sP3lB0aG8yN8yT6aQ5aI8sN1aD4",
                PartnerId = "5dff2cdf-ef15-48fb-a87b-375ebff415bb"
            };

            _unionBankClient = new UnionBankClient(unionBankClientConfiguration);
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

            //if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            //{
            //    applicants = _administrationRepository.GetAllApplicantRecordByBranch(currentUser.BranchId, sortOrder, searchString);
            //}
            //else // (await _userManager.IsInRoleAsync(user, "Super Administrator"))
            //{
            applicants = _administrationRepository.GetAllApplicantRecord(sortOrder, searchString);
            //}

            Log("Visit Homepage.", User.Identity.Name);

            int pageSize = 10;
            return View(await PaginatedList<AdminApplicantRecordViewModel>.CreateAsync(applicants, pageNumber ?? 1, pageSize));

            //return View(await applicants);
            //return View();
        }


        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            
            ViewBag.ReturnUrl = returnUrl;

            if (User.Identity.IsAuthenticated)
                await _signInManager.SignOutAsync();

            Log("Visit Admin Login page.");
            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                Log($"Click login but with error: {model.Email}");
                return View(model);
            }

            var user = _userManager.Users.Where(a => a.Email == model.Email).FirstOrDefault(); //.FindByEmailAsync(model.Email.Normalize().ToUpperInvariant());


            if (user != null)
            {
                if (user.Type != 1)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    Log($"Click login but with error invalid username or password.: {model.Email}");
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
                Log($"Successfully logged in: {model.Email}");
                return RedirectToAction("Home", "Administration");
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
                Log($"Click login but with error invalid username or password.: {model.Email}");
                return View(model);
            }

        }


        [HttpGet]
        public IActionResult Home()
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            Log($"Visit Home page.", User.Identity.Name);
            return View();
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
            ViewBag.Roles = _roleManager.Roles.Where(a => a.Name == "Administrator" || a.Name == "Super Administrator");
            ViewBag.Branches = _administrationRepository.GetBranches();
            ViewData["ReturnUrl"] = returnUrl;
            Log($"Visit Registration page for new admin user.", User.Identity.Name);
            return View();
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult RegisterLRA(string returnUrl = null)
        {
            ViewBag.Roles = _roleManager.Roles.Where(a => a.Name == "LRA");
            //ViewBag.Branches = _administrationRepository.GetBranches();
            ViewData["ReturnUrl"] = returnUrl;
            Log($"Visit Registration page for new LRA user.", User.Identity.Name);
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

                var role = await _roleManager.FindByIdAsync(model.Role);

                if (role == null)
                {
                    ViewBag.errorMessage = "Role cannot be found.";
                    return View("Error");
                }

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
                    Type = role.Name == "Super Administrator" || role.Name == "Administrator"? 1 : role.Name == "LRA"? 2 : 0,
                    CreatedDate = DateTime.Now,
                    //EmailConfirmed = false,
                    BranchId = model.BranchId
                };

                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    var newAddedUser = _userManager.Users.Where(a => a.Email == model.Email).FirstOrDefault();
                    

                    if (!await _userManager.IsInRoleAsync(newAddedUser, role.Name))
                    {
                        await _userManager.AddToRoleAsync(newAddedUser, role.Name);
                    }
                    string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");
                    Log($"Register new user with email {user.Email} and role of {role.Name}", User.Identity.Name);
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

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("/ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { email = model.Email, code },
                    protocol: Request.Scheme);

                //var callbackUrl2 = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Scheme);

                await _emailService.SendAsync(
                    model.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                //var attachment = new Attachment("test.pdf", await GeneratePDF(), new ContentType("application", "pdf"));

                //await _messageService.SendEmailAsync(model.Email, model.Email, "Reset Password",
                //        $"Please reset your password by <a href = '{HtmlEncoder.Default.Encode(callbackUrl)}'> clicking here </a>.",
                //        attachment);

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View();

        }


        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
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

            ViewData["UpdateAccountMessage"] = this.TempData["updateAccountTemp"];
            Log($"Visit Admin Accounts", User.Identity.Name);
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
            Log($"Visit User Accounts", User.Identity.Name);
            return View(await PaginatedList<UserAccountViewModel>.CreateAsync(accounts, pageNumber ?? 1, pageSize));


        }

        public async Task<ActionResult> BlackList(
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

            var accounts = _administrationRepository.Blacklist(sortOrder, searchString);

            int pageSize = 10;
            Log($"Visit BlackList page", User.Identity.Name);
            return View(await PaginatedList<UserAccountViewModel>.CreateAsync(accounts, pageNumber ?? 1, pageSize));


        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public async Task<IActionResult> AddToBlackList(string userId)
        {

            var result = await _administrationRepository.AddToBlackList(userId);
            Log($"Add to blacklist the userId of {userId}", User.Identity.Name);
            return RedirectToAction("UserAccount");
            //return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult EditAccount(string userId)
        {
            var model = _administrationRepository.EditAccount(userId);
            ViewBag.Roles = _roleManager.Roles;
            ViewBag.Branches = _administrationRepository.GetBranches();
            var result = new UpdateAccountViewModel
            {
                Id = model.Id,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Suffix = model.Suffix,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Email = model.Email
            };


            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public async Task<IActionResult> EditAccount(UpdateAccountViewModel model)
        {
            // update to repo
            //var result = await _userManager.UpdateAsync(user);
            await _administrationRepository.EditAccount(model);
            this.TempData["updateAccountTemp"] = "Account successfully saved.";
            Log($"Update the account of {model.Email}", User.Identity.Name);
            return RedirectToAction("Account");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public async Task<IActionResult> DeleteAccount(string userId)
        {

            var result = await _administrationRepository.DeleteAccount(userId);
            Log($"Delete the account with userId of {userId}", User.Identity.Name);
            return RedirectToAction("Account");
            //return View(result);
        }

        public async Task<ActionResult> LogOff(string returnUrl = null)
        {
            Log($"Logged out.", User.Identity.Name);
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
            Log($"Visit Consular Office page.", User.Identity.Name);
            return View(await PaginatedList<Branch>.CreateAsync(branches, pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult EditBranch(long branchId)
        {
            var branch = _administrationRepository.GetBranchForEdit(branchId);
            Log($"Visit Update Consular Office page.", User.Identity.Name);
            return View(branch);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBranch(EditBranchViewModel branch)
        {

            var result = await _administrationRepository.UpdateBranch(branch);
            Log($"Update Consular Office with details of {JsonConvert.SerializeObject(branch)}.", User.Identity.Name);
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
            Log($"Visit Calendar.", User.Identity.Name);
            return View(await PaginatedList<Holiday>.CreateAsync(holidays, pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> ExceptionDay(
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

            var holidays = _administrationRepository.GetExceptionDay(sortOrder, searchString);

            int pageSize = 100;
            return View(await PaginatedList<Holiday>.CreateAsync(holidays, pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Calendar()
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calendar(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            if (dateFrom == default || dateTo == default)
            {
                foreach (var modelValue in ModelState.Values)
                {
                    modelValue.Errors.Clear();
                }
                ModelState.AddModelError(string.Empty, "Invalid data in DateFrom or DateTo.");
                return View();
            }

            _administrationRepository.UnAvailableDay(branchId, dateFrom, dateTo);
            return RedirectToAction("ExceptionDay");
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
            Log($"Update holiday: {holiday.Title}", User.Identity.Name);
            _administrationRepository.AddHoliday(holiday);
            return RedirectToAction("Holiday");
        }

        [HttpGet]
        public IActionResult DeleteHoliday(long holidayId)
        {
            Log($"DeleteHoliday: {holidayId}", User.Identity.Name);
            _administrationRepository.DeleteHoliday(holidayId);
            return RedirectToAction("Holiday");
        }

        [HttpGet]
        public IActionResult DeleteExceptionDay(long holidayId)
        {
            Log($"DeleteExceptionDay: {holidayId}", User.Identity.Name);
            _administrationRepository.DeleteHoliday(holidayId);
            return RedirectToAction("ExceptionDay");
        }


        [Authorize(Roles = "Super Administrator, Administrator")]
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
            Log($"Visit SetPrice.", User.Identity.Name);
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult Price(Price price)
        {
            var result = _administrationRepository.UpdatePrice(price);
            this.TempData["updatePriceTemp"] = "Price successfully saved.";
            Log($"Set Price to {JsonConvert.SerializeObject(price)}.", User.Identity.Name);
            return RedirectToAction("Price");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult Notice()
        {
            var result = _administrationRepository.GetNotice(1);
            ViewData["UpdateNoticeMessage"] = this.TempData["updateNoticeTemp"];
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult Notice(Notice notice)
        {
            _administrationRepository.UpdateNotice(1, notice);
            this.TempData["updateNoticeTemp"] = "Announcement successfully saved.";
            Log($"Update Announcement.", User.Identity.Name);
            return RedirectToAction("Notice");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult Declaration()
        {
            var result = _administrationRepository.GetNotice(2);
            ViewData["UpdateNoticeMessage"] = this.TempData["updateNoticeTemp"];
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult Declaration(Notice notice)
        {
            _administrationRepository.UpdateNotice(2, notice);
            this.TempData["updateNoticeTemp"] = "Announcement successfully saved.";
            Log($"Update Declaration.", User.Identity.Name);
            return RedirectToAction("Declaration");
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpGet]
        public IActionResult TermsAndCondition()
        {
            var result = _administrationRepository.GetNotice(3);
            ViewData["UpdateNoticeMessage"] = this.TempData["updateNoticeTemp"];
            return View(result);
        }

        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        public IActionResult TermsAndCondition(Notice notice)
        {
            _administrationRepository.UpdateNotice(3, notice);
            this.TempData["updateNoticeTemp"] = "Announcement successfully saved.";
            Log($"Update TermsAndCondition.", User.Identity.Name);
            return RedirectToAction("TermsAndCondition");
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
            model.LogoPath = Path.Combine(_env.WebRootPath + "/dfa.png");

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
            //var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            //if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            //{
            //    //todo
            //    model.ExportTemplates = _administrationRepository.ExportApplicantRecordByBranch(currentUser.BranchId.GetValueOrDefault(), dateFrom, dateTo);
            //}
            //else
            //{
            //    model.ExportTemplates = _administrationRepository.ExportAttendanceToPDF(branchId, dateFrom, dateTo);
            //}

            model.ExportTemplates = _administrationRepository.ExportAttendanceToPDF(branchId, dateFrom, dateTo);
            //var x = model.ExportTemplates.ToList();

            var count = model.ExportTemplates.Where(a => a.Attendance == "Yes").Count();


            var sum = model.ExportTemplates.Where(a => a.Attendance == "Yes").Sum(a => a.TotalDocuments);
            var totalSum = model.ExportTemplates.Sum(a => a.TotalDocuments);

            double documentPercent = ((double)sum / (double)totalSum) * 100;
            if (Double.IsNaN(documentPercent)) documentPercent = 0;
            
            documentPercent = Math.Round(documentPercent, 2);

            double attendancePercent = ((double)count / (double)model.ExportTemplates.Count()) * 100;
            if (Double.IsNaN(attendancePercent)) attendancePercent = 0;
            attendancePercent = Math.Round(attendancePercent, 2);

            //ViewBag.Sum = sum;


            model.From = dateFrom;
            model.To = dateTo;
            model.Count = count;
            model.Sum = sum;
            model.TotalSum = totalSum;
            model.DocumentsPercent = documentPercent;
            model.AttendancePercent = attendancePercent;
            model.LogoPath = Path.Combine(_env.WebRootPath + "/dfa.png");

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

        [Authorize(Roles = "Super Administrator, Administrator")]
        //[AllowAnonymous]
        //[HttpPost]
        public async Task<IActionResult> ExportUnAttendanceToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {

            var model = new ExportPDFViewModel();

            model.ExportTemplates = _administrationRepository.ExportUnAttendanceToPDF(branchId, dateFrom, dateTo);

            //var count = model.ExportTemplates.Where(a => a.Attendance == "Yes").Count();

            //var sum = model.ExportTemplates.Where(a => a.Attendance == "Yes").Sum(a => a.TotalDocuments);
            var totalSum = model.ExportTemplates.Sum(a => a.TotalDocuments);


            model.From = dateFrom;
            model.To = dateTo;
            //model.Count = count;
            //model.Sum = sum;
            model.TotalSum = totalSum;
            model.LogoPath = Path.Combine(_env.WebRootPath + "/dfa.png");

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

            var pdf = await _generatePdf.GetByteArray("Views/ExportUnAttendancePDF.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        [Authorize(Roles = "Super Administrator, Administrator")]
        [HttpPost]
        public async Task<IActionResult> ExportCancelledAppointmentToPDF(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            var model = new ExportPDFViewModel();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            model.ExportTemplates = _administrationRepository.ExportCancelledAppointmentToPDF(branchId, dateFrom, dateTo);

            var sum = model.ExportTemplates.Sum(a => a.TotalDocuments);

            model.From = dateFrom;
            model.To = dateTo;
            model.Sum = sum;
            model.LogoPath = Path.Combine(_env.WebRootPath + "/dfa.png");

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

            var pdf = await _generatePdf.GetByteArray("Views/ExportPDF.cshtml", model);
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

            model.ActivityLogs = _administrationRepository.GetActivityLogs(dateFrom, dateTo).ToList();//.AsEnumerable();


            //var sum = model.ExportTemplates.Sum(a => a.TotalDocuments);
            //ViewBag.Sum = sum;


            model.From = dateFrom;
            model.To = dateTo;
            model.LogoPath = Path.Combine(_env.WebRootPath + "/dfa.png");
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
                catch (Exception e)
                {
                    ViewBag.errorMessage = e.Message;
                    return View("Error");
                }


                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                //model.File.SaveAs(path);

                this.TempData["UploadImageTemp"] = $"{fileName} successfully updated.";

                return RedirectToAction("UploadImage");
            }

            return View();
        }

        [HttpGet]
        public IActionResult UploadVideo()
        {
            ViewData["UploadImageMessage"] = this.TempData["UploadImageTemp"];
            string wwwRootPath = _env.WebRootPath;
            string path = Path.Combine(wwwRootPath + "/video/");
            ViewData["Path"] = path;
            return View();
        }


        [Authorize(Roles = "Super Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVideo(ImageModel model)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _env.WebRootPath;
                string fileName = $"{model.Name}.mp4";

                string path = Path.Combine(wwwRootPath + "/video/", fileName);

                try
                {
                    System.IO.File.Delete(path);
                }
                catch (Exception e)
                {
                    ViewBag.errorMessage = e.Message;
                    return View("Error");
                }


                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                //model.File.SaveAs(path);

                this.TempData["UploadImageTemp"] = $"{fileName} successfully updated.";

                return RedirectToAction("UploadVideo");
            }

            return View();
        }

        //[AllowAnonymous]
        [HttpGet]
        public async Task<bool> ResendApplication(string applicationCode)
        {
            var model = _administrationRepository.GetApplicantRecord(applicationCode);

            var attachments = new List<Attachment>();

            bool generatePowerOfAttorney = false;
            bool generateAuthLetter = false;

            var dateTimeSched = model.ScheduleDate; //DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            var branch = _administrationRepository.GetBranch(model.ProcessingSite);

            if (model.NameOfRepresentative is null)
            {
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
                    FirstName = model.FirstName?.ToUpper(),
                    MiddleName = model.MiddleName?.ToUpper(),
                    LastName = model.LastName?.ToUpper(),
                    Suffix = model.Suffix?.ToUpper(),
                    DateOfBirth = model.DateOfBirth,
                    ContactNumber = model.ContactNumber,
                    CountryDestination = model.CountryDestination?.ToUpper(),
                    ApostileData = model.ApostileData,
                    ProcessingSite = model.ProcessingSite?.ToUpper(),
                    ProcessingSiteAddress = model.ProcessingSiteAddress?.ToUpper(),
                    ScheduleDate = dateTimeSched, //DateTime.ParseExact(model.ScheduleDate, "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                    ApplicationCode = model.ApplicationCode,
                    //CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Fees,
                    Type = 0,
                    DateCreated = DateTime.UtcNow,
                    QRCode = _administrationRepository.GenerateQRCode($"{model.FirstName?.ToUpper()} {model.MiddleName?.ToUpper()} {model.LastName?.ToUpper()}" +
                    $"{Environment.NewLine}{model.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                    $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.ProcessingSite?.ToUpper()}")
                };

                applicantRecord.AdditionalCode = AddtnlCode(model);

                attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;
            }
            else
            {
                var applicantRecord = new ApplicantRecord
                {
                    BranchId = branch != null ? branch.Id : 0,
                    FirstName = model.FirstName?.ToUpper(),
                    MiddleName = model.MiddleName?.ToUpper(),
                    LastName = model.LastName?.ToUpper(),
                    Suffix = model.Suffix?.ToUpper(),
                    DateOfBirth = model.DateOfBirth,
                    //Address = $"{record.Barangay?.ToUpper()} {record.City?.ToUpper()} {record.Region?.ToUpper()} ",
                    //Nationality = record.Nationality?.ToUpper(),
                    ContactNumber = string.IsNullOrEmpty(model.ContactNumber) ? "" : model.ContactNumber,
                    //CompanyName = record.CompanyName?.ToUpper(),
                    CountryDestination = model.CountryDestination?.ToUpper(),
                    NameOfRepresentative = model.NameOfRepresentative,
                    RepresentativeContactNumber = model.RepresentativeContactNumber,
                    ApostileData = model.ApostileData,
                    ProcessingSite = model.ProcessingSite?.ToUpper(),
                    ProcessingSiteAddress = model.ProcessingSiteAddress?.ToUpper(),
                    ScheduleDate = dateTimeSched,
                    ApplicationCode = model.ApplicationCode, //record.ApplicationCode,
                    //CreatedBy = new Guid(_userManager.GetUserId(User)),
                    Fees = model.Fees,
                    Type = 1,
                    DateCreated = DateTime.UtcNow,
                    QRCode = _administrationRepository.GenerateQRCode($"{model.FirstName?.ToUpper()} {model.MiddleName?.ToUpper()} {model.LastName?.ToUpper()}" +
                                $"{Environment.NewLine}{model.ApplicationCode}{Environment.NewLine}{dateTimeSched.ToString("MM/dd/yyyy")}" +
                                $"{Environment.NewLine}{dateTimeSched.ToString("hh:mm tt")}{Environment.NewLine}{model.ProcessingSite?.ToUpper()}")
                };

                applicantRecord.AdditionalCode = AddtnlCode(model);

                attachments.Add(new Attachment("Apostille Appointment.pdf", await GeneratePDF(applicantRecord), new MimeKit.ContentType("application", "pdf")));

                var age = DateTime.Today.Year - applicantRecord.DateOfBirth.Year;
                if (age < 18)
                    generatePowerOfAttorney = true;

                generateAuthLetter = true;
            }

            if (generatePowerOfAttorney)
            {
                attachments.Add(new Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()),
                    new MimeKit.ContentType("application", "pdf")));
            }

            if (generateAuthLetter)
            {
                attachments.Add(new Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()),
                    new MimeKit.ContentType("application", "pdf")));
            }

            var getUser = _administrationRepository.GetAccount(model.CreatedBy.ToString());
            await _messageService.SendEmailAsync(getUser.UserName, getUser.UserName, "Application File",
                    HtmlTemplate(),
                    attachments.ToArray());
            //Log("Generate Appointment", User.Identity.Name);
            Log($"Resend appointment with code of {applicationCode}.", User.Identity.Name);
            return true;

            //return View();
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

        public string HtmlTemplate()
        {
            using (StreamReader SourceReader = System.IO.File.OpenText(_env.WebRootFileProvider.GetFileInfo("template.html")?.PhysicalPath))
            {

                return SourceReader.ReadToEnd();
            }

        }


        private string AddtnlCode(ApplicantRecord model)
        {
            var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(model.ApostileData);

            var addtnlCode = $"{data.Sum(a => a.Quantity)}-{model.BranchId}{model.ScheduleDate.ToString("HH")}{model.CountryDestination.Length}-" +
                $"{model.DateCreated.ToString("dd")}{model.DateCreated.ToString("MM")}{model.DateCreated.ToString("yy")}" +
                $"{model.DateCreated.ToString("HH")}{model.DateCreated.ToString("mm")}-{model.FirstName.Length}";
            return addtnlCode;
        }

        [HttpGet]
        public async Task<IActionResult> ViewPDFApplication(string applicationCode)
        {

            var get = _administrationRepository.GetApplicantRecord(applicationCode);

            var model2 = new ApplicantRecord
            {
                BranchId = get.BranchId,
                Title = get.Title,
                FirstName = get.FirstName,
                MiddleName = get.MiddleName,
                LastName = get.LastName,
                Suffix = get.Suffix,
                Nationality = get.Nationality,
                ContactNumber = get.ContactNumber,
                CompanyName = get.CompanyName,
                CountryDestination = get.CountryDestination,
                NameOfRepresentative = get.NameOfRepresentative,
                RepresentativeContactNumber = get.RepresentativeContactNumber,
                ApostileData = get.ApostileData,
                ProcessingSite = get.ProcessingSite,
                ProcessingSiteAddress = get.ProcessingSiteAddress,
                ScheduleDate = get.ScheduleDate,
                ApplicationCode = get.ApplicationCode,
                DateCreated = get.DateCreated,
                QRCode = _administrationRepository.GenerateQRCode($"{get.FirstName?.ToUpper()} {get.MiddleName?.ToUpper()} {get.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{get.ApplicationCode}{Environment.NewLine}{get.ScheduleDate.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{get.ScheduleDate.ToString("hh:mm tt")}{Environment.NewLine}{get.ProcessingSite.ToUpper()}"),
                Fees = get.Fees
            };

            model2.AdditionalCode = AddtnlCode(model2);

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

            var pdf = await _generatePdf.GetByteArray("Views/TestBootstrapSSL.cshtml", model2);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }


        //[AllowAnonymous]
        [HttpGet]
        public async Task<bool> CancelApplication(string applicationCode)
        {
            var result = await _administrationRepository.CancelApplication(applicationCode);
            Log($"Cancel appointment with code of {applicationCode}.", User.Identity.Name);
            return result;
        }


        //[AllowAnonymous]
        [HttpPost]
        public IActionResult GetAllApplicantRecordForDT()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            IQueryable<ApplicantModel> applicants;

            //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            //{
            //    applicants = applicants.OrderBy(sortColumn + " " + sortColumnDirection);

            //}
            if (!string.IsNullOrEmpty(searchValue))
            {
                recordsTotal = _administrationRepository.GetAllApplicantRecordCountForDT(searchValue);
                applicants = _administrationRepository.GetAllApplicantRecordForDT(skip, pageSize, searchValue);
            }
            else
            {
                recordsTotal = _administrationRepository.GetAllApplicantRecordCountForDT();
                applicants = _administrationRepository.GetAllApplicantRecordForDT(skip, pageSize);
            }








            var data = applicants.ToList().GroupBy(a => new
            {
                a.ApplicationCode,
                a.ScheduleDate,
                a.DateCreated,
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
            }).Select(gcs => new ApplicantTemplate
            {
                ApplicationCode = gcs.Key.ApplicationCode,
                ScheduleDate = gcs.Key.ScheduleDate.ToString("yyyy/MM/dd hh:mm:ss tt"),
                DateCreated = gcs.Key.DateCreated.ToString("yyyy/MM/dd hh:mm:ss tt"),
                Email = gcs.Key.Email,
                DocumentOwner = $"{gcs.Key.LastName}, {gcs.Key.FirstName} {gcs.Key.MiddleName}",
                ContactNumber = gcs.Key.ContactNumber,
                NameOfRepresentative = gcs.Key.NameOfRepresentative,
                RepresentativeContactNumber = gcs.Key.RepresentativeContactNumber,
                ConsularOffice = gcs.Key.ProcessingSite,
                CountryDestination = gcs.Key.CountryDestination,
                Documents = string.Join("<br>", gcs.Select(a => a.DocumentName)),
                Quantity = string.Join("<br>", gcs.Select(d => d.Quantity)),
                Transaction = string.Join("<br>", gcs.Select(d => d.Transaction))


            }).ToList();

            var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
            return Ok(jsonData);

        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            ViewBag.Branch = 1;
            ViewBag.FilterDate = DateTime.Now;
            var result = _administrationRepository.GetDocumentCountByDay(1, DateTime.Now);
            Log("Visit Dashboard.", User.Identity.Name);
            return View(result);
        }


        [HttpPost]
        public IActionResult Dashboard(long branchId, DateTime startDate)
        {
            ViewBag.Branches = _administrationRepository.GetBranches();
            if (branchId == default || startDate == default)
            {
                foreach (var modelValue in ModelState.Values)
                {
                    modelValue.Errors.Clear();
                }
                ModelState.AddModelError(string.Empty, "Invalid attempt.");
                ViewBag.Branch = 1;
                ViewBag.FilterDate = DateTime.Now;
                return View(default);
            }
            var result = _administrationRepository.GetDocumentCountByDay(branchId, startDate);
            ViewBag.Branch = branchId;
            ViewBag.FilterDate = startDate;
            return View(result);
        }

        //[AllowAnonymous]
        public async Task<IActionResult> PaymentTransaction()
        {
            var accessToken = await _unionBankClient.GetPartnerAccountAccessTokenAsync(PartnerAccountUsername, PartnerAccountPassword);
            var result = await _unionBankClient.GetPartnerAccountTransactionHistoryAsync(new DateTime(2017, 1, 1), new DateTime(2017, 12, 31), PartnerAccountTransactionType.Debit, 100, accessToken);
            return View(result);
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
                    Email = email,
                    UserType = 1
                };
                _administrationRepository.AddActivityLog(activity);
            }
        }




    }
}
