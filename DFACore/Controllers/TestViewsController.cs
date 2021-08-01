using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DFACore.Models;
using Wkhtmltopdf.NetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using DFACore.Repository;

namespace DFACore.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestViewsController : ControllerBase
    {
        readonly IGeneratePdf _generatePdf;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicantRecordRepository _applicantRepo;
        public TestViewsController(IGeneratePdf generatePdf,
            IWebHostEnvironment env,
             ApplicantRecordRepository applicantRepo)
        {
            _generatePdf = generatePdf;
            _env = env;
            _applicantRepo = applicantRepo;
        }

        [HttpGet]
        [Route("TestViewer")]
        public async Task<IActionResult> TestViewer()
        {
            //var model = new ApplicantRecord
            //{
            //    Title = "MR.",
            //    FirstName = "KENNETH ASADFA",
            //    MiddleName = "MAGCALAS",
            //    LastName = "VILLAFUERTE ADSFA",
            //    Suffix = "",
            //    Nationality = "FILIPINO",
            //    ContactNumber = "09777639853",
            //    CompanyName = "BASECAMP TECHNOLOGY",
            //    CountryDestination = "UNITED STATES OF AMERICA",
            //    NameOfRepresentative = "John S. Doe",
            //    RepresentativeContactNumber = "09876543210",
            //    ApostileData = "[{\"Name\":\"NBI Clearance/Sundry\",\"Quantity\":1,\"Transaction\":\"Regular\"}," +
            //    "{\"Name\":\"Birth Certificate\",\"Quantity\":1,\"Transaction\":\"Regular\"}," +
            //    "{\"Name\":\"Marriage Certificate\",\"Quantity\":1,\"Transaction\":\"Regular\"}," +
            //    "{\"Name\":\"Death Certificate\",\"Quantity\":1,\"Transaction\":\"Regular\"}," +
            //    "{\"Name\":\"Certificate of No Marriage Record\",\"Quantity\":1,\"Transaction\":\"Regular\"}]",
            //    ProcessingSite = "DFA - Office of Consular Affairs",
            //    ScheduleDate = DateTime.UtcNow,
            //    ApplicationCode = "MNL-420001415004",
            //    QRCode = _applicantRepo.GenerateQRCode($"Kenneth Villafuerte {Environment.NewLine} 0212124 {Environment.NewLine} May 24, 2020 {Environment.NewLine} 10am {Environment.NewLine} DFA-OCA")
            //};

            var get = _applicantRepo.Get(205494);

            var model2 = new ApplicantRecord
            {
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
                QRCode = _applicantRepo.GenerateQRCode($"{get.FirstName?.ToUpper()} {get.MiddleName?.ToUpper()} {get.LastName?.ToUpper()}" +
                            $"{Environment.NewLine}{get.ApplicationCode}{Environment.NewLine}{get.ScheduleDate.ToString("MM/dd/yyyy")}" +
                            $"{Environment.NewLine}{get.ScheduleDate.ToString("hh:mm tt")}{Environment.NewLine}{"DFA - Office of Consular Affairs (ASEANA)".ToUpper()}"),
                Fees = get.Fees
            };


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

    

    }
}
