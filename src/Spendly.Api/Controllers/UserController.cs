using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.DTOs.User;
using Spendly.Application.UseCases.User;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly GetUserProfileUseCase _getProfile;
        private readonly UpdateUserProfileUseCase _updateProfile;
        private readonly ChangePasswordUseCase _changePassword;

        public UserController(
            GetUserProfileUseCase getProfile,
            UpdateUserProfileUseCase updateProfile,
            ChangePasswordUseCase changePassword)
        {
            _getProfile = getProfile;
            _updateProfile = updateProfile;
            _changePassword = changePassword;
        }

        /// <summary>
        /// GET /api/user/profile
        /// Returns the authenticated user's profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _getProfile.ExecuteAsync(User.GetUserId());
            return Ok(profile);
        }

        /// <summary>
        /// PUT /api/user/profile
        /// Updates the authenticated user's profile
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            await _updateProfile.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>
        /// PUT /api/user/change-password
        /// Changes the authenticated user's password
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _changePassword.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Password changed successfully" });
        }
    }
}
