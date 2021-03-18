using DFACore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public interface IApplicantRecordRepository
    {
        
        IEnumerable<ApplicantRecord> GetAll();
        bool Add(ApplicantRecord applicantRecord);
        bool Update(ApplicantRecord applicantRecord);
        bool Delete(long id);
        bool ValidateScheduleDate(DateTime date);
        bool ValidateScheduleDate(DateTime date, int applicationCount);
    }
}
