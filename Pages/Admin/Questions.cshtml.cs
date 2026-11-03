namespace LearnHub.Pages.Admin;

public class QuestionsModel : PageModel
{
    private readonly IConfiguration configuration;

    public QuestionsModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Course Course { get; set; } = new Course();

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using (var command = new MySqlCommand("SELECT title FROM courses WHERE id = @id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            var title = command.ExecuteScalar() as string;
            if (title == null) return NotFound();
            Course = new Course { Id = id, Title = title };
        }

        using (var command = new MySqlCommand("SELECT id, prompt FROM questions WHERE course_id = @id ORDER BY id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                Course.Questions.Add(new Question { Id = reader.GetInt32("id"), Prompt = reader.GetString("prompt") });
        }
        return Page();
    }

    public IActionResult OnPostDeleteQuestion(int id, int itemId, bool confirmed)
    {
        if (!confirmed) return BadRequest();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM questions WHERE id = @item AND course_id = @course", connection);
        command.Parameters.AddWithValue("@item", itemId);
        command.Parameters.AddWithValue("@course", id);
        if (command.ExecuteNonQuery() == 0) return NotFound();

        TempData["Message"] = "Question deleted.";
        return RedirectToPage(new { id });
    }
}
