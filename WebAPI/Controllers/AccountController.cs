using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Account;
using Domain.ViewModels.CreateAccountVM;
using Domain.ViewModels.Login;
using Domain.ViewModels.Response;
using Domain.ViewModels.UpdateAccountVM;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Service.Services;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System.ComponentModel;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogService _logService;


        public AccountController(IAccountService accountService, ILogService logService)
        {
            _accountService = accountService;
            _logService = logService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            try
            {
                var result = await _accountService.LoginAsync(loginVM);
                if (result.Success == true)
                {
                    Response.Cookies.Append("access_token", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(120),
                        Path = "/"
                    });

                    Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Path = "/"
                    });

                    return Ok(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(Login)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
            {
            var refreshToken = Request.Cookies["refresh_token"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _accountService.LogOut(refreshToken);
                
            }

            // Asegurar la eliminación de las cookies con opciones adecuadas
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1) // Forzar expiración
            };

            Response.Cookies.Append("access_token", "", cookieOptions);
            Response.Cookies.Append("refresh_token", "", cookieOptions);

            return Ok(new { message = "Sesión cerrada exitosamente." });
        }



        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];

            //Console.WriteLine($"refreshToken recibido: {refreshToken}");
            //Console.WriteLine($"Cookies recibidas: {string.Join(", ", Request.Cookies.Keys)}");

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "No se proporcionó un refresh token válido" });
            }

            var result = await _accountService.RefreshAccessTokenAsync(refreshToken);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            Response.Cookies.Append("access_token", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(120),
                Path = "/"
            });
            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/",
            });

            return Ok(result);
        }

        [HttpGet("GetAllAccounts")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var result = await _accountService.GetAllAccounts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllAccounts)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{idPersona}")]
        public async Task<IActionResult> GetById(int idPersona)
        {
            try
            {
                var itemsDto = await _accountService.GetById(idPersona);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetById)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccountAsync(int id)
        {
            try
            {
                var result = await _accountService.DeleteAccountAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(DeleteAccountAsync)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountVM createAccountVM)
        {
            try
            {
                var result = await _accountService.CreateAccountAsync(createAccountVM);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(CreateAccount)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{IdPersona}")]
        public async Task<IActionResult> UpdateAccountAsync(int IdPersona, [FromBody] UpdateAccountVM updateAccountVM)
        {
            try
            {
                var result = await _accountService.UpdateAccountAsync(updateAccountVM, IdPersona);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(UpdateAccountAsync)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("validateToken")]
        public async Task<IActionResult> ValidateJwtToken([FromBody] string token)
        {
            try
            {
                var result = await _accountService.ValidateJwtToken(token);

                if (result.Message == "Token Invalido")
                {
                    return Unauthorized(new { message = "User not found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ValidateJwtToken)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        [HttpGet("getAllRoles")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var result = await _accountService.GetAllRoles();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ValidateJwtToken)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("currentUser")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "No se encontró el ID del usuario en los claims" });
            }

            var response = await _accountService.GetCurrentUser(userId);

            if (!response.Success)
            {
                return Ok(response);
            }

            return Ok(response);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVM forgotPasswordVM)
        {
            var response = await _accountService.FindUserByEmailAsync(forgotPasswordVM.Email);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM model)
        {
            var response = await _accountService.ResetPasswordAsync(model);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }


}
