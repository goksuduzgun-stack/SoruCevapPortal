using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortal.ViewModels;

namespace SoruCevapPortal.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Admin");

        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var configuredUsername = _configuration["Admin:Username"];
        var configuredPassword = _configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrEmpty(configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "Yönetici hesabı yapılandırılmamış. Kurulum yönergesini izleyin.");
            return View(model);
        }

        var providedPassword = Encoding.UTF8.GetBytes(model.Password);
        var expectedPassword = Encoding.UTF8.GetBytes(configuredPassword);
        var validPassword = providedPassword.Length == expectedPassword.Length &&
            CryptographicOperations.FixedTimeEquals(providedPassword, expectedPassword);

        if (!string.Equals(model.Username, configuredUsername, StringComparison.Ordinal) || !validPassword)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, configuredUsername),
            new Claim(ClaimTypes.Name, configuredUsername)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Admin");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
