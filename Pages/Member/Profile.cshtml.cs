using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace LearnHub.Pages.Member;

public class ProfileModel : PageModel
{
    private readonly IConfiguration configuration;
    private readonly IPasswordHasher<UserAccount> hasher;

    public ProfileModel(IConfiguration configuration, IPasswordHasher<UserAccount> hasher)
    {
        this.configuration = configuration;
        this.hasher = hasher;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }

    public string Email { get; set; } = "";

    [BindProperty, Required, StringLength(100), Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Enter your current password."), DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Enter a new password."), DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Password must be at least 10 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).+$", ErrorMessage = "Password must contain uppercase, lowercase, a number and a symbol.")]
    public string NewPassword { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Confirm the new password."), DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords must match.")]
    public string ConfirmPassword { get; set; } = "";
    [BindProperty] public bool ConfirmDelete { get; set; }
    [BindProperty, Required(ErrorMessage = "Enter your current password."), DataType(DataType.Password)] public string DeletePassword { get; set; } = "";

    public void OnGet() => Load(fillFullName: true);

    public IActionResult OnPostSave()
    {
        ModelState.Remove(nameof(CurrentPassword));
        ModelState.Remove(nameof(DeletePassword));
        ModelState.Remove(nameof(NewPassword));
        ModelState.Remove(nameof(ConfirmPassword));
        if (!ModelState.IsValid)
        {
            Load(fillFullName: false);
            return Page();
        }

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("UPDATE users SET full_name = @name WHERE id = @u", connection);
        command.Parameters.AddWithValue("@name", FullName.Trim());
        command.Parameters.AddWithValue("@u", GetUserId());
        command.ExecuteNonQuery();
        TempData["Message"] = "Profile updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPasswordAsync()
    {
        ModelState.Clear();
        var user = LoadUser();
        if (user == null) return Forbid();

        if (!PasswordMatches(user, CurrentPassword))
            ModelState.AddModelError(nameof(CurrentPassword), "Wrong Password");
        NewPassword ??= "";
        ConfirmPassword ??= "";
        bool strong = NewPassword.Length >= 10 && NewPassword.Length <= 100
            && Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).+$");
        if (!strong)
            ModelState.AddModelError(nameof(NewPassword), "Password must be at least 10 characters.");
        if (NewPassword != ConfirmPassword)
            ModelState.AddModelError(nameof(ConfirmPassword), "Passwords do not match.");
        if (!ModelState.IsValid)
        {
            Load(fillFullName: true);
            return Page();
        }

        using (var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb")))
        {
            connection.Open();
            using var command = new MySqlCommand("UPDATE users SET password_hash = @hash WHERE id = @u", connection);
            command.Parameters.AddWithValue("@hash", hasher.HashPassword(user, NewPassword));
            command.Parameters.AddWithValue("@u", GetUserId());
            command.ExecuteNonQuery();
        }
        await HttpContext.SignOutAsync();
        TempData["Message"] = "Password changed. Sign in again.";
        return RedirectToPage("/Account/Login");
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        ModelState.Clear();
        var user = LoadUser();
        if (user == null) return Forbid();

        if (user.Role == "Admin")
            ModelState.AddModelError("", "Administrators cannot delete their own account.");
        else
        {
            if (!PasswordMatches(user, DeletePassword))
                ModelState.AddModelError(nameof(DeletePassword), "Wrong Password");
            if (!ConfirmDelete)
                ModelState.AddModelError(nameof(ConfirmDelete), "Check the box to confirm");
        }
        if (!ModelState.IsValid)
        {
            Load(fillFullName: true);
            return Page();
        }

        using (var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb")))
        {
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM users WHERE id = @u AND role = 'Member'", connection);
            command.Parameters.AddWithValue("@u", GetUserId());
            command.ExecuteNonQuery();
        }
        await HttpContext.SignOutAsync();
        TempData["Message"] = "Your account and learning records were deleted.";
        return RedirectToPage("/Index");
    }

    private bool PasswordMatches(UserAccount user, string? password) =>
        !string.IsNullOrEmpty(password)
        && hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;

    private UserAccount? LoadUser()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("SELECT email, full_name, role, password_hash FROM users WHERE id = @u", connection);
        command.Parameters.AddWithValue("@u", GetUserId());
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new UserAccount
        {
            Id = GetUserId(),
            Email = reader.GetString("email"),
            FullName = reader.GetString("full_name"),
            Role = reader.GetString("role"),
            PasswordHash = reader.GetString("password_hash"),
        };
    }

    private void Load(bool fillFullName)
    {
        var user = LoadUser()!;
        Email = user.Email;
        if (fillFullName) FullName = user.FullName;
    }
}
