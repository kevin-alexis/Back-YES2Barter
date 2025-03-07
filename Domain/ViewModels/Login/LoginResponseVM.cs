using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Login
{
    public class LoginResponseVM
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; } = false;
    }
}
