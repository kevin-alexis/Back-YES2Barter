using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Services.Contracts;

namespace Service.Services.Contracts
{
    public interface ITokenService
    {
        string GenerateResetToken(string email);
        string ValidateResetToken(string token);
    }
}
