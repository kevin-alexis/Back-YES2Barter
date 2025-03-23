using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class EmailService : IEmailService
    {
        public async Task SendResetPasswordEmail(string email, string resetLink)
        {
            // Implementar lógica para enviar correo electrónico
            await Task.CompletedTask;
        }
    }
}
