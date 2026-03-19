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
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Redirect("/login?error=InvalidCredentials");

        try 
        {
            // 사용자 이름으로만 조회 (비밀번호는 해시이므로 DB에서 직접 비교 불가)
            var user = await _context.Members.FirstOrDefaultAsync(m => m.Name == username);

            // 유저가 없는 경우 즉시 에러 파라미터와 함께 로그인 창으로 리다이렉트
            if (user == null)
                return Redirect("/login?error=InvalidCredentials");

            // 비밀번호 검증 중 예외(해시 형식이 아닌 평문 저장 등)가 발생할 수 있으므로 분리해서 처리
            bool isPasswordValid = false;
            try 
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch 
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
                return Redirect("/login?error=InvalidCredentials");

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
        catch (Exception)
        {
            // DB 연결 등 기타 예상치 못한 에러 발생 시에도 빈 화면 예외를 띄우지 않고 로그인 창에 안내
            return Redirect("/login?error=InvalidCredentials");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] string username, [FromForm] string password)
    {
        // 입력값 유효성 검사: 빈 값, 길이 제한
        if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
            return Redirect("/login?error=InvalidUsername");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 2 || password.Length > 100)
            return Redirect("/login?error=InvalidPassword");

        if (await _context.Members.AnyAsync(m => m.Name == username))
            return Redirect("/login?error=UsernameExists");

        // BCrypt.HashPassword: 평문 비밀번호 → 솔트 포함 해시 (복호화 불가능)
        var newMember = new Member
        {
            Name = username,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
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