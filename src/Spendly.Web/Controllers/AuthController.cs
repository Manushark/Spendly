using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.Auth;
using Spendly.Web.Services;
using Spendly.Web.Helpers;

namespace Spendly.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApi;
        private readonly string _apiBaseUrl;

        public AuthController(AuthApiClient authApi, IConfiguration configuration)
        {
            _authApi = authApi;
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

            return RedirectToAction("Index", "Expenses");
        }

        public IActionResult Logout()
        {
            TokenHelper.ClearToken(HttpContext);
            return RedirectToAction("Login");
        }
    }
}

