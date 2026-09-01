using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.Auth;
using Spendly.Web.Services;
using Spendly.Web.Helpers;

namespace Spendly.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApi;
        private readonly UserApiClient _userApi;
        private readonly string _apiBaseUrl;

        public AuthController(AuthApiClient authApi, UserApiClient userApi, IConfiguration configuration)
        {
            _authApi = authApi;
            _userApi = userApi;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "";
        }

        public IActionResult Login()
        {
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var (result, errorMessage) = await _authApi.LoginAsync(model);

            if (result == null)
            {
                ViewBag.Error = errorMessage ?? "Invalid email or password.";
                return View(model);
            }

            TokenHelper.SetToken(HttpContext, result.Token);
            HttpContext.Session.SetString("userEmail", model.Email);
            // Load and cache the user's timezone so Web controllers can use local dates
            var profile = await _userApi.GetProfileAsync();
            if (profile != null)
                HttpContext.Session.SetString("userTimeZone", profile.TimeZone);

            return RedirectToAction("Index", "Expenses");
        }

        public IActionResult Register()
        {
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(model);
            }

            var (result, errorMessage) = await _authApi.RegisterAsync(model);

            if (result == null)
            {
                ViewBag.Error = errorMessage ?? "Registration failed. Email may already be in use.";
                return View(model);
            }

            TokenHelper.SetToken(HttpContext, result.Token);
            HttpContext.Session.SetString("userEmail", model.Email);
            // Load and cache the user's timezone so Web controllers can use local dates
            var profile = await _userApi.GetProfileAsync();
            if (profile != null)
                HttpContext.Session.SetString("userTimeZone", profile.TimeZone);

            return RedirectToAction("Index", "Expenses");
        }

        public IActionResult Logout()
        {
            TokenHelper.ClearToken(HttpContext);
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            await _authApi.ForgotPasswordAsync(email);

            // Siempre mostramos el mismo mensaje — no revelamos si el email existe
            ViewBag.Success = "If that email is registered, you will receive a reset link shortly. Check your inbox (and spam folder).";
            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login");

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            var (success, error) = await _authApi.ResetPasswordAsync(token, newPassword, confirmPassword);

            if (!success)
            {
                ViewBag.Token = token;
                ViewBag.Error = error ?? "Could not reset password. Please try again.";
                return View();
            }

            TempData["Success"] = "Password reset successfully. You can now log in with your new password.";
            return RedirectToAction("Login");
        }
    }
}

