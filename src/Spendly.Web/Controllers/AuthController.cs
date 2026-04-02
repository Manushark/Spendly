using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.Auth;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApi;

        public AuthController(AuthApiClient authApi)
        {
            _authApi = authApi;
        }

        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _authApi.LoginAsync(model);

            if (result == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            HttpContext.Session.SetString("token", result.Token);
            HttpContext.Session.SetString("userEmail", model.Email);

            return RedirectToAction("Index", "Expenses");
        }

        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(model);
            }

            var result = await _authApi.RegisterAsync(model);

            if (result == null)
            {
                ViewBag.Error = "Registration failed. Email may already be in use.";
                return View(model);
            }

            HttpContext.Session.SetString("token", result.Token);
            HttpContext.Session.SetString("userEmail", model.Email);

            return RedirectToAction("Index", "Expenses");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
