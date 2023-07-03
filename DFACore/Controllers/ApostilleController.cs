using DFACore.Models;
using DFACore.Repository;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore;

namespace DFACore.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]/send")]
    public class ApostilleController : ControllerBase
    {

        private readonly IMessageService _messageService;
        private readonly IGeneratePdf _generatePdf;
        private readonly ApplicantRecordRepository _applicantRepo;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApostilleController(IMessageService messageService, ApplicantRecordRepository applicantRepo, UserManager<ApplicationUser> userManager, IGeneratePdf generatePdf, IWebHostEnvironment env)
        {
            _messageService = messageService;
            _userManager = userManager;
            _applicantRepo = applicantRepo;
            _generatePdf = generatePdf;
            _env = env;
        }

        [HttpPost(Name = "send")]
        public async Task<IActionResult> Send([FromBody] ApostilleModel model)
        {
            try
            {
                var attachments = new List<DFACore.Repository.Attachment>();

                var apostilleDocs = _applicantRepo.GetByCodeWithoutDash(model.ApplicationCode);

                if (!apostilleDocs.Any())
                {
                    return new NotFoundObjectResult(new { IsSuccess = false, Reason = $"Code with {model.ApplicationCode} not found." });
                }

                bool generatePowerOfAttorney = false;
                bool generateAuthLetter = false;
                foreach (var item in apostilleDocs)
                {
                    item.QRCode = _applicantRepo.GenerateQRCode($"{item.FirstName?.ToUpper()} {item.MiddleName?.ToUpper()} {item.LastName?.ToUpper()}" +
                                $"{Environment.NewLine}{item.ApplicationCode}{Environment.NewLine}{item.ScheduleDate.ToString("MM/dd/yyyy")}" +
                                $"{Environment.NewLine}{item.ScheduleDate.ToString("hh:mm tt")}{Environment.NewLine}{item.ProcessingSite?.ToUpper()}");

                    attachments.Add(new Repository.Attachment($"{item.ApplicationCode} Apostille Appointment.pdf", await GeneratePDF(item), new MimeKit.ContentType("application", "pdf")));
                    var age = DateTime.Today.Year - item.DateOfBirth.Year;
                    if (age < 18)
                        generatePowerOfAttorney = true;
                    if (!string.IsNullOrEmpty(item.NameOfRepresentative))
                        generateAuthLetter = true;
                }

                //attach only 1 pdf for this
                if (generatePowerOfAttorney)
                    attachments.Add(new Repository.Attachment("Power-Of-Attorney.pdf", await GeneratePowerOfAttorneyPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));

                if (generateAuthLetter)
                    attachments.Add(new Repository.Attachment("Authorization-Letter.pdf", await GenerateAuthorizationLetterPDF(new TestData()), new MimeKit.ContentType("application", "pdf")));

                var apostilleDoc = apostilleDocs.FirstOrDefault();
                var user = _userManager.Users.FirstOrDefault(x => x.Id == apostilleDoc.CreatedBy.ToString());

                if (user == null)
                    return new NotFoundObjectResult(new { IsSuccess = false, Reason = $"User associated with the code {model.ApplicationCode} not found." });

                await _messageService.SendEmailAsync(apostilleDoc.Type == 1 ? apostilleDoc.NameOfRepresentative : $"{apostilleDoc.FirstName} {apostilleDoc.LastName}", user?.Email, "Application File",
                           HtmlTemplate(), attachments.ToArray());

                return new JsonResult(new { IsSuccess = true, Reason = "" });
            }
            catch (Exception e)
            {
                return new JsonResult(new { IsSuccess = false, Reason = $"An error has occured while sending the pdf. Info: {e.Message}" });
            }
            
        }


        private async Task<MemoryStream> GeneratePDF(ApplicantRecord model)
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

        private string AddtnlCode(ApplicantRecord model)
        {
            var data = JsonConvert.DeserializeObject<List<ApostilleDocumentModel>>(model.ApostileData);

            var addtnlCode = $"{data.Sum(a => a.Quantity)}-{model.BranchId}{model.ScheduleDate.ToString("HH")}{model.CountryDestination.Length}-" +
                $"{model.DateCreated.ToString("dd")}{model.DateCreated.ToString("MM")}{model.DateCreated.ToString("yy")}" +
                $"{model.DateCreated.ToString("HH")}{model.DateCreated.ToString("mm")}-{model.FirstName.Length}";
            return addtnlCode;
        }

        private string HtmlTemplate()
        {
            using (StreamReader SourceReader = System.IO.File.OpenText(_env.WebRootFileProvider.GetFileInfo("template.html")?.PhysicalPath))
            {
                return SourceReader.ReadToEnd();
            }

        }


        private async Task<MemoryStream> GeneratePowerOfAttorneyPDF(TestData data)
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

        private async Task<MemoryStream> GenerateAuthorizationLetterPDF(TestData data)
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

    }

    public class ApostilleModel
    {
        public string ApplicationCode { get; set; }
    }
}
