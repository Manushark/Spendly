using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.Auth;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApi;
        private readonly IConfiguration _config;

        public AuthController(AuthApiClient authApi, IConfiguration config)
        {
            _authApi = authApi;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
                return RedirectToAction("Index", "Dashboard");

            var apiBase = _config["ApiBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7267";
            ViewBag.GoogleLoginUrl = $"{apiBase}/api/auth/google/login";

            // Prevent caching of login page
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var result = await _authApi.LoginAsync(model);

                if (result == null)
                {
                    ViewBag.Error = "Email o contraseña inválidos.";
                    var apiBase = _config["ApiBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7267";
                    ViewBag.GoogleLoginUrl = $"{apiBase}/api/auth/google/login";
                    return View(model);
                }

                HttpContext.Session.SetString("token", result.Token);
                HttpContext.Session.SetString("userEmail", model.Email);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar al servidor. Asegúrate de que el API esté corriendo.";
                var apiBase = _config["ApiBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7267";
                ViewBag.GoogleLoginUrl = $"{apiBase}/api/auth/google/login";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
                return RedirectToAction("Index", "Dashboard");

            // Prevent caching
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";

            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View(model);
            }

            try
            {
                var result = await _authApi.RegisterAsync(model);

                if (result == null)
                {
                    ViewBag.Error = "Registro fallido. El email puede estar en uso.";
                    return View(model);
                }

                HttpContext.Session.SetString("token", result.Token);
                HttpContext.Session.SetString("userEmail", model.Email);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar al servidor. Asegúrate de que el API esté corriendo.";
                return View(model);
            }
        }

        /// <summary>
        /// Receives the JWT token from Google OAuth callback redirect
        /// </summary>
        [HttpGet]
        public IActionResult GoogleCallback(string token, string email)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Google authentication failed. Please try again.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("token", token);
            HttpContext.Session.SetString("userEmail", email ?? "Google User");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
