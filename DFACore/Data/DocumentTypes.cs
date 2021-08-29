using DFACore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Data
{
    public class DocumentTypes
    {
        List<Documents> _documents;
        public DocumentTypes()
        {
            _documents = new List<Documents>();
        }

        public List<Documents> Get()
        {
            var nbi = new Documents
            {
                Id = "nbiClearance",
                Value = "NBI Clearance",
                Name = "NBI Clearance/Sundry",
                Description = "*Original document issued by National Bureau of Investigation (NBI) with dry seal."
            };

            nbi.Quantities = new List<DocumentInfo>();
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearanceExpedite", Name = "Expedite" });
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearanceRegular", Name = "Regular" });
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearance", Name = "Total" });

            nbi.Info = new List<DocumentInfo>();
            nbi.Info.Add(new DocumentInfo { Name = "*Personal Copy is NOT valid", Source = "~/images/documents/1 NBI Personal Copy sample (Not Acceptable).png" });
            nbi.Info.Add(new DocumentInfo { Name = "*Sample copy of NBI sundry", Source = "~/images/documents/1 Sample NBI Sundry-1.png" });

            var psaBirthCert = new Documents
            {
                Id = "birthCertificate",
                Value = "Birth Certificate",
                Name = "PSA/NSO/Local Civil Registrar Birth Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaBirthCert.Quantities = new List<DocumentInfo>();
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificateExpedite", Name = "Expedite" });
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificateRegular", Name = "Regular" });
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificate", Name = "Total" });

            psaBirthCert.Info = new List<DocumentInfo>();
            psaBirthCert.Info.Add(new DocumentInfo { Name = "*LCR copy of Birth Certificate (Form 1A)", Source = "~/images/documents/2 LCR copy of Birth (Form 1A).png" });

            var psaMarriageCert = new Documents
            {
                Id = "marriageCertificate",
                Value = "Marriage Certificate",
                Name = "PSA/NSO/Local Civil Registrar Marriage Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificateExpedite", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificateRegular", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificate", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "*Marriage Certificate (LCR Copy, 1A, 3A)", Source = "~/images/documents/2 LCR copy of Marriage (Form 3A).png" });

            var psaDeathCert = new Documents
            {
                Id = "deathCertificate",
                Value = "Death Certificate",
                Name = "PSA/NSO/Local Civil Registrar Death Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaDeathCert.Quantities = new List<DocumentInfo>();
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificateExpedite", Name = "Expedite" });
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificateRegular", Name = "Regular" });
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificate", Name = "Total" });

            psaDeathCert.Info = new List<DocumentInfo>();
            psaDeathCert.Info.Add(new DocumentInfo { Name = "*LCR copy of Death Certificate (Form 2A)", Source = "~/images/documents/2 LCR copy of Death (Form 2A).png" });

            var psaCenomar = new Documents
            {
                Id = "cenomar",
                Value = "Certificate of No Marriage Record",
                Name = "PSA/NSO/Local Civil Registrar Certificate of No Marriage Record (CENOMAR, Advisory on Marriage and/or Negative Records)",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaCenomar.Quantities = new List<DocumentInfo>();
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomarExpedite", Name = "Expedite" });
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomarRegular", Name = "Regular" });
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomar", Name = "Total" });

            var school = new Documents
            {
                Id = "elementaryAndHighschool",
                Value = "Form 137 & Diploma",
                Name = "School Documents For Elementary and High School Level (Form 137 and Diploma)",
                Description = "*Certified True Copies from school"
            };

            school.Quantities = new List<DocumentInfo>();
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschoolExpedite", Name = "Expedite" });
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschoolRegular", Name = "Regular" });
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschool", Name = "Total" });

            school.Info = new List<DocumentInfo>();
            school.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from DepEd Regional Office.", Source = "~/images/documents/3a Sample CAV for Elementary and Highschool Level (Form-137 and Diploma).png" });

            var schoolTOR = new Documents
            {
                Id = "techVoch",
                Value = "TOR & Diploma/National Certificate",
                Name = "School Documents For Technical and Vocational Courses (TOR and Diploma/National Certificate)",
                Description = "*Certified True Copies from school"
            };

            schoolTOR.Quantities = new List<DocumentInfo>();
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVochExpedite", Name = "Expedite" });
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVochRegular", Name = "Regular" });
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVoch", Name = "Total" });

            schoolTOR.Info = new List<DocumentInfo>();
            schoolTOR.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from Technical and Skills Development Authority (TESDA).", Source = "~/images/documents/3b Sample CAV for Technical and Vocational Courses (TOR and Diploma; National Certificate).png" });

            var schoolCollege = new Documents
            {
                Id = "stateCollegesAndUniversities",
                Value = "State Colleges and Universities TOR & Diploma",
                Name = "School Documents For State Colleges and Universities (TOR and Diploma)s",
                Description = "*Certified True Copies from school"
            };

            schoolCollege.Quantities = new List<DocumentInfo>();
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversitiesExpedite", Name = "Expedite" });
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversitiesRegular", Name = "Regular" });
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversities", Name = "Total" });

            schoolCollege.Info = new List<DocumentInfo>();
            schoolCollege.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from the school.", Source = "~/images/documents/3c Sample CAV for State Colleges and Universities (TOR and Diploma).png" });

            var schoolPrivateOrLocal = new Documents
            {
                Id = "privateOrLocalColleges",
                Value = "Private/Local Colleges and Universities TOR & Diploma",
                Name = "School Documents For Private/Local Colleges and Universities (TOR and Diploma)",
                Description = "*Certified True Copies from school"
            };

            schoolPrivateOrLocal.Quantities = new List<DocumentInfo>();
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalCollegesExpedite", Name = "Expedite" });
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalCollegesRegular", Name = "Regular" });
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalColleges", Name = "Total" });

            schoolPrivateOrLocal.Info = new List<DocumentInfo>();
            schoolPrivateOrLocal.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from Commission on Higher Education (CHED).", Source = "~/images/documents/3d Sample CAV for Private or Local Colleges and Universities (TOR and Diploma).png" });

            var prc = new Documents
            {
                Id = "PRC",
                Value = "PRC Document/s",
                Name = "Professional Regulation Commission (PRC) document/s",
                Description = ""
            };

            prc.Quantities = new List<DocumentInfo>();
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRCExpedite", Name = "Expedite" });
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRCRegular", Name = "Regular" });
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRC", Name = "Total" });

            prc.Info = new List<DocumentInfo>();
            prc.Info.Add(new DocumentInfo { Name = "*Originally-signed and/or certified true copies from PRC.", Source = "~/images/documents/4 PRC Certified True Copy Stamp.png" });

            var medCerts = new Documents
            {
                Id = "medCerts",
                Value = "Medical Certificates",
                Name = "Medical Certificates",
                Description = ""
            };

            medCerts.Quantities = new List<DocumentInfo>();
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCertsExpedite", Name = "Expedite" });
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCertsRegular", Name = "Regular" });
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCerts", Name = "Total" });

            medCerts.Info = new List<DocumentInfo>();
            medCerts.Info.Add(new DocumentInfo { Name = "*For employement: DOH Stamp per document.", Source = "~/images/documents/5 Sample DOH stamp.png" });
            medCerts.Info.Add(new DocumentInfo { Name = "*For other purpose: Certification issued by the DOH with attached Medical Certificate.", Source = "~/images/documents/5 Sample DOH Certification.png" });

            var caap = new Documents
            {
                Id = "CAAP",
                Value = "CAAP Documents",
                Name = "Civil Aviation Authority of the Philippines (CAAP) issued document/s",
                Description = ""
            };

            caap.Quantities = new List<DocumentInfo>();
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAPExpedite", Name = "Expedite" });
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAPRegular", Name = "Regular" });
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAP", Name = "Total" });

            caap.Info = new List<DocumentInfo>();
            caap.Info.Add(new DocumentInfo { Name = "*Certificate by CAAP.", Source = "~/images/documents/6 Sample CAAP Certification.png" });

            var driverLicense = new Documents
            {
                Id = "driverLicense",
                Value = "Driver's License",
                Name = "Driver's License",
                Description = ""
            };

            driverLicense.Quantities = new List<DocumentInfo>();
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicenseExpedite", Name = "Expedite" });
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicenseRegular", Name = "Regular" });
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicense", Name = "Total" });

            driverLicense.Info = new List<DocumentInfo>();
            driverLicense.Info.Add(new DocumentInfo { Name = "*Certification from Land Transportation Office (LTO Main Branch Only).", Source = "~/images/documents/7 Sample LTO Certification.png" });

            var coe = new Documents
            {
                Id = "COE",
                Value = "Certificate of Employment",
                Name = "Documents issued by a private entity : Certificate of Employment",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            coe.Quantities = new List<DocumentInfo>();
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOEExpedite", Name = "Expedite" });
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOERegular", Name = "Regular" });
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOE", Name = "Total" });

            coe.Info = new List<DocumentInfo>();
            coe.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "~/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var invitationLetter = new Documents
            {
                Id = "invitationLetter",
                Value = "Invitation Letter",
                Name = "Documents issued by a private entity : Invitation Letter",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            invitationLetter.Quantities = new List<DocumentInfo>();
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetterExpedite", Name = "Expedite" });
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetterRegular", Name = "Regular" });
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetter", Name = "Total" });

            invitationLetter.Info = new List<DocumentInfo>();
            invitationLetter.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "~/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var trainings = new Documents
            {
                Id = "trainings",
                Value = "Trainings",
                Name = "Documents issued by a private entity : Trainings",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            trainings.Quantities = new List<DocumentInfo>();
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainingsExpedite", Name = "Expedite" });
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainingsRegular", Name = "Regular" });
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainings", Name = "Total" });

            trainings.Info = new List<DocumentInfo>();
            trainings.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "~/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var baptismalCert = new Documents
            {
                Id = "baptismalCert",
                Value = "Baptismal Certificate",
                Name = "Documents issued by a private entity : Baptismal Certificate",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            baptismalCert.Quantities = new List<DocumentInfo>();
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCertExpedite", Name = "Expedite" });
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCertRegular", Name = "Regular" });
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCert", Name = "Total" });

            baptismalCert.Info = new List<DocumentInfo>();
            baptismalCert.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "~/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var privateIssuedOtherDocu = new Documents
            {
                Id = "privateIssuedOtherDocu",
                Value = "Other Document (Private Entity)",
                Name = "Documents issued by a private entity : Other Document",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            privateIssuedOtherDocu.Quantities = new List<DocumentInfo>();
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocuExpedite", Name = "Expedite" });
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocuRegular", Name = "Regular" });
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocu", Name = "Total" });

            privateIssuedOtherDocu.Info = new List<DocumentInfo>();
            privateIssuedOtherDocu.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "~/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            var psaCenomar = new Documents
            {
                Id = "",
                Value = "",
                Name = "",
                Description = ""
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Expedite" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Regular" });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "", Source = "" });

            _documents.Add(nbi);
            _documents.Add(psaBirthCert);
            _documents.Add(psaMarriageCert);
            _documents.Add(psaDeathCert);
            _documents.Add(psaCenomar);
            return _documents;
        }
    }
}
