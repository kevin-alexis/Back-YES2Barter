using Domain.Entities;
using Repository.Context;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;


namespace Service.Services.Implementation
{
    public class UserService : IUserService
    {
        public async Task<User> GetByEmailAsync(string email)
        {
            return await Task.FromResult(new User { Email = email, PasswordHash = "hashed_password" });
        }

        public async Task UpdateAsync(User user)
        {
            await Task.CompletedTask;
        }
    }

}
