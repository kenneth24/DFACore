using DFACore.Models;
using DFACore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
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

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            GoogleCaptchaService googleCaptchaService,
            IMessageService messageService,
            IGeneratePdf generatePdf,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _googleCaptchaService = googleCaptchaService;
            _messageService = messageService;
            _generatePdf = generatePdf;
            _env = env;
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
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    UserName = model.Email,
                    Email = model.Email,
                    Type = 0
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
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
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
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
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
                //return RedirectToAction("Index", "Home");
                return RedirectToAction("ApplicantTypeSelection", "Home");
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
            model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
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


        //[HttpPost]
        ///[ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }



    }
}

