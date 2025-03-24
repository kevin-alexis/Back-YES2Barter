using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Account
{
    public class ForgotPasswordResponseVM
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string ResetToken { get; set; } 
    }
}
