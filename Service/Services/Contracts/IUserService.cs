using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IUserService
    {
        Task<User> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
    }
}
