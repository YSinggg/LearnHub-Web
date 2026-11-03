namespace LearnHub.Pages.Admin;

public class UsersModel : PageModel
{
    private static readonly string[] Roles = { "Member", "Admin" };

    private readonly IConfiguration configuration;

    public UsersModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [BindProperty(SupportsGet = true), StringLength(100)]
    public string? Q { get; set; }

    public List<UserAccount> UsersList { get; set; } = new List<UserAccount>();

    public int CurrentUserId
{
    get
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }
}

    public void OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand(
            "SELECT id, email, full_name, role FROM users WHERE LOCATE(@q, email) > 0 OR LOCATE(@q, full_name) > 0 ORDER BY email",
            connection);
        command.Parameters.AddWithValue("@q", Q?.Trim() ?? "");
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            UsersList.Add(new UserAccount
            {
                Id = reader.GetInt32("id"),
                Email = reader.GetString("email"),
                FullName = reader.GetString("full_name"),
                Role = reader.GetString("role"),
            });
        }
    }

    public IActionResult OnPostRole(int id, string role)
    {
        if (!Roles.Contains(role)) return Back("Choose Member or Admin.");
        return Change(id, role, delete: false);
    }

    public IActionResult OnPostDelete(int id, bool confirmed)
    {
        if (!confirmed) return Back("Tick the box to confirm before deleting a user.");
        return Change(id, null, delete: true);
    }

    private IActionResult Change(int id, string? role, bool delete)
    {
        if (id == CurrentUserId) return Back("You cannot change or delete your own account here.");

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var adminIds = new List<int>();
        using (var admins = new MySqlCommand("SELECT id FROM users WHERE role = 'Admin' ORDER BY id", connection, transaction))
        using (var reader = admins.ExecuteReader())
        {
            while (reader.Read()) adminIds.Add(reader.GetInt32(0));
        }

        if (!adminIds.Contains(CurrentUserId)) return Forbid();

        bool removesLastAdmin = adminIds.Contains(id) && adminIds.Count <= 1 && (delete || role == "Member");
        if (removesLastAdmin) return Back("LearnHub must keep at least one administrator.");

        string sql = delete ? "DELETE FROM users WHERE id = @id" : "UPDATE users SET role = @role WHERE id = @id";
        using var change = new MySqlCommand(sql, connection, transaction);
        change.Parameters.AddWithValue("@id", id);
        change.Parameters.AddWithValue("@role", role ?? "");
        int affected = change.ExecuteNonQuery();
        transaction.Commit();

        if (affected == 0) return Back("That user no longer exists.");
        return Back(delete ? "User and their learning records deleted." : $"Role changed to {role}.");
    }

    private RedirectToPageResult Back(string message)
    {
        TempData["Message"] = message;
        return RedirectToPage(new { Q });
    }
}
