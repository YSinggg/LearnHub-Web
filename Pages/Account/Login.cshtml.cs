using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace LearnHub.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IConfiguration configuration;
    private readonly IPasswordHasher<UserAccount> hasher;

    public LoginModel(IConfiguration config, IPasswordHasher<UserAccount> hasher)
    {
        this.configuration = config;
        this.hasher = hasher;
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

        const int MaxAttempts = 5;
        const int LockMinutes = 15;

        using var conn = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        await conn.OpenAsync();

        UserAccount? user = null;
        int failedAttempts = 0;
        bool isLocked = false;

        using (var cmd = new MySqlCommand(@"
            SELECT id, email, password_hash, role, failed_attempts,
                   (lockout_until IS NOT NULL AND lockout_until > UTC_TIMESTAMP()) AS is_locked
            FROM users WHERE email = @email", conn))
        {
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
                failedAttempts = reader.GetInt32("failed_attempts");
                isLocked = reader.GetInt32("is_locked") == 1;
            }
        }

        if (isLocked)
        {
            ModelState.AddModelError("", $"Too many failed attempts. Try again in {LockMinutes} minutes.");
            return Page();
        }

        bool passwordOk = user != null &&
            hasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password) != PasswordVerificationResult.Failed;

        if (!passwordOk)
        {
            if (user != null)
            {
                failedAttempts++;
                string sql = failedAttempts >= MaxAttempts
                    ? "UPDATE users SET failed_attempts = 0, lockout_until = UTC_TIMESTAMP() + INTERVAL @mins MINUTE WHERE id = @id"
                    : "UPDATE users SET failed_attempts = @n WHERE id = @id";
                using var update = new MySqlCommand(sql, conn);
                update.Parameters.AddWithValue("@n", failedAttempts);
                update.Parameters.AddWithValue("@mins", LockMinutes);
                update.Parameters.AddWithValue("@id", user.Id);
                await update.ExecuteNonQueryAsync();
            }
            ModelState.AddModelError("", "Invalid email or password.");
            return Page();
        }

        using (var reset = new MySqlCommand("UPDATE users SET failed_attempts = 0, lockout_until = NULL WHERE id = @id", conn))
        {
            reset.Parameters.AddWithValue("@id", user!.Id);
            await reset.ExecuteNonQueryAsync();
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