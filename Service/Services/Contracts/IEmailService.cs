using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IEmailService
    {
        Task SendResetPasswordEmail(string email, string resetLink);
    }
}
