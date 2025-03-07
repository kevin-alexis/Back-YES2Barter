using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreateAccountVM;
using Domain.ViewModels.Login;
using Domain.ViewModels.UpdateAccountVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            var result = await _accountService.LoginAsync(loginVM);
            if (result.Success == true)
            {
                return Ok(result);
            }

            return Unauthorized(result);
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
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("validateToken")]
        public async Task<IActionResult> ValidateJwtToken([FromBody] string token)
        {
            var result = await _accountService.ValidateJwtToken(token);

            if (result.Message == "Token Invalido")
            {
                return Unauthorized(new { message = "User not found." });
            }

            return Ok(result);
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
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
