using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Services
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string email, string subject, string content);
    }
}
