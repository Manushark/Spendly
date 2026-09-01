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
        private readonly GetNotificationPreferencesUseCase _getNotifPrefs;
        private readonly UpdateNotificationPreferencesUseCase _updateNotifPrefs;

        public UserController(
            GetUserProfileUseCase getProfile,
            UpdateUserProfileUseCase updateProfile,
            ChangePasswordUseCase changePassword,
            GetNotificationPreferencesUseCase getNotifPrefs,
            UpdateNotificationPreferencesUseCase updateNotifPrefs)
        {
            _getProfile = getProfile;
            _updateProfile = updateProfile;
            _changePassword = changePassword;
            _getNotifPrefs = getNotifPrefs;
            _updateNotifPrefs = updateNotifPrefs;
        }

        /// <summary>GET /api/user/profile</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _getProfile.ExecuteAsync(User.GetUserId());
            return Ok(profile);
        }

        /// <summary>PUT /api/user/profile</summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            await _updateProfile.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>PUT /api/user/change-password</summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _changePassword.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>GET /api/user/notification-preferences</summary>
        [HttpGet("notification-preferences")]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            var prefs = await _getNotifPrefs.ExecuteAsync(User.GetUserId());
            return Ok(prefs);
        }

        /// <summary>PUT /api/user/notification-preferences</summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("notification-preferences")]
        public async Task<IActionResult> UpdateNotificationPreferences(
            [FromBody] UpdateNotificationPreferencesDto dto)
        {
            await _updateNotifPrefs.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Notification preferences updated." });
        }
    }
}
