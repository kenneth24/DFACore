using DFACore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Data
{
    public class DocumentTypes
    {
        public DocumentTypes()
        {
        }

        public List<Documents> Get()
        {
            List<Documents> _documents = new List<Documents>(); ;
            var nbi = new Documents
            {
                Id = "nbiClearance",
                Value = "NBI Clearance",
                Name = "NBI Clearance/Sundry",
                Description = "*Original document issued by National Bureau of Investigation (NBI) with dry seal."
            };

            nbi.Quantities = new List<DocumentInfo>();
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearanceExpedite", Name = "Expedite", Min = 0, Max = 10 });
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearanceRegular", Name = "Regular", Min = 0, Max = 10 });
            nbi.Quantities.Add(new DocumentInfo { Id = "qtyNbiClearance", Name = "Total" });

            nbi.Info = new List<DocumentInfo>();
            nbi.Info.Add(new DocumentInfo { Name = "*Personal Copy is NOT valid", Source = "/images/documents/1 NBI Personal Copy sample (Not Acceptable).png" });
            nbi.Info.Add(new DocumentInfo { Name = "*Sample copy of NBI sundry", Source = "/images/documents/1 Sample NBI Sundry-1.png" });

            var psaBirthCert = new Documents
            {
                Id = "birthCertificate",
                Value = "Birth Certificate",
                Name = "PSA/NSO/Local Civil Registrar Birth Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaBirthCert.Quantities = new List<DocumentInfo>();
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificateExpedite", Name = "Expedite", Min = 0, Max = 10 });
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificateRegular", Name = "Regular", Min = 0, Max = 10 });
            psaBirthCert.Quantities.Add(new DocumentInfo { Id = "qtyBirthCertificate", Name = "Total" });

            psaBirthCert.Info = new List<DocumentInfo>();
            psaBirthCert.Info.Add(new DocumentInfo { Name = "*LCR copy of Birth Certificate (Form 1A)", Source = "/images/documents/2 LCR copy of Birth (Form 1A).png" });

            var psaMarriageCert = new Documents
            {
                Id = "marriageCertificate",
                Value = "Marriage Certificate",
                Name = "PSA/NSO/Local Civil Registrar Marriage Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaMarriageCert.Quantities = new List<DocumentInfo>();
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificateExpedite", Name = "Expedite", Min = 0, Max = 10 });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificateRegular", Name = "Regular", Min = 0, Max = 10 });
            psaMarriageCert.Quantities.Add(new DocumentInfo { Id = "qtyMarriageCertificate", Name = "Total" });

            psaMarriageCert.Info = new List<DocumentInfo>();
            psaMarriageCert.Info.Add(new DocumentInfo { Name = "*Marriage Certificate (LCR Copy, 1A, 3A)", Source = "/images/documents/2 LCR copy of Marriage (Form 3A).png" });

            var psaDeathCert = new Documents
            {
                Id = "deathCertificate",
                Value = "Death Certificate",
                Name = "PSA/NSO/Local Civil Registrar Death Certificate",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaDeathCert.Quantities = new List<DocumentInfo>();
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificateExpedite", Name = "Expedite", Min = 0, Max = 10 });
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificateRegular", Name = "Regular", Min = 0, Max = 10 });
            psaDeathCert.Quantities.Add(new DocumentInfo { Id = "qtyDeathCertificate", Name = "Total" });

            psaDeathCert.Info = new List<DocumentInfo>();
            psaDeathCert.Info.Add(new DocumentInfo { Name = "*LCR copy of Death Certificate (Form 2A)", Source = "/images/documents/2 LCR copy of Death (Form 2A).png" });

            var psaCenomar = new Documents
            {
                Id = "cenomar",
                Value = "Certificate of No Marriage Record",
                Name = "PSA/NSO/Local Civil Registrar Certificate of No Marriage Record (CENOMAR, Advisory on Marriage and/or Negative Records)",
                Description = "*Original document issued by Philippine Statistics Authority (PSA)/ National Statistics Office (NSO).\n*For newly registered records, Local Civil Registrar (LCR) copy should be certified by PSA.\n*Provide an LCR copy of Birth (Form 1A)/ Death (Form 2A)/ Marriage (Form 3A) Certificate if the entries from PSA/NSO are UNCLEAR."
            };

            psaCenomar.Quantities = new List<DocumentInfo>();
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomarExpedite", Name = "Expedite", Min = 0, Max = 10 });
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomarRegular", Name = "Regular", Min = 0, Max = 10 });
            psaCenomar.Quantities.Add(new DocumentInfo { Id = "qtyCenomar", Name = "Total" });

            var school = new Documents
            {
                Id = "elementaryAndHighschool",
                Value = "Form 137 & Diploma",
                Name = "School Documents For Elementary and High School Level (Form 137 and Diploma)",
                Description = "*Certified True Copies from school"
            };

            school.Quantities = new List<DocumentInfo>();
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschoolExpedite", Name = "Expedite", Min = 0, Max = 10 });
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschoolRegular", Name = "Regular", Min = 0, Max = 10 });
            school.Quantities.Add(new DocumentInfo { Id = "qtyElementaryAndHighschool", Name = "Total" });

            school.Info = new List<DocumentInfo>();
            school.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from DepEd Regional Office.", Source = "/images/documents/3a Sample CAV for Elementary and Highschool Level (Form-137 and Diploma).png" });

            var schoolTOR = new Documents
            {
                Id = "techVoch",
                Value = "TOR & Diploma/National Certificate",
                Name = "School Documents For Technical and Vocational Courses (TOR and Diploma/National Certificate)",
                Description = "*Certified True Copies from school"
            };

            schoolTOR.Quantities = new List<DocumentInfo>();
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVochExpedite", Name = "Expedite", Min = 0, Max = 10 });
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVochRegular", Name = "Regular", Min = 0, Max = 10 });
            schoolTOR.Quantities.Add(new DocumentInfo { Id = "qtyTechVoch", Name = "Total" });

            schoolTOR.Info = new List<DocumentInfo>();
            schoolTOR.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from Technical and Skills Development Authority (TESDA).", Source = "/images/documents/3b Sample CAV for Technical and Vocational Courses (TOR and Diploma; National Certificate).png" });

            var schoolCollege = new Documents
            {
                Id = "stateCollegesAndUniversities",
                Value = "State Colleges and Universities TOR & Diploma",
                Name = "School Documents For State Colleges and Universities (TOR and Diploma)s",
                Description = "*Certified True Copies from school"
            };

            schoolCollege.Quantities = new List<DocumentInfo>();
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversitiesExpedite", Name = "Expedite", Min = 0, Max = 10 });
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversitiesRegular", Name = "Regular", Min = 0, Max = 10 });
            schoolCollege.Quantities.Add(new DocumentInfo { Id = "qtyStateCollegesAndUniversities", Name = "Total" });

            schoolCollege.Info = new List<DocumentInfo>();
            schoolCollege.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from the school.", Source = "/images/documents/3c Sample CAV for State Colleges and Universities (TOR and Diploma).png" });

            var schoolPrivateOrLocal = new Documents
            {
                Id = "privateOrLocalColleges",
                Value = "Private/Local Colleges and Universities TOR & Diploma",
                Name = "School Documents For Private/Local Colleges and Universities (TOR and Diploma)",
                Description = "*Certified True Copies from school"
            };

            schoolPrivateOrLocal.Quantities = new List<DocumentInfo>();
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalCollegesExpedite", Name = "Expedite", Min = 0, Max = 10 });
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalCollegesRegular", Name = "Regular", Min = 0, Max = 10 });
            schoolPrivateOrLocal.Quantities.Add(new DocumentInfo { Id = "qtyPrivateOrLocalColleges", Name = "Total" });

            schoolPrivateOrLocal.Info = new List<DocumentInfo>();
            schoolPrivateOrLocal.Info.Add(new DocumentInfo { Name = "*Certification, Authentication and Verification (CAV) from Commission on Higher Education (CHED).", Source = "/images/documents/3d Sample CAV for Private or Local Colleges and Universities (TOR and Diploma).png" });

            var prc = new Documents
            {
                Id = "PRC",
                Value = "PRC Document/s",
                Name = "Professional Regulation Commission (PRC) document/s",
                Description = ""
            };

            prc.Quantities = new List<DocumentInfo>();
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRCExpedite", Name = "Expedite", Min = 0, Max = 10 });
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRCRegular", Name = "Regular", Min = 0, Max = 10 });
            prc.Quantities.Add(new DocumentInfo { Id = "qtyPRC", Name = "Total" });

            prc.Info = new List<DocumentInfo>();
            prc.Info.Add(new DocumentInfo { Name = "*Originally-signed and/or certified true copies from PRC.", Source = "/images/documents/4 PRC Certified True Copy Stamp.png" });

            var medCerts = new Documents
            {
                Id = "medCerts",
                Value = "Medical Certificates",
                Name = "Medical Certificates",
                Description = ""
            };

            medCerts.Quantities = new List<DocumentInfo>();
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCertsExpedite", Name = "Expedite", Min = 0, Max = 10 });
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCertsRegular", Name = "Regular", Min = 0, Max = 10 });
            medCerts.Quantities.Add(new DocumentInfo { Id = "qtyMedCerts", Name = "Total" });

            medCerts.Info = new List<DocumentInfo>();
            medCerts.Info.Add(new DocumentInfo { Name = "*For employement: DOH Stamp per document.", Source = "/images/documents/5 Sample DOH stamp.png" });
            medCerts.Info.Add(new DocumentInfo { Name = "*For other purpose: Certification issued by the DOH with attached Medical Certificate.", Source = "/images/documents/5 Sample DOH Certification.png" });

            var caap = new Documents
            {
                Id = "CAAP",
                Value = "CAAP Documents",
                Name = "Civil Aviation Authority of the Philippines (CAAP) issued document/s",
                Description = ""
            };

            caap.Quantities = new List<DocumentInfo>();
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAPExpedite", Name = "Expedite", Min = 0, Max = 10 });
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAPRegular", Name = "Regular", Min = 0, Max = 10 });
            caap.Quantities.Add(new DocumentInfo { Id = "qtyCAAP", Name = "Total" });

            caap.Info = new List<DocumentInfo>();
            caap.Info.Add(new DocumentInfo { Name = "*Certificate by CAAP.", Source = "/images/documents/6 Sample CAAP Certification.png" });

            var driverLicense = new Documents
            {
                Id = "driverLicense",
                Value = "Driver's License",
                Name = "Driver's License",
                Description = ""
            };

            driverLicense.Quantities = new List<DocumentInfo>();
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicenseExpedite", Name = "Expedite", Min = 0, Max = 10 });
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicenseRegular", Name = "Regular", Min = 0, Max = 10 });
            driverLicense.Quantities.Add(new DocumentInfo { Id = "qtyDriverLicense", Name = "Total" });

            driverLicense.Info = new List<DocumentInfo>();
            driverLicense.Info.Add(new DocumentInfo { Name = "*Certification from Land Transportation Office (LTO Main Branch Only).", Source = "/images/documents/7 Sample LTO Certification.png" });

            var coe = new Documents
            {
                Id = "COE",
                Value = "Certificate of Employment",
                Name = "Documents issued by a private entity : Certificate of Employment",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            coe.Quantities = new List<DocumentInfo>();
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOEExpedite", Name = "Expedite", Min = 0, Max = 20 });
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOERegular", Name = "Regular", Min = 0, Max = 20 });
            coe.Quantities.Add(new DocumentInfo { Id = "qtyCOE", Name = "Total" });

            coe.Info = new List<DocumentInfo>();
            coe.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var invitationLetter = new Documents
            {
                Id = "invitationLetter",
                Value = "Invitation Letter",
                Name = "Documents issued by a private entity : Invitation Letter",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            invitationLetter.Quantities = new List<DocumentInfo>();
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetterExpedite", Name = "Expedite", Min = 0, Max = 20 });
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetterRegular", Name = "Regular", Min = 0, Max = 20 });
            invitationLetter.Quantities.Add(new DocumentInfo { Id = "qtyInvitationLetter", Name = "Total" });

            invitationLetter.Info = new List<DocumentInfo>();
            invitationLetter.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var trainings = new Documents
            {
                Id = "trainings",
                Value = "Trainings",
                Name = "Documents issued by a private entity : Trainings",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            trainings.Quantities = new List<DocumentInfo>();
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainingsExpedite", Name = "Expedite", Min = 0, Max = 20 });
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainingsRegular", Name = "Regular", Min = 0, Max = 20 });
            trainings.Quantities.Add(new DocumentInfo { Id = "qtyTrainings", Name = "Total" });

            trainings.Info = new List<DocumentInfo>();
            trainings.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var baptismalCert = new Documents
            {
                Id = "baptismalCert",
                Value = "Baptismal Certificate",
                Name = "Documents issued by a private entity : Baptismal Certificate",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            baptismalCert.Quantities = new List<DocumentInfo>();
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCertExpedite", Name = "Expedite", Min = 0, Max = 20 });
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCertRegular", Name = "Regular", Min = 0, Max = 20 });
            baptismalCert.Quantities.Add(new DocumentInfo { Id = "qtyBaptismalCert", Name = "Total" });

            baptismalCert.Info = new List<DocumentInfo>();
            baptismalCert.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var privateIssuedOtherDocu = new Documents
            {
                Id = "privateIssuedOtherDocu",
                Value = "Other Document (Private Entity)",
                Name = "Documents issued by a private entity : Other Document",
                Description = "*Notarized Affidavit stating necessary factual circumstances and indicating certificate/s as attachments.\n*Copy of Notarial Commission is not the same as Certificate of Authority for a Notarial Act."
            };

            privateIssuedOtherDocu.Quantities = new List<DocumentInfo>();
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocuExpedite", Name = "Expedite", Min = 0, Max = 20 });
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocuRegular", Name = "Regular", Min = 0, Max = 20 });
            privateIssuedOtherDocu.Quantities.Add(new DocumentInfo { Id = "qtyPrivateIssuedOtherDocu", Name = "Total" });

            privateIssuedOtherDocu.Info = new List<DocumentInfo>();
            privateIssuedOtherDocu.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var specialPowerOfAttorney = new Documents
            {
                Id = "specialPowerOfAttorney",
                Value = "Special Power of Attorney",
                Name = "Notary Public Documents : Special Power of Attorney",
                Description = ""
            };

            specialPowerOfAttorney.Quantities = new List<DocumentInfo>();
            specialPowerOfAttorney.Quantities.Add(new DocumentInfo { Id = "qtySpecialPowerOfAttorneyExpedite", Name = "Expedite", Min = 0, Max = 20 });
            specialPowerOfAttorney.Quantities.Add(new DocumentInfo { Id = "qtySpecialPowerOfAttorneyRegular", Name = "Regular", Min = 0, Max = 20 });
            specialPowerOfAttorney.Quantities.Add(new DocumentInfo { Id = "qtySpecialPowerOfAttorney", Name = "Total" });

            specialPowerOfAttorney.Info = new List<DocumentInfo>();
            specialPowerOfAttorney.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var moa = new Documents
            {
                Id = "MOA",
                Value = "Memorandum of Agreement",
                Name = "Notary Public Documents : Memorandum of Agreement",
                Description = ""
            };

            moa.Quantities = new List<DocumentInfo>();
            moa.Quantities.Add(new DocumentInfo { Id = "qtyMOAExpedite", Name = "Expedite", Min = 0, Max = 20 });
            moa.Quantities.Add(new DocumentInfo { Id = "qtyMOARegular", Name = "Regular", Min = 0, Max = 20 });
            moa.Quantities.Add(new DocumentInfo { Id = "qtyMOA", Name = "Total" });

            moa.Info = new List<DocumentInfo>();
            moa.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var mou = new Documents
            {
                Id = "MOU",
                Value = "Memorandum of Understanding",
                Name = "Notary Public Documents : Memorandum of Understanding",
                Description = ""
            };

            mou.Quantities = new List<DocumentInfo>();
            mou.Quantities.Add(new DocumentInfo { Id = "qtyMOUExpedite", Name = "Expedite", Min = 0, Max = 20 });
            mou.Quantities.Add(new DocumentInfo { Id = "qtyMOURegular", Name = "Regular", Min = 0, Max = 20 });
            mou.Quantities.Add(new DocumentInfo { Id = "qtyMOU", Name = "Total" });

            mou.Info = new List<DocumentInfo>();
            mou.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var anyContract = new Documents
            {
                Id = "anyContract",
                Value = "Contract",
                Name = "Notary Public Documents : Any Form of Contract",
                Description = ""
            };

            anyContract.Quantities = new List<DocumentInfo>();
            anyContract.Quantities.Add(new DocumentInfo { Id = "qtyAnyContractExpedite", Name = "Expedite", Min = 0, Max = 20 });
            anyContract.Quantities.Add(new DocumentInfo { Id = "qtyAnyContractRegular", Name = "Regular", Min = 0, Max = 20 });
            anyContract.Quantities.Add(new DocumentInfo { Id = "qtyAnyContract", Name = "Total" });

            anyContract.Info = new List<DocumentInfo>();
            anyContract.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var affidavitOfConsent = new Documents
            {
                Id = "affidavitOfConsent",
                Value = "Affidavit of Consent/Advice",
                Name = "Notary Public Documents : Affidavit of Consent or Advise",
                Description = ""
            };

            affidavitOfConsent.Quantities = new List<DocumentInfo>();
            affidavitOfConsent.Quantities.Add(new DocumentInfo { Id = "qtyAffidavitOfConsentExpedite", Name = "Expedite", Min = 0, Max = 20 });
            affidavitOfConsent.Quantities.Add(new DocumentInfo { Id = "qtyAffidavitOfConsentRegular", Name = "Regular", Min = 0, Max = 20 });
            affidavitOfConsent.Quantities.Add(new DocumentInfo { Id = "qtyAffidavitOfConsent", Name = "Total" });

            affidavitOfConsent.Info = new List<DocumentInfo>();
            affidavitOfConsent.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var jointAffidavit = new Documents
            {
                Id = "jointAffidavit",
                Value = "Joint Affidavit",
                Name = "Notary Public Documents : Joint Affidavit",
                Description = ""
            };

            jointAffidavit.Quantities = new List<DocumentInfo>();
            jointAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyJointAffidavitExpedite", Name = "Expedite", Min = 0, Max = 20 });
            jointAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyJointAffidavitRegular", Name = "Regular", Min = 0, Max = 20 });
            jointAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyJointAffidavit", Name = "Total" });

            jointAffidavit.Info = new List<DocumentInfo>();
            jointAffidavit.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var otherAffidavit = new Documents
            {
                Id = "otherAffidavit",
                Value = "Other Affidavit",
                Name = "Notary Public Documents : Other Affidavit",
                Description = ""
            };

            otherAffidavit.Quantities = new List<DocumentInfo>();
            otherAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyOtherAffidavitExpedite", Name = "Expedite", Min = 0, Max = 20 });
            otherAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyOtherAffidavitRegular", Name = "Regular", Min = 0, Max = 20 });
            otherAffidavit.Quantities.Add(new DocumentInfo { Id = "qtyOtherAffidavit", Name = "Total" });

            otherAffidavit.Info = new List<DocumentInfo>();
            otherAffidavit.Info.Add(new DocumentInfo { Name = "*Certificate of Authority for a Notarial Act (CANA) signed by the Executive Judge/Vice-Executive Judge/any office authorized signatories (issued by the Regional Trial Court).", Source = "/images/documents/9 Sample Certificate of Authority for a Notarial Act (CANA).png" });

            var courtDecision = new Documents
            {
                Id = "courtDecision",
                Value = "Court Decision",
                Name = "Court Document/s : Decision",
                Description = ""
            };

            courtDecision.Quantities = new List<DocumentInfo>();
            courtDecision.Quantities.Add(new DocumentInfo { Id = "qtyCourtDecisionExpedite", Name = "Expedite", Min = 0, Max = 10 });
            courtDecision.Quantities.Add(new DocumentInfo { Id = "qtyCourtDecisionRegular", Name = "Regular", Min = 0, Max = 10 });
            courtDecision.Quantities.Add(new DocumentInfo { Id = "qtyCourtDecision", Name = "Total" });

            courtDecision.Info = new List<DocumentInfo>();
            courtDecision.Info.Add(new DocumentInfo { Name = "*Certified True Copies from the Court.", Source = "/images/documents/10 Court Document Certified True Copy Stamp.png" });

            var courtResolution = new Documents
            {
                Id = "courtResolution",
                Value = "Court Resolution/Order",
                Name = "Court Document/s : Resolution/Order",
                Description = ""
            };

            courtResolution.Quantities = new List<DocumentInfo>();
            courtResolution.Quantities.Add(new DocumentInfo { Id = "qtyCourtResolutionExpedite", Name = "Expedite", Min = 0, Max = 10 });
            courtResolution.Quantities.Add(new DocumentInfo { Id = "qtyCourtResolutionRegular", Name = "Regular", Min = 0, Max = 10 });
            courtResolution.Quantities.Add(new DocumentInfo { Id = "qtyCourtResolution", Name = "Total" });

            courtResolution.Info = new List<DocumentInfo>();
            courtResolution.Info.Add(new DocumentInfo { Name = "*Certified True Copies from the Court.", Source = "/images/documents/10 Court Document Certified True Copy Stamp.png" });

            var courtOtherDocument = new Documents
            {
                Id = "courtOtherDocument",
                Value = "Other Court Documents",
                Name = "Court Document/s : Other Court Documents",
                Description = ""
            };

            courtOtherDocument.Quantities = new List<DocumentInfo>();
            courtOtherDocument.Quantities.Add(new DocumentInfo { Id = "qtyCourtOtherDocumentExpedite", Name = "Expedite", Min = 0, Max = 10 });
            courtOtherDocument.Quantities.Add(new DocumentInfo { Id = "qtyCourtOtherDocumentRegular", Name = "Regular", Min = 0, Max = 10 });
            courtOtherDocument.Quantities.Add(new DocumentInfo { Id = "qtyCourtOtherDocument", Name = "Total" });

            courtOtherDocument.Info = new List<DocumentInfo>();
            courtOtherDocument.Info.Add(new DocumentInfo { Name = "*Certified True Copies from the Court.", Source = "/images/documents/10 Court Document Certified True Copy Stamp.png" });

            var immigration = new Documents
            {
                Id = "Immigration",
                Value = "Immigration Record/s",
                Name = "Immigration Record/s",
                Description = ""
            };

            immigration.Quantities = new List<DocumentInfo>();
            immigration.Quantities.Add(new DocumentInfo { Id = "qtyImmigrationExpedite", Name = "Expedite", Min = 0, Max = 10 });
            immigration.Quantities.Add(new DocumentInfo { Id = "qtyImmigrationRegular", Name = "Regular", Min = 0, Max = 10 });
            immigration.Quantities.Add(new DocumentInfo { Id = "qtyImmigration", Name = "Total" });

            immigration.Info = new List<DocumentInfo>();
            immigration.Info.Add(new DocumentInfo { Name = "*Certified by Bureau of Immigration.", Source = "/images/documents/11 Sample Immigration Documents.png" });

            var DSWDClearance = new Documents
            {
                Id = "DSWDClearance",
                Value = "DSWD Clearance",
                Name = "DSWD Clearance",
                Description = ""
            };

            DSWDClearance.Quantities = new List<DocumentInfo>();
            DSWDClearance.Quantities.Add(new DocumentInfo { Id = "qtyDSWDClearanceExpedite", Name = "Expedite", Min = 0, Max = 10 });
            DSWDClearance.Quantities.Add(new DocumentInfo { Id = "qtyDSWDClearanceRegular", Name = "Regular", Min = 0, Max = 10 });
            DSWDClearance.Quantities.Add(new DocumentInfo { Id = "qtyDSWDClearance", Name = "Total" });

            DSWDClearance.Info = new List<DocumentInfo>();
            DSWDClearance.Info.Add(new DocumentInfo { Name = "*Original or certified true copies issued by the Department of Social Welfare and Development.", Source = "/images/documents/12 Sample DSWD Clearance-1.png" });
            DSWDClearance.Info.Add(new DocumentInfo { Name = "", Source = "/images/documents/12 Sample DSWD Clearance-2.png" });

            var policeClearance = new Documents
            {
                Id = "policeClearance",
                Value = "Police Clearance",
                Name = "Police Clearance/Sundry",
                Description = ""
            };

            policeClearance.Quantities = new List<DocumentInfo>();
            policeClearance.Quantities.Add(new DocumentInfo { Id = "qtyPoliceClearanceExpedite", Name = "Expedite", Min = 0, Max = 10 });
            policeClearance.Quantities.Add(new DocumentInfo { Id = "qtyPoliceClearanceRegular", Name = "Regular", Min = 0, Max = 10 });
            policeClearance.Quantities.Add(new DocumentInfo { Id = "qtyPoliceClearance", Name = "Total" });

            policeClearance.Info = new List<DocumentInfo>();
            policeClearance.Info.Add(new DocumentInfo { Name = "*Original document issued by Philippine National Police (PNP).", Source = "/images/documents/13 Sample PNP Certification.png" });
            policeClearance.Info.Add(new DocumentInfo { Name = "*Sample copy of PNP Sundry.", Source = "/images/documents/13 Sample PNP Sundry-1.png" });
            policeClearance.Info.Add(new DocumentInfo { Name = "", Source = "/images/documents/13 Sample PNP Sundry-2.png" });

            var businessRegistration = new Documents
            {
                Id = "businessRegistration",
                Value = "Business Registration & Other Documents issued by a Government Agency",
                Name = "Business Registration & Other Documents issued by a Government Agency (e.g. SEC, DTI, SSS, BIR, Municipal Business Permit and Licensing Office, etc.)",
                Description = ""
            };

            businessRegistration.Quantities = new List<DocumentInfo>();
            businessRegistration.Quantities.Add(new DocumentInfo { Id = "qtyBusinessRegistrationExpedite", Name = "Expedite", Min = 0, Max = 20 });
            businessRegistration.Quantities.Add(new DocumentInfo { Id = "qtyBusinessRegistrationRegular", Name = "Regular", Min = 0, Max = 20 });
            businessRegistration.Quantities.Add(new DocumentInfo { Id = "qtyBusinessRegistration", Name = "Total" });

            businessRegistration.Info = new List<DocumentInfo>();
            businessRegistration.Info.Add(new DocumentInfo { Name = "*Certified true copy/ies from the issuing office.", Source = "/images/documents/14 SEC GIS CERTIFIED TRUE COPY AND SSS CERTIFICATION-1.png" });
            businessRegistration.Info.Add(new DocumentInfo { Name = "", Source = "/images/documents/14 SEC GIS CERTIFIED TRUE COPY AND SSS CERTIFICATION-2.png" });

            var barangayClearance = new Documents
            {
                Id = "barangayClearance",
                Value = "Barangay Clearance/Certifcate",
                Name = "Barangay Clearance/Certifcate",
                Description = ""
            };

            barangayClearance.Quantities = new List<DocumentInfo>();
            barangayClearance.Quantities.Add(new DocumentInfo { Id = "qtyBarangayClearanceExpedite", Name = "Expedite", Min = 0, Max = 10 });
            barangayClearance.Quantities.Add(new DocumentInfo { Id = "qtyBarangayClearanceRegular", Name = "Regular", Min = 0, Max = 10 });
            barangayClearance.Quantities.Add(new DocumentInfo { Id = "qtyBarangayClearance", Name = "Total" });

            barangayClearance.Info = new List<DocumentInfo>();
            barangayClearance.Info.Add(new DocumentInfo { Name = "*Mayor's certification/clearance which has a jurisdiction over the Barangay.", Source = "/images/documents/15 Sample Mayor's Certification.png" });

            var exportDocuments = new Documents
            {
                Id = "exportDocuments",
                Value = "Export Document/s",
                Name = "Export Document/s",
                Description = ""
            };

            exportDocuments.Quantities = new List<DocumentInfo>();
            exportDocuments.Quantities.Add(new DocumentInfo { Id = "qtyExportDocumentsExpedite", Name = "Expedite", Min = 0, Max = 50 });
            exportDocuments.Quantities.Add(new DocumentInfo { Id = "qtyExportDocumentsRegular", Name = "Regular", Min = 0, Max = 50 });
            exportDocuments.Quantities.Add(new DocumentInfo { Id = "qtyExportDocuments", Name = "Total" });

            exportDocuments.Info = new List<DocumentInfo>();
            exportDocuments.Info.Add(new DocumentInfo { Name = "*Certified by Philippine Chamber of Commerce (PCCI), Department Of Health (DOH), Department of Agriculture (DA) or by the Food and Drug Administration (FDA) depending on the nature of the document.", Source = "/images/documents/16 Sample DOH Quarantine Certification and PCCI Certification-1.png" });
            exportDocuments.Info.Add(new DocumentInfo { Name = "", Source = "/images/documents/16 Sample DOH Quarantine Certification and PCCI Certification-2.png" });

            var phEmbassy = new Documents
            {
                Id = "phEmbassy",
                Value = "Documents issued by Philippine Embassy/Consulate",
                Name = "Documents issued by Philippine Embassy/Consulate",
                Description = "For issuance of DFA which will used in the Philippines only"
            };

            phEmbassy.Quantities = new List<DocumentInfo>();
            phEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyPHEmbassyExpedite", Name = "Expedite", Min = 0, Max = 10 });
            phEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyPHEmbassyRegular", Name = "Regular", Min = 0, Max = 10 });
            phEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyPHEmbassy", Name = "Total" });

            var foreignEmbassy = new Documents
            {
                Id = "foreignEmbassy",
                Value = "Documents issued by Foreign Embassy/Consulate in the Philippines",
                Name = "Documents issued by Foreign Embassy/Consulate in the Philippines",
                Description = "For issuance of DFA which will used in the Philippines only"
            };

            foreignEmbassy.Quantities = new List<DocumentInfo>();
            foreignEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyForeignEmbassyExpedite", Name = "Expedite", Min = 0, Max = 10 });
            foreignEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyForeignEmbassyRegular", Name = "Regular", Min = 0, Max = 10 });
            foreignEmbassy.Quantities.Add(new DocumentInfo { Id = "qtyForeignEmbassy", Name = "Total" });

            _documents.Add(nbi);
            _documents.Add(psaBirthCert);
            _documents.Add(psaMarriageCert);
            _documents.Add(psaDeathCert);
            _documents.Add(psaCenomar);
            _documents.Add(school);
            _documents.Add(schoolTOR);
            _documents.Add(schoolCollege);
            _documents.Add(schoolPrivateOrLocal);
            _documents.Add(prc);
            _documents.Add(medCerts);
            _documents.Add(caap);
            _documents.Add(driverLicense);
            _documents.Add(coe);
            _documents.Add(invitationLetter);
            _documents.Add(trainings);
            _documents.Add(baptismalCert);
            _documents.Add(privateIssuedOtherDocu);
            _documents.Add(specialPowerOfAttorney);
            _documents.Add(moa);
            _documents.Add(mou);
            _documents.Add(anyContract);
            _documents.Add(affidavitOfConsent);
            _documents.Add(jointAffidavit);
            _documents.Add(otherAffidavit);
            _documents.Add(courtDecision);
            _documents.Add(courtResolution);
            _documents.Add(courtOtherDocument);
            _documents.Add(immigration);
            _documents.Add(DSWDClearance);
            _documents.Add(policeClearance);
            _documents.Add(businessRegistration);
            _documents.Add(barangayClearance);
            _documents.Add(exportDocuments);
            _documents.Add(phEmbassy);
            _documents.Add(foreignEmbassy);

            return _documents;
        }
    }
}
