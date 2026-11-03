namespace LearnHub.Pages.Courses;

public class PreviewModel : PageModel
{
    private readonly IConfiguration configuration;

    public PreviewModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Course Course { get; set; } = new Course();
    public Lesson? Lesson { get; set; }

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using (var command = new MySqlCommand("SELECT title, category, image_path FROM courses WHERE id = @id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return NotFound();

            Course = new Course
            {
                Id = id,
                Title = reader.GetString("title"),
                Category = reader.GetString("category"),
                ImagePath = reader.GetString("image_path"),
            };
        }

        using (var command = new MySqlCommand(
            "SELECT title, content, video_url, image_path FROM lessons WHERE course_id = @id ORDER BY position, id LIMIT 1", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
                Lesson = new Lesson
                {
                    Title = reader.GetString("title"),
                    Content = reader.GetString("content"),
                    VideoUrl = reader.GetString("video_url"),
                    ImagePath = reader.GetString("image_path"),
                };
        }
        return Page();
    }
}
