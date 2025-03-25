using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using Repository.Migrations;

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
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly IEmailService _emailService;

        public AccountService(
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
            Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, 
            IConfiguration configuration,
            DataBaseContext context,
            ILogService logService,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _connectionString = _context.Database.GetConnectionString();
            _logService = logService;
            _httpContextAccesor = httpContextAccessor;
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<LoginResponseVM> LogOut(string refreshToken)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user != null)
                {
                    user.RefreshToken = null;
                    await _context.SaveChangesAsync();
                }

                    return new LoginResponseVM { Message = "Cierre de sesión exitoso", Success = true };
                
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(LogOut)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }


        public async Task<LoginResponseVM> LoginAsync(LoginVM loginVM)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginVM.Email);
                if (user == null)
                {
                    return new LoginResponseVM { Message = "Usuario o contraseña incorrectos", Success = false };
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var token = await GenerateJwtToken(user, loginVM.RememberMe);
                    var refreshToken = await GenerateRefreshTokenAsync();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                    await _userManager.UpdateAsync(user);
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        //SameSite = SameSiteMode.Lax,
                        //SameSite = SameSiteMode.None,
                        SameSite = SameSiteMode.Strict,
                        Expires = loginVM.RememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddMinutes(120),
                        Path = "/",
                        //Domain = "localhost"
                    };  
                    //_httpContextAccesor.HttpContext.Response.Cookies.Append("accessToken", token, cookieOptions);

                    var refreshTokenOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        //SameSite = SameSiteMode.Lax,
                        //SameSite = SameSiteMode.None,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.Now.AddDays(7),
                        Path = "/",
                        //Domain = "localhost"
                    };
                    //_httpContextAccesor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, refreshTokenOptions);
                    return new LoginResponseVM { Message = "Inicio de sesión exitoso", Token = token, RefreshToken = refreshToken, Success = true };
                }

                return new LoginResponseVM { Message = "Usuario o contraseña incorrectos", Success = false };
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

        private async Task<string> GenerateJwtToken(ApplicationUser user, bool rememberMe)
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
                new Claim("RememberMe", rememberMe.ToString()),
                new Claim("uid", user.Id.ToString())
            };

                // Agregar el rol a los claims
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //Expiración del token con "Remember Me"
                var tokenExpiration = rememberMe ? DateTime.Now.AddDays(6) : DateTime.Now.AddMinutes(120);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: tokenExpiration,
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

                var rememberMe = principal.FindFirst("RememberMe")?.Value;
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);

                return new LoginResponseVM { Message = rememberMe == "True" ? "Token válido (Con rememberMe)" : "Token válido (Sin rememberMe)", Token = token, Success = true };
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

        public async Task<string> GenerateRefreshTokenAsync()
        {
            try
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }
                return Convert.ToBase64String(randomNumber)
                      .Replace('+', '-')
                      .Replace('/', '_')
                      .TrimEnd('=');
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GenerateRefreshTokenAsync)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<LoginResponseVM> RefreshAccessTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return new LoginResponseVM { Message = "El refresh token es requerido." };
                }

                var decodedRefreshToken = Uri.UnescapeDataString(refreshToken);

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == decodedRefreshToken);

                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new LoginResponseVM { Message = "Refresh token inválido o expirado." };
                }

                //genera un nuevo accesToken y refreshToken
                var newToken = await GenerateJwtToken(user, true);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                //se guarda el nuevo refreshToken
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return new LoginResponseVM { Message = "Error al actualizar el usuario, intenta nuevamente." };
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    //SameSite = SameSiteMode.Lax,
                    //SameSite = SameSiteMode.None,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddDays(7),
                    Path = "/",
                    //Domain = "localhost"
                };
                _httpContextAccesor.HttpContext?.Response.Cookies.Append("access_token", newToken, cookieOptions);
                _httpContextAccesor.HttpContext?.Response.Cookies.Append("refresh_token", newRefreshToken, cookieOptions);

                return new LoginResponseVM
                {
                    Message = "Token renovado correctamente",
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(RefreshAccessTokenAsync)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                return user != null && user.RefreshTokenExpiryTime > DateTime.Now;
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ValidateRefreshTokenAsync)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return false;
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
                    RutaFotoPerfil = "Uploads\\FotoPerfil\\FotoPerfilDefecto.png",
                    Biografia = "",
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

        public async Task<EndpointResponse<AccountVM>> GetByEmail(string email)
        {
            try
            {
                // Validar que el email no esté vacío o sea nulo
                if (string.IsNullOrEmpty(email))
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "El email es requerido",
                        Success = false,
                        Data = new AccountVM()
                    };
                }

                // Buscar la persona por email y que no esté marcada como borrada
                var result = await _context.Personas
                    .Include(x => x.Usuario)
                    .FirstOrDefaultAsync(x => x.Usuario.Email == email && x.EsBorrado == false);

                if (result == null)
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "No se encontró la cuenta con ese email",
                        Success = false,
                        Data = new AccountVM()
                    };
                }

                // Obtener los roles del usuario
                var roles = await _userManager.GetRolesAsync(result.Usuario);

                var userRole = roles.FirstOrDefault();
                var roleId = string.Empty;

                if (userRole != null)
                {
                    var role = await _roleManager.FindByNameAsync(userRole);
                    roleId = role?.Id;
                }

                // Mapear los datos a AccountVM
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
                    Mensaje = $"Error en el método {nameof(GetByEmail)}, de la clase {nameof(AccountService)}: {ex.Message}",
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

        public async Task<EndpointResponse<AccountVM>> GetCurrentUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "El ID del usuario es requerido",
                        Success = false,
                        Data = new AccountVM()
                    };
                }

                var result = await _context.Personas
                    .Include(x => x.Usuario)
                    .FirstOrDefaultAsync(x => x.Usuario.Id == userId && x.EsBorrado == false);

                if (result == null)
                {
                    return new EndpointResponse<AccountVM>
                    {
                        Message = "No se encontró la cuenta del usuario autenticado",
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
                    Mensaje = $"Error en el método {nameof(GetCurrentUser)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }
        public async Task<ForgotPasswordResponseVM> FindUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ForgotPasswordResponseVM { Message = "El correo no está registrado", Success = false };
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                user.RefreshToken = resetToken;
                await _userManager.UpdateAsync(user);

                var encodedToken = WebUtility.UrlEncode(resetToken);

                string resetUrl = $"https://localhost:5173/reset-password?token={encodedToken}";

                await _emailService.SendEmailAsync(user.Email, "Restablecer contraseña",
                    $@"<!DOCTYPE html>
                    <html lang='es'>
                        <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Restablecer Contraseña</title>
                        <style>
                        body {{
                                font-family: Arial, sans-serif;
                                background-color: #f3f9f3;
                                color: #333;
                                margin: 0;
                                padding: 0;
                                }}
                                .container {{
                                    max-width: 500px;
                                    margin: 20px auto;
                                    background: #ffffff;
                                    padding: 20px;
                                    border-radius: 8px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                    border: 1px solid #d9e2d3;
                                }}
                                .header {{
                                    text-align: center;
                                    padding-bottom: 20px;
                                    border-bottom: 2px solid #a7c5a6;
                                }}
                                .header h2 {{
                                    color: #2e8b57; 
                                }}
                                .content {{
                                    text-align: center;
                                    padding: 20px 0;
                                }}
                                .button {{
                                    display: inline-block;
                                    background: #4CAF50; 
                                    color: white !important; 
                                    padding: 12px 20px;
                                    border-radius: 5px;
                                    text-decoration: none;
                                    font-size: 16px;
                                    font-weight: bold;
                                    margin-top: 10px;
                                    transition: background-color 0.3s ease;
                                    text-align: center;
                                }}
                                .button:hover {{
                                    background: #388e3c; 
                                }}
                                .footer {{
                                    text-align: center;
                                    font-size: 12px;
                                    color: #666;
                                    margin-top: 20px;
                                }}
                                .footer a {{
                                    color: #4CAF50; 
                                    text-decoration: none;
                                }}
                                .footer a:hover {{
                                    text-decoration: underline;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>Solicitud para Restablecer Contraseña</h2>
                                </div>
                                <div class='content'>
                                    <p>Hemos recibido una solicitud para restablecer tu contraseña. Si no realizaste esta solicitud, ignora este mensaje.</p>
                                    <p>Para cambiar tu contraseña, haz clic en el botón de abajo:</p>
                                    <a href='{resetUrl}' class='button' style='color: white !important;'>Restablecer Contraseña</a> 
                                </div>
                                <div class='footer'>
                                    <p>Este enlace expirará en 1 hora.</p>
                                </div>
                            </div>
                        </body>
                    </html>");



                return new ForgotPasswordResponseVM
                {
                    Message = "Se ha enviado un correo con instrucciones para restablecer la contraseña",
                    Success = true,
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(FindUserByEmailAsync)}, de la clase {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task<ResetPasswordResponseVM> ResetPasswordAsync(ResetPasswordVM model)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.ResetToken);

                if (user == null)
                {
                    return new ResetPasswordResponseVM { Message = "El token no es válido o ha expirado", Success = false };
                }

                var resetResult = await _userManager.ResetPasswordAsync(user, model.ResetToken, model.NewPassword);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    return new ResetPasswordResponseVM { Message = $"Error al restablecer la contraseña: {errors}", Success = false };
                }

                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);

                return new ResetPasswordResponseVM { Message = "Contraseña restablecida exitosamente", Success = true };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en {nameof(ResetPasswordAsync)} en {nameof(AccountService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }



    }
}
