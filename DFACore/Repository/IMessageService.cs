using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public interface IMessageService
    {
        Task SendEmailAsync(
           string toName,
           string toEmailAddress,
           string subject,
           string message,
           params Attachment[] attachments);

    }
}
