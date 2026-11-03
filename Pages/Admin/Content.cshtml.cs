namespace LearnHub.Pages.Admin;

public class ContentModel : PageModel
{
    private readonly IConfiguration configuration;

    public ContentModel(IConfiguration configuration)
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

        using (var command = new MySqlCommand("SELECT id, title, position FROM lessons WHERE course_id = @id ORDER BY position, id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Course.Lessons.Add(new Lesson
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Position = reader.GetInt32("position"),
                });
            }
        }
        return Page();
    }

    public IActionResult OnPostDeleteLesson(int id, int itemId, bool confirmed)
    {
        if (!confirmed) return BadRequest();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM lessons WHERE id = @item AND course_id = @course", connection);
        command.Parameters.AddWithValue("@item", itemId);
        command.Parameters.AddWithValue("@course", id);
        if (command.ExecuteNonQuery() == 0) return NotFound();

        TempData["Message"] = "Lesson deleted.";
        return RedirectToPage(new { id });
    }
}
