using Microsoft.AspNetCore.Identity;

namespace LearnHub.Pages.Account;

public class RegisterModel : PageModel
{
    private const int DuplicateKeyError = 1062;

    private readonly IConfiguration configuration;
    private readonly IPasswordHasher<UserAccount> _hasher;

    public RegisterModel(IConfiguration configuration, IPasswordHasher<UserAccount> hasher)
    {
        this.configuration = configuration;
        _hasher = hasher;
    }

    [BindProperty] public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required, StringLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 10)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).+$", ErrorMessage = "Use uppercase, lowercase, a number and a symbol.")]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var user = new UserAccount { Email = UserAccount.NormalizeEmail(Input.Email) };

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        string sql = "INSERT INTO users (email, password_hash, role, full_name) VALUES (@email, @hash, 'Member', @name)";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@hash", _hasher.HashPassword(user, Input.Password));
        command.Parameters.AddWithValue("@name", Input.FullName.Trim());
        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.Number == DuplicateKeyError)
        {
            ModelState.AddModelError("", "This email is already registered. Please sign in.");
            return Page();
        }

        TempData["Message"] = "Account created. Sign in to start learning.";
        return RedirectToPage("/Account/Login");
    }
}
