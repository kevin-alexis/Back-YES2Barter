using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Account
{
    public class ResetPasswordVM
    {
        //public string Email { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
}
