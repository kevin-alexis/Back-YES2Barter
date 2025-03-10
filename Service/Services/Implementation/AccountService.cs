using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Account;
using Domain.ViewModels.CloseChat;
using Domain.ViewModels.CreateAccountVM;
using Domain.ViewModels.Login;
using Domain.ViewModels.Response;
using Domain.ViewModels.UpdateAccountVM;
using global::Service.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;

namespace Service.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly DataBaseContext _context;
        private readonly string _connectionString;
        private readonly ILogService _logService;
        public AccountService(
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
            Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, 
            IConfiguration configuration,
            DataBaseContext context,
            ILogService logService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _connectionString = _context.Database.GetConnectionString();
            _logService = logService;
        }

        public async Task<LoginResponseVM> LoginAsync(LoginVM loginVM)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginVM.Email);
                if (user == null)
                {
                    return new LoginResponseVM { Message = "Usuario o contraseña incorrectos" };
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var token = await GenerateJwtToken(user);
                    return new LoginResponseVM { Message = "Inicio de sesión exitoso", Token = token, Success = true };
                }

                return new LoginResponseVM { Message = "Usuario o contraseña incorrectos" };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(LoginAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("uid", user.Id.ToString())
            };

                // Agregar el rol a los claims
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(3000),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GenerateJwtToken)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<LoginResponseVM> ValidateJwtToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = key
                }, out var validatedToken);

                if (principal == null)
                {
                    return new LoginResponseVM { Message = "Token Invalido" };
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                return new LoginResponseVM { Message = "Token Valido", Token = token, Success = true };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ValidateJwtToken)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return new LoginResponseVM { Message = "Token Invalido" };
            }
        }

        public async Task<List<IdentityRole>> GetAllRoles()
        {
            try
            {
                List<IdentityRole> roles = new List<IdentityRole>();

                roles = await _roleManager.Roles.ToListAsync();
                return roles;
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllRoles)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<EndpointResponse<string>> CreateAccountAsync(CreateAccountVM createAccountVM)
        {
            try
            {
                var result = await CreateUserAsync(createAccountVM);
                
                if (!result.Success)
                {
                    return new EndpointResponse<string> { Message = result.Message, Success = false };
                }

                Persona persona = new Persona
                {
                    Nombre = createAccountVM.Nombre,
                    IdUsuario = result.Data,
                    EsBorrado = false
                };

                var resultPersona = await _context.Personas.AddAsync(persona);
                var saveChangesResult = await _context.SaveChangesAsync();

                if (saveChangesResult <= 0)
                {
                    var user = await _userManager.FindByIdAsync(result.Data);
                    await _userManager.DeleteAsync(user);
                    return new EndpointResponse<string> { Message = "Error al crear la persona", Success = false, Data = null };
                }

                return new EndpointResponse<string> { Message = "Cuenta creada exitosamente", Success = true };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(CreateAccountAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return new EndpointResponse<string>() { Message = $"Error inesperado: {ex.Message}", Success = false, Data = null };
            }
        }

        public async Task<EndpointResponse<string>> CreateUserAsync(CreateAccountVM createAccountVM)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(createAccountVM.Email);
                if (user != null)
                {
                    return new EndpointResponse<string> { Message = "Ya existe un usuario con ese correo electrónico", Success = false, Data = null };
                }

                user = new ApplicationUser
                {
                    UserName = createAccountVM.Email,
                    Email = createAccountVM.Email,
                };

                var createUserResult = await _userManager.CreateAsync(user, createAccountVM.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    return new EndpointResponse<string> { Message = $"Error al crear el usuario: {errors}", Success = false, Data = null };
                }

                var role = await _roleManager.FindByIdAsync(createAccountVM.IdRol);
                //if (role == null)
                //{
                //    await _userManager.DeleteAsync(user);
                //    return new EndpointResponse<string> { Message = "El rol especificado no existe", Success = false, Data = null };
                //}

                var addToRoleResult = await _userManager.AddToRoleAsync(user, role?.Name ?? "Intercambiador");
                if (!addToRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    return new EndpointResponse<string> { Message = $"Error al asignar el rol: {errors}", Success = false, Data = null };
                }

                return new EndpointResponse<string> { Message = "Cuenta creada exitosamente", Success = true, Data = user.Id };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(CreateUserAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return new EndpointResponse<string>() { Message = $"Error inesperado: {ex.Message}", Success = false, Data = null };
            }
        }

        public async Task<EndpointResponse<string>> UpdateAccountAsync(UpdateAccountVM updateAccountVM, int IdPersona)
        {
            try
            {
                var persona = await _context.Personas.FirstOrDefaultAsync(p => p.Id == IdPersona);
                if (persona == null)
                {
                    return new EndpointResponse<string> { Message = "La persona no existe", Success = false, Data = null };
                }

                persona.Nombre = updateAccountVM.Nombre;
                _context.Personas.Update(persona);
                var updatePersonaResult = await _context.SaveChangesAsync();
                if (updatePersonaResult <= 0)
                {
                    return new EndpointResponse<string> { Message = "Error al actualizar la persona", Success = false, Data = null };
                }

                var user = await _userManager.FindByIdAsync(persona.IdUsuario);
                if (user == null)
                {
                    return new EndpointResponse<string> { Message = "El usuario no existe", Success = false, Data = null };
                }

                user.UserName = updateAccountVM.Email;

                if (user.Email != updateAccountVM.Email)
                {
                    var emailChangeToken = await _userManager.GenerateChangeEmailTokenAsync(user, updateAccountVM.Email);
                    var emailChangeResult = await _userManager.ChangeEmailAsync(user, updateAccountVM.Email, emailChangeToken);
                    if (!emailChangeResult.Succeeded)
                    {
                        var errors = string.Join(", ", emailChangeResult.Errors.Select(e => e.Description));
                        return new EndpointResponse<string> { Message = $"Ya existe un usuario con ese correo electrónico", Success = false, Data = null };
                    }
                }

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    var errors = string.Join(", ", updateUserResult.Errors.Select(e => e.Description));
                    return new EndpointResponse<string> { Message = $"Error al actualizar el usuario: {errors}", Success = false, Data = null };
                }

                if (!string.IsNullOrEmpty(updateAccountVM.Password))
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                    {
                        var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                        return new EndpointResponse<string> { Message = $"Error al eliminar la contraseña existente: {errors}", Success = false, Data = null };
                    }

                    var addPasswordResult = await _userManager.AddPasswordAsync(user, updateAccountVM.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                        return new EndpointResponse<string> { Message = $"Error al establecer la nueva contraseña: {errors}", Success = false, Data = null };
                    }
                }

                if (!string.IsNullOrEmpty(updateAccountVM.IdRol))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var role = await _roleManager.FindByIdAsync(updateAccountVM.IdRol);

                    if (!currentRoles.Contains(role.Name))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        var roleUpdateResult = await _userManager.AddToRoleAsync(user, role.Name);

                        if (!roleUpdateResult.Succeeded)
                        {
                            var roleErrors = string.Join(", ", roleUpdateResult.Errors.Select(e => e.Description));
                            return new EndpointResponse<string> { Message = $"Error al actualizar el rol del usuario: {roleErrors}", Success = false, Data = null };
                        }
                    }
                }

                return new EndpointResponse<string> { Message = "Cuenta actualizada exitosamente", Success = true, Data = user.Id };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(UpdateAccountAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return new EndpointResponse<string>() { Message = $"Error inesperado: {ex.Message}", Success = false, Data = null };
            }
        }
        public async Task<EndpointResponse<List<AccountVM>>> GetAllAccounts()
        {
            try
            {
                var result = new List<AccountVM>();

                var personas = _context.Personas
                    .Where(p => p.EsBorrado == false) 
                    .Include(p => p.Usuario)
                    .ToList();

                foreach (var persona in personas)
                {
                    var user = persona.Usuario; 
                    var roles = await _userManager.GetRolesAsync(user);

                    foreach (var role in roles)
                    {
                        result.Add(new AccountVM
                        {
                            IdUsuario = user.Id,
                            Email = user.Email,
                            NumeroDeTelefono = user.PhoneNumber,
                            IdRol = role,
                            Rol = role,
                            IdPersona = persona.Id.ToString(),
                            Nombre = persona.Nombre
                        });
                    }
                }

                if (!result.Any())
                {
                    return new EndpointResponse<List<AccountVM>>
                    {
                        Message = "No hay usuarios registrados",
                        Success = true,
                        Data = new List<AccountVM>()
                    };
                }

                return new EndpointResponse<List<AccountVM>>
                {
                    Message = "Usuarios obtenidos exitosamente",
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllAccounts)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;

            }
        }

        public async Task<EndpointResponse<AccountVM>> GetById(int idPersona)
        {
            try
            {
                if (idPersona <= 0)
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "El id es requerido",
                        Success = false,
                        Data = new AccountVM()
                    };
                }

                var result = await _context.Personas
                    .Include(x => x.Usuario)
                    .FirstOrDefaultAsync(x => x.Id == idPersona && x.EsBorrado == false);

                if (result == null)
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "No se encontró la cuenta con ese id",
                        Success = false,
                        Data = new AccountVM()
                    };
                }

                var roles = await _userManager.GetRolesAsync(result.Usuario);

                var userRole = roles.FirstOrDefault();
                var roleId = string.Empty;

                if (userRole != null)
                {
                    var role = await _roleManager.FindByNameAsync(userRole);
                    roleId = role?.Id;
                }

                var accountVM = new AccountVM
                {
                    IdPersona = result.Id.ToString(),
                    Nombre = result.Nombre,
                    Email = result.Usuario.Email,
                    IdUsuario = result.Usuario.Id.ToString(),
                    Rol = userRole ?? "No Role",
                    IdRol = roleId ?? "No Role Id"
                };

                return new EndpointResponse<AccountVM>
                {
                    Message = "Cuenta obtenida con éxito",
                    Success = true,
                    Data = accountVM
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetById)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<EndpointResponse<string>> DeleteAccountAsync(int id)
        {
            try
            {
                var person = await _context.Personas.FirstOrDefaultAsync(p => p.Id == id);
                var user = await _userManager.FindByIdAsync(person.IdUsuario);
                
                if (user == null)
                {
                    return new EndpointResponse<string> { Message = "El usuario no existe", Success = false, Data = null };
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new EndpointResponse<string> { Message = $"Error al eliminar el usuario: {errors}", Success = false, Data = null };
                }

                var resultPersona = _context.Personas.Remove(person);
                _context.SaveChanges();
                return new EndpointResponse<string> { Message = "Cuenta eliminada exitosamente", Success = true, Data = null };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(DeleteAccountAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return new EndpointResponse<string>() { Message = $"Error inesperado: {ex.Message}", Success = false, Data = null };
            }
        }

    }
}
