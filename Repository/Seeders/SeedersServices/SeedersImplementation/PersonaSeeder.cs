using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Repository.Context;
using Repository.Seeders.SeedersServices.SeedersContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Seeders.SeedersServices.SeedersImplementation
{
    public class PersonaSeeder : ISeeder
    {
        private readonly DataBaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PersonaSeeder(DataBaseContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            if (!_context.Personas.Any())
            {
                // Crear roles si no existen
                var roles = new[] { "Administrador", "Intercambiador" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Crear usuarios y asignar roles
                var users = new List<(ApplicationUser user, string role)>
                {
                    (new ApplicationUser { UserName = "admin@yestobarter.com", Email = "admin@yestobarter.com" }, "Administrador"),
                    (new ApplicationUser { UserName = "intercambiador@yestobarter.com", Email = "Intercambiador@yestobarter.com" }, "Intercambiador"),
                    (new ApplicationUser { UserName = "intercambiador2@yestobarter.com", Email = "Intercambiador2@yestobarter.com" }, "Intercambiador"),
                };

                foreach (var (user, role) in users)
                {
                    var result = await _userManager.CreateAsync(user, "Contraseña1234?");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }

                // Crear personas
                var personas = new List<Persona>
                {
                    new Persona { Nombre = "Administrador", Biografia="Biografía del administrador", IdUsuario = users[0].user.Id, RutaFotoPerfil = "Uploads\\FotoPerfil\\FotoPerfilDefecto.png" }, // Admin
                    new Persona { Nombre = "Intercambiador 1", Biografia="Biografía de Intercambiador 1", IdUsuario = users[1].user.Id, RutaFotoPerfil = "Uploads\\FotoPerfil\\FotoPerfilDefecto.png" }, // Intercambiador
                    new Persona { Nombre = "Intercambiador 2", Biografia="Biografía de Intercambiador 2", IdUsuario = users[2].user.Id, RutaFotoPerfil = "Uploads\\FotoPerfil\\FotoPerfilDefecto.png" }, // Intercambiador
                };

                _context.Personas.AddRange(personas);
                await _context.SaveChangesAsync();
            }
        }
    }
}
