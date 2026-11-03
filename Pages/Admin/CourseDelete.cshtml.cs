namespace LearnHub.Pages.Admin;

public class CourseDeleteModel : PageModel
{
    private readonly IConfiguration configuration;

    public CourseDeleteModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Course Course { get; set; } = new Course();

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("SELECT title FROM courses WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        var title = command.ExecuteScalar() as string;
        if (title == null) return NotFound();

        Course = new Course { Id = id, Title = title };
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM courses WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteNonQuery() == 0) return NotFound();

        TempData["Message"] = "Course and its related learning records deleted.";
        return RedirectToPage("/Admin/Index");
    }
}
