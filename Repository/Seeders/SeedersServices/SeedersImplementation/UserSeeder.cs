using Domain.Entities;
using Repository.Seeders.SeedersServices.SeedersContracts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Seeders.SeedersServices.SeedersImplementation
{
    public class UserSeeder : ISeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Crear los roles si no existen
            await CreateRoleIfNotExists("Administrador");
            await CreateRoleIfNotExists("Intercambiador");

            // Crear el usuario administrador si no existe
            var adminUser = await _userManager.FindByEmailAsync("admin@yestobarter.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@yestobarter.com",
                    Email = "admin@yestobarter.com"
                };
                await _userManager.CreateAsync(adminUser, "Contraseña1234?");
                await _userManager.AddToRoleAsync(adminUser, "Administrador");
            }
            var intercambiadorUser = await _userManager.FindByEmailAsync("intercambiador@yestobarter.com");
            if (intercambiadorUser == null)
            {
                intercambiadorUser = new ApplicationUser
                {
                    UserName = "intercambiador@yestobarter.com",
                    Email = "intercambiador@yestobarter.com"
                };
                await _userManager.CreateAsync(intercambiadorUser, "Contraseña1234?");
                await _userManager.AddToRoleAsync(intercambiadorUser, "Intercambiador");
            }

        }

        private async Task CreateRoleIfNotExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
