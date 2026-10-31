using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace LearnHub.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<UserAccount> _hasher;

    public LoginModel(IConfiguration config, IPasswordHasher<UserAccount> hasher)
    {
        _config = config;
        _hasher = hasher;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        UserAccount? user = null;

        using (var conn = new MySqlConnection(_config.GetConnectionString("LearningDb")))
        {
            await conn.OpenAsync();
            var sql = "SELECT id, email, password_hash, role FROM users WHERE email = @email";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", Input.Email.Trim().ToLower());

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                user = new UserAccount
                {
                    Id = reader.GetInt32("id"),
                    Email = reader.GetString("email"),
                    PasswordHash = reader.GetString("password_hash"),
                    Role = reader.GetString("role")
                };
            }
        }

        if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Member/Dashboard");
    }
}