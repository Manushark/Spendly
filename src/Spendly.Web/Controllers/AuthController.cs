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

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
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
            catch (Exception)
            {
                ViewBag.Error = "Could not connect to the server. Please make sure the API is running.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(model);
            }

            try
            {
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
            catch (Exception)
            {
                ViewBag.Error = "Could not connect to the server. Please make sure the API is running.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
