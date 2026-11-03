namespace LearnHub.Pages.Admin;

public class QuestionEditModel : PageModel
{
    private readonly IConfiguration configuration;

    public QuestionEditModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public int CourseId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required, StringLength(400)] public string Prompt { get; set; } = "";
        [Required, StringLength(200), Display(Name = "Option A")] public string OptionA { get; set; } = "";
        [Required, StringLength(200), Display(Name = "Option B")] public string OptionB { get; set; } = "";
        [Required, StringLength(200), Display(Name = "Option C")] public string OptionC { get; set; } = "";
        [Range(0, 2)] public int CorrectOption { get; set; }
        [Required, StringLength(800)] public string Explanation { get; set; } = "";
    }

    public IActionResult OnGet(int id, int courseId)
    {
        Id = id;
        CourseId = courseId;

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!CourseExists(connection)) return NotFound();
        if (id == 0) return Page();

        using var command = new MySqlCommand("""
            SELECT prompt, option_a, option_b, option_c, correct_option, explanation
            FROM questions WHERE id = @id AND course_id = @course
            """, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@course", courseId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();

        Input = new InputModel
        {
            Prompt = reader.GetString("prompt"),
            OptionA = reader.GetString("option_a"),
            OptionB = reader.GetString("option_b"),
            OptionC = reader.GetString("option_c"),
            CorrectOption = reader.GetInt32("correct_option"),
            Explanation = reader.GetString("explanation"),
        };
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!CourseExists(connection)) return NotFound();

        string sql = Id == 0
            ? @"
              INSERT INTO questions (course_id, prompt, option_a, option_b, option_c, correct_option, explanation)
              VALUES (@course, @prompt, @option_a, @option_b, @option_c, @correct_option, @explanation)
              "
            : @"
              UPDATE questions
              SET prompt = @prompt, option_a = @option_a, option_b = @option_b, option_c = @option_c,
                  correct_option = @correct_option, explanation = @explanation
              WHERE id = @id AND course_id = @course
              ";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", Id);
        command.Parameters.AddWithValue("@course", CourseId);
        command.Parameters.AddWithValue("@prompt", Input.Prompt);
        command.Parameters.AddWithValue("@option_a", Input.OptionA);
        command.Parameters.AddWithValue("@option_b", Input.OptionB);
        command.Parameters.AddWithValue("@option_c", Input.OptionC);
        command.Parameters.AddWithValue("@correct_option", Input.CorrectOption);
        command.Parameters.AddWithValue("@explanation", Input.Explanation);
        if (command.ExecuteNonQuery() == 0 && Id != 0) return NotFound();

        TempData["Message"] = "Question saved.";
        return RedirectToPage("/Admin/Questions", new { id = CourseId });
    }

    private bool CourseExists(MySqlConnection connection)
    {
        using var command = new MySqlCommand("SELECT COUNT(*) FROM courses WHERE id = @course", connection);
        command.Parameters.AddWithValue("@course", CourseId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
