using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CanineSourceRepository.Ui.Controllers;


public record ApplicationUser(string Id, string Email, string Initials, string[] AccessScopes );




public class SecurityController : Controller
{
  //VerifyCodeBlock,GetSnippetsForCodeBlock

  private static string GenerateToken(string email)
  {
    var token = Convert.ToBase64String(Guid.CreateVersion7().ToByteArray());
    // Store token and email in a database or memory cache for validation later
    // Associate token with email and set expiration time
    //TODO :: SaveTokenToDatabase(email, token);
    return token;
  }

  private void SendMagicLinkLogic(string email)
  {
    var token = GenerateToken(email);
    var callbackUrl = Url.Action("ValidateToken", "Security", new { token }, protocol: Request.Scheme);

    var smtpClient = new SmtpClient("smtp.example.com")//mail server (config file...)
    {
      Port = 587,//mail server (config file...)
      Credentials = new NetworkCredential("your-email@example.com", "your-password"),//mail server (config file...)
      EnableSsl = true,//mail server (config file...)
    };

    var mailMessage = new MailMessage
    {
      From = new MailAddress("your-email@example.com"),//mail server (config file...)
      Subject = "Login to Your Account",//mail server (config file...)
      Body = $"Click on this link to log in: {callbackUrl}",//mail server (config file...)
      IsBodyHtml = true,
    };
    mailMessage.To.Add(email);
    smtpClient.Send(mailMessage);
  }

  private async Task SignInUser(ApplicationUser user)
  {
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, user.Email),
        new(ClaimTypes.NameIdentifier, user.Id),
   };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
  }



  [HttpPost]
  public IActionResult SendMagicLink(string email)
  {
    // Validate that the email exists in the system
    var user = "";//TODO FindUserByEmail(email); // Implement your user lookup logic
    if (user != null)
    {
      SendMagicLinkLogic(email);
    }

    return Ok("Magic link has been sent if the email is registered.");
  }

  [HttpGet]
  public async Task<IActionResult> ValidateToken(string token)
  {
    // Lookup token in the database and check if it is still valid
    var email = ""; //TODO GetEmailByToken(token); // Implement logic to find email by token
    if (email != null && !false) //TODO !IsTokenExpired(token))
    {
      // Log in the user (e.g., using cookies or session)
      await SignInUser(new ApplicationUser( Id : "", Email: "", AccessScopes: ["test"], Initials: "" ));//TODO FindUserByEmail(email);

      return RedirectToAction("Index", "Home");
    }

    return Unauthorized();
  }
}
