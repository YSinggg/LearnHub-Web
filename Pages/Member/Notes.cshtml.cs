namespace LearnHub.Pages.Member;

public class NotesModel : PageModel
{
    private readonly IConfiguration configuration;

    public NotesModel(IConfiguration configuration)
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
    [BindProperty(SupportsGet = true)] public int LessonId { get; set; }
    [BindProperty, Required(ErrorMessage = "Write your note."), StringLength(4000), Display(Name = "Note")] public string Body { get; set; } = "";

    public string LessonTitle { get; set; } = "";
    public bool Exists { get; set; }

    public IActionResult OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        return Load(connection, fillBody: true) ? Page() : Forbid();
    }

    public IActionResult OnPostSave()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection, fillBody: false)) return Forbid();
        if (string.IsNullOrWhiteSpace(Body)) ModelState.AddModelError(nameof(Body), "Write a note.");
        if (!ModelState.IsValid) return Page();

        using var command = new MySqlCommand(@"
            INSERT INTO lesson_notes (user_id, lesson_id, note_text) VALUES (@u, @l, @b)
            ON DUPLICATE KEY UPDATE note_text = @b, updated_utc = UTC_TIMESTAMP()
            ", connection);
        command.Parameters.AddWithValue("@u", GetUserId());
        command.Parameters.AddWithValue("@l", LessonId);
        command.Parameters.AddWithValue("@b", Body.Trim());
        command.ExecuteNonQuery();

        TempData["Message"] = "Private note saved.";
        return RedirectToPage("/Member/Study", new { id = Id });
    }

    public IActionResult OnPostDelete()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection, fillBody: false)) return Forbid();

        using var command = new MySqlCommand("DELETE FROM lesson_notes WHERE user_id = @u AND lesson_id = @l", connection);
        command.Parameters.AddWithValue("@u", GetUserId());
        command.Parameters.AddWithValue("@l", LessonId);
        command.ExecuteNonQuery();

        TempData["Message"] = "Note deleted.";
        return RedirectToPage("/Member/Study", new { id = Id });
    }

    private bool Load(MySqlConnection connection, bool fillBody)
    {
        using (var command = new MySqlCommand(@"
            SELECT l.title
            FROM lessons l JOIN enrollments e ON e.course_id = l.course_id
            WHERE l.id = @l AND l.course_id = @c AND e.user_id = @u
            ", connection))
        {
            command.Parameters.AddWithValue("@l", LessonId);
            command.Parameters.AddWithValue("@c", Id);
            command.Parameters.AddWithValue("@u", GetUserId());
            var title = command.ExecuteScalar() as string;
            if (title == null) return false;
            LessonTitle = title;
        }

        using (var command = new MySqlCommand("SELECT note_text FROM lesson_notes WHERE user_id = @u AND lesson_id = @l", connection))
        {
            command.Parameters.AddWithValue("@u", GetUserId());
            command.Parameters.AddWithValue("@l", LessonId);
            var note = command.ExecuteScalar() as string;
            Exists = note != null;
            if (fillBody) Body = note ?? "";
        }
        return true;
    }
}
