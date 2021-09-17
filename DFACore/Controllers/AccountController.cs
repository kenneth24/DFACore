using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using NETCore.MailKit.Core;
using Shyjus.BrowserDetection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore;

namespace DFACore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly GoogleCaptchaService _googleCaptchaService;
        private readonly IMessageService _messageService;
        private readonly IGeneratePdf _generatePdf;
        private readonly IWebHostEnvironment _env;
        private readonly IActionContextAccessor _accessor;
        private readonly IBrowserDetector _browserDetector;
        private readonly ApplicantRecordRepository _applicantRepo;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            GoogleCaptchaService googleCaptchaService,
            IMessageService messageService,
            IGeneratePdf generatePdf,
            IWebHostEnvironment env,
            IActionContextAccessor accessor,
            IBrowserDetector browserDetector,
            ApplicantRecordRepository applicantRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _googleCaptchaService = googleCaptchaService;
            _messageService = messageService;
            _generatePdf = generatePdf;
            _env = env;
            _accessor = accessor;
            _browserDetector = browserDetector;
            _applicantRepo = applicantRepo;
        }

        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var googleReCaptcha = _googleCaptchaService.VerifyReCaptcha(model.Token);
            if (!googleReCaptcha.Result.success && googleReCaptcha.Result.score <= 0.5)
            {
                ModelState.AddModelError(string.Empty, "Invalid attempt.");
                return View();
            }

            if (!model.IsTermsAndConditionChecked)
            {
                ModelState.AddModelError(string.Empty, "You need to accept the Terms and Conditions to continue.");
                return View(model);
            }

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
                    Type = 0,
                    CreatedDate = DateTime.Now
                };

                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {

                    string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Scheme);
                    ////_email.SendMail(user.Email, callbackUrl);
                    //await _emailService.SendAsync(model.Email, "Email Verification", $"<a href=\"{callbackUrl}\" style='background-color: #217ff3;color: white; '>Verify Email</a>", true);
                    ////await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("VerifyEmail", new { email = user.Email });
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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                string errorMessage = error.Description;
                if (error.Description.EndsWith("is already taken."))
                    errorMessage = "Email 'kneth.villafuerte@gmail.com' is already taken.";
                ModelState.AddModelError("", errorMessage);
            }
        }

        private async Task<string> SendEmailConfirmationTokenAsync(ApplicationUser user, string subject)
        {
            string code = (await _userManager.GenerateEmailConfirmationTokenAsync(user)).Base64ForUrlEncode();
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Scheme);
            await _emailService.SendAsync(user.Email, subject, $"Please confirm your account by clicking <a href=\"{callbackUrl}\">here</a>", true);

            return callbackUrl;
        }

        [AllowAnonymous]
        public ActionResult VerifyEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                ViewData["Email"] = email;
            }
            return View();

        }


        [AllowAnonymous]
        public async Task<ActionResult> ResendEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");
                return RedirectToAction("VerifyEmail", new { email = user.Email });
            }
            else
            {
                ViewBag.errorMessage = "User email not found.";
                return View("Error");
            }

        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code.Base64ForUrlDecode());
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

   

        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl = null)
        {

            await LogOff();
            ViewBag.ReturnUrl = returnUrl;
            ViewData["NoticeMessage"] = _applicantRepo.GetNotice(1);
            ViewData["DeclarationMessage"] = _applicantRepo.GetNotice(2);
            ViewData["TermsAndConditionsMessage"] = _applicantRepo.GetNotice(3);
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
            {
                Log("Visited");
                return View();
            }

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

            //var user = await _userManager.FindByEmailAsync(model.Email);
            var user = _userManager.Users.Where(a => a.Email == model.Email).FirstOrDefault();

            //await _userManager.SetLockoutEnabledAsync(user, true);
            //await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(100));

            if (user != null)
            {
                if (user.Type != 0)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");

                    // Uncomment to debug locally  
                    // ViewBag.Link = callbackUrl;
                    ViewBag.errorMessage = "You must have a confirmed email to log on. "
                                         + "The confirmation token has been resent to your email account.";
                    return View("Error");
                }
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                //var user = await _userManager.FindByEmailAsync(model.Email);
                //if (user != null)
                //{
                //    if (!await _userManager.IsEmailConfirmedAsync(user))
                //    {
                //        await LogOff();
                //        ViewBag.errorMessage = "You must have a confirmed email to log on. "
                //              + "The confirmation token has been resent to your email account.";
                //        return View("Error"); // verify first the email
                //    }
                //}
                //else
                //{
                //    await LogOff();
                //    return RedirectToAction("Login");
                //}
                //return View(model);
                //return RedirectToAction("Index", "Home");
                Log("Logged In", model.Email);
                return RedirectToAction("ApplicantTypeSelection", "Home");
            }

            if (result.IsNotAllowed)
            {
                Log("Logged In but email is not activated.", model.Email);
                ViewBag.errorMessage = "You must have a confirmed email to log on. "
                              + "The confirmation token has been resent to your email account.";
                return View("Error");
            }
            if (result.IsLockedOut)
            {
                //return View("Lockout");
                Log("Logged In but account is locked.", model.Email);
                ModelState.AddModelError("", "You are no longer authorized to use the Online Appointment System because you have violated and continue to violate the terms and conditions of this website.");
                return View(model);
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            }
            else
            {
                Log("Logged In but invalid username or password.", model.Email);
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

        }


        [AllowAnonymous]
        public async Task<ActionResult> AdminLogin(string returnUrl = null)
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
        public async Task<ActionResult> AdminLogin(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    string callbackUrl = await SendEmailConfirmationTokenAsync(user, "Email Verification");

                    // Uncomment to debug locally  
                    // ViewBag.Link = callbackUrl;
                    ViewBag.errorMessage = "You must have a confirmed email to log on. "
                                         + "The confirmation token has been resent to your email account.";
                    return View("Error");
                }
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                //var user = await _userManager.FindByEmailAsync(model.Email);
                //if (user != null)
                //{
                //    if (!await _userManager.IsEmailConfirmedAsync(user))
                //    {
                //        await LogOff();
                //        ViewBag.errorMessage = "You must have a confirmed email to log on. "
                //              + "The confirmation token has been resent to your email account.";
                //        return View("Error"); // verify first the email
                //    }
                //}
                //else
                //{
                //    await LogOff();
                //    return RedirectToAction("Login");
                //}
                //return View(model);
                return RedirectToAction("Index", "Home");
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

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                code = code.Base64ForUrlEncode();
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

        //public async Task<MemoryStream> GeneratePDF()
        //{
        //    var header = _env.WebRootFileProvider.GetFileInfo("header.html")?.PhysicalPath;
        //    var footer = _env.WebRootFileProvider.GetFileInfo("footer.html")?.PhysicalPath;
        //    var options = new ConvertOptions
        //    {
        //        HeaderHtml = header,
        //        FooterHtml = footer,
        //        PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
        //        PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
        //        {
        //            Top = 20,
        //            Bottom = 20
        //        }
        //    };
        //    _generatePdf.SetConvertOptions(options);

        //    var data = new TestData
        //    {
        //        Text = "This is a test",
        //        Number = 123456
        //    };

        //    var pdf = await _generatePdf.GetByteArray("Views/TestBootstrapSSL.cshtml", data);
        //    var pdfStream = new System.IO.MemoryStream();
        //    pdfStream.Write(pdf, 0, pdf.Length);
        //    pdfStream.Position = 0;
        //    return pdfStream;
        //}

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                ViewBag.errorMessage = "A code must be supplied for password reset.";
                return View("Error");
            }
            else
                return View();

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return View("ResetPasswordConfirmation");
            }
            //model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            model.Code = model.Code.Base64ForUrlDecode();
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {

            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateAccount([FromBody]LoginViewModel model)
        {

            if (!ModelState.IsValid)
                return Ok(false);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                    return Ok(false);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return Ok(true);
            }
            

            return Ok(false);
        }


        //[HttpPost]
        ///[ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
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


    }


    public static class StringExtensions
    {
        public static string Base64ForUrlEncode(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            return UrlTokenEncode(buffer);
        }

        public static string Base64ForUrlDecode(this string str)
        {
            var buffer = UrlTokenDecode(str);
            return buffer != null ? Encoding.UTF8.GetString(buffer) : null;
        }

        private static string UrlTokenEncode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input.Length < 1)
                return string.Empty;

            ////////////////////////////////////////////////////////
            // Step 1: Do a Base64 encoding
            var base64String = Convert.ToBase64String(input);
            if (base64String == null)
                return null;

            int endPosition;
            ////////////////////////////////////////////////////////
            // Step 2: Find how many padding chars are present in the end
            for (endPosition = base64String.Length; endPosition > 0; endPosition--)
            {
                if (base64String[endPosition - 1] != '=') // Found a non-padding char!
                {
                    break; // Stop here
                }
            }

            ////////////////////////////////////////////////////////
            // Step 3: Create char array to store all non-padding chars,
            //      plus a char to indicate how many padding chars are needed
            var base64Chars = new char[endPosition + 1];
            base64Chars[endPosition] = (char)('0' + base64String.Length - endPosition); // Store a char at the end, to indicate how many padding chars are needed

            ////////////////////////////////////////////////////////
            // Step 3: Copy in the other chars. Transform the "+" to "-", and "/" to "_"
            for (var i = 0; i < endPosition; i++)
            {
                var character = base64String[i];

                switch (character)
                {
                    case '+':
                        base64Chars[i] = '-';
                        break;

                    case '/':
                        base64Chars[i] = '_';
                        break;

                    case '=':
                        Debug.Assert(false);
                        base64Chars[i] = character;
                        break;

                    default:
                        base64Chars[i] = character;
                        break;
                }
            }
            return new string(base64Chars);
        }

        private static byte[] UrlTokenDecode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var inputLength = input.Length;
            if (inputLength < 1)
                return new byte[0];

            ///////////////////////////////////////////////////////////////////
            // Step 1: Calculate the number of padding chars to append to this string.
            //         The number of padding chars to append is stored in the last char of the string.
            var paddingCharacters = input[inputLength - 1] - '0';
            if (paddingCharacters < 0 || paddingCharacters > 10)
                return null;


            ///////////////////////////////////////////////////////////////////
            // Step 2: Create array to store the chars (not including the last char)
            //          and the padding chars
            var base64Chars = new char[inputLength - 1 + paddingCharacters];


            ////////////////////////////////////////////////////////
            // Step 3: Copy in the chars. Transform the "-" to "+", and "*" to "/"
            for (var i = 0; i < inputLength - 1; i++)
            {
                var character = input[i];

                switch (character)
                {
                    case '-':
                        base64Chars[i] = '+';
                        break;

                    case '_':
                        base64Chars[i] = '/';
                        break;

                    default:
                        base64Chars[i] = character;
                        break;
                }
            }

            ////////////////////////////////////////////////////////
            // Step 4: Add padding chars
            for (var i = inputLength - 1; i < base64Chars.Length; i++)
            {
                base64Chars[i] = '=';
            }

            // Do the actual conversion
            return Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
        }
    }
}

