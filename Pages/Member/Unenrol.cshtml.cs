namespace LearnHub.Pages.Member;

public class UnenrolModel : PageModel
{
    private readonly IConfiguration configuration;

    public UnenrolModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

  private int GetUserId()
{
    var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (claim == null) return 0;
    return int.Parse(claim);
}

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public bool Confirm { get; set; }
    public string Title { get; set; } = "";

    public IActionResult OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand(@"
            SELECT c.title
            FROM enrollments e JOIN courses c ON c.id = e.course_id
            WHERE e.user_id = @u AND e.course_id = @c
            ", connection);
        command.Parameters.AddWithValue("@u", GetUserId());
        command.Parameters.AddWithValue("@c", Id);
        var title = command.ExecuteScalar() as string;
        if (title == null) return NotFound();

        Title = title;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!Confirm)
        {
            TempData["Message"] = "Tick the box to confirm that you want to leave this course.";
            return RedirectToPage(new { id = Id });
        }

        var userId = GetUserId();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using var command = new MySqlCommand("DELETE FROM enrollments WHERE user_id = @u AND course_id = @c", connection);
        command.Parameters.AddWithValue("@u", userId);
        command.Parameters.AddWithValue("@c", Id);
        command.ExecuteNonQuery();

        TempData["Message"] = "You left the course.";
        return RedirectToPage("/Member/Dashboard");
    }
}
