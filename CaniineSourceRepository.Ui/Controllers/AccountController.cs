using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace CanineSourceRepository.Ui.Controllers;


public class AccountController : Controller
{
  [HttpGet]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Login", "Account");
  }

  [HttpGet]
  public IActionResult Login()
  {
    return View();
  }
}
