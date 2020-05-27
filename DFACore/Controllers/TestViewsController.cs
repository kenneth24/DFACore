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

namespace DFACore.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestViewsController : ControllerBase
    {
        readonly IGeneratePdf _generatePdf;
        private readonly IWebHostEnvironment _env;
        public TestViewsController(IGeneratePdf generatePdf,
            IWebHostEnvironment env)
        {
            _generatePdf = generatePdf;
            _env = env;
        }

        [HttpGet]
        [Route("TestViewer")]
        public async Task<IActionResult> TestViewer()
        {
            var model = new ApplicantRecordViewModel
            {
                Title = "MR.",
                FirstName = "KENNETH",
                MiddleName = "MAGCALAS",
                LastName = "VILLAFUERTE",
                Suffix = "",
                Barangay = "SAN ISIDRO ST CAMARIN",
                City = "CALOOCAN CITY",
                Region = "NCR",
                Nationality = "FILIPINO",
                ContactNumber = "09777639853",
                CompanyName = "BASECAMP TECHNOLOGY",
                CountryDestination = "UNITED STATES OF AMERICA",
                NameOfRepresentative = "John S. Doe",
                RepresentativeContactNumber = "09876543210",
                ApostileData = "[{\"Name\":\"NBI Clearance/Sundry\",\"Quantity\":1},{\"Name\":\"Birth Certificate\",\"Quantity\":1},{\"Name\":\"Marriage Certificate\",\"Quantity\":1},{\"Name\":\"Death Certificate\",\"Quantity\":1},{\"Name\":\"Certificate of No Marriage Record\",\"Quantity\":1}]",
                ProcessingSite = "DFA - Office of Consular Affairs",
                ScheduleDate = DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm tt"),
                ApplicationCode = "MNL-420001415004"
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

            var pdf = await _generatePdf.GetByteArray("Views/TestBootstrapSSL.cshtml", model);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

    

    }
}
