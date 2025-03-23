using Domain.Entities;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class UserService : IUserService
    {
        public async Task<User> GetByEmailAsync(string email)
        {
            // Implementar lógica para obtener usuario
            return await Task.FromResult(new User { Email = email, PasswordHash = "hashed_password" });
        }

        public async Task UpdateAsync(User user)
        {
            // Implementar lógica para actualizar usuario
            await Task.CompletedTask;
        }
    }
}
