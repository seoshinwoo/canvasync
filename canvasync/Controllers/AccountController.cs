using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using canvasync.Library.Models;
using canvasync.Data;
using Microsoft.EntityFrameworkCore;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly CanvasDbContext _context;

    public AccountController(CanvasDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]    
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
    {
        var user = await _context.Members.FirstOrDefaultAsync(m => m.Name == username && m.Password == password);
        
        if (user != null) 
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Redirect("/"); 
        }

        return Redirect("/login?error=InvalidCredentials");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] string username, [FromForm] string password)
    {
        if (await _context.Members.AnyAsync(m => m.Name == username))
        {
             return Redirect("/login?error=UsernameExists");
        }

        var newMember = new Member
        {
            Name = username,
            Password = password,
        };
        
        _context.Members.Add(newMember);
        await _context.SaveChangesAsync();
        
        return await Login(username, password);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}