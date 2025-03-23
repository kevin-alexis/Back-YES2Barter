using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;
using Domain.DTOs;
using Service.Services.Implementation;

namespace WebAPI.Controllers
{

    [ApiController]
    [Route("api/")]
    public class ForgotPasswordController : ControllerBase
    {

        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public ForgotPasswordController(ITokenService tokenService, IEmailService emailService, IUserService userService)
        {
            _tokenService = tokenService;
            _emailService = emailService;
            _userService = userService;
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var token = _tokenService.GenerateResetToken(request.Email);
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";

            await _emailService.SendResetPasswordEmail(request.Email, resetLink);

            return Ok(new { message = "Password reset email sent!" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var email = _tokenService.ValidateResetToken(request.Token);
            if (email == null)
                return BadRequest(new { message = "Invalid or expired token" });

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            // Uso de PasswordHasher de forma estática
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            await _userService.UpdateAsync(user);

            return Ok(new { message = "Password successfully reset!" });
        }
    }
}
