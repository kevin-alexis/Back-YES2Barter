using Domain.ViewModels.Account;
using Domain.ViewModels.CreateAccountVM;
using Domain.ViewModels.Login;
using Domain.ViewModels.Response;
using Domain.ViewModels.UpdateAccountVM;
using Microsoft.AspNetCore.Identity;
using Service.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IAccountService
    {
        Task<List<IdentityRole>> GetAllRoles();
        Task<LoginResponseVM> LoginAsync(LoginVM loginVM);
        Task<LoginResponseVM> ValidateJwtToken(string token);
        Task<EndpointResponse<string>> CreateAccountAsync(CreateAccountVM createAccountVM);
        Task<EndpointResponse<string>> DeleteAccountAsync(int id);
        Task<EndpointResponse<List<AccountVM>>> GetAllAccounts();
        Task<EndpointResponse<string>> UpdateAccountAsync(UpdateAccountVM updateAccountVM, int IdPersona);
        Task<EndpointResponse<AccountVM>> GetById(int idPersona);
    }
}
