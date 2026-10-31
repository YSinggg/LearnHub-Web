namespace LearnHub.Pages.Admin;

public class CourseEditModel : PageModel
{
    private readonly IConfiguration _configuration;

    public CourseEditModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [Required, StringLength(40)]
        public string Category { get; set; } = "";

        [Required, StringLength(1000)]
        public string Description { get; set; } = "";

        [Required, StringLength(30)]
        public string Level { get; set; } = "Beginner";

        [Required, RegularExpression(@"^/images/[A-Za-z0-9_-]+\.(png|jpg)$"), Display(Name = "Course image")]
        public string ImagePath { get; set; } = "";
    }

    public IActionResult OnGet(int id)
    {
        Id = id;
        if (id == 0) return Page();

        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand(
            "SELECT title, category, description, level, image_path FROM courses WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();

        Input = new InputModel
        {
            Title = reader.GetString("title"),
            Category = reader.GetString("category"),
            Description = reader.GetString("description"),
            Level = reader.GetString("level"),
            ImagePath = reader.GetString("image_path"),
        };
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();

        string sql = Id == 0
            ? "INSERT INTO courses (title, category, description, level, image_path) VALUES (@title, @category, @description, @level, @image)"
            : "UPDATE courses SET title = @title, category = @category, description = @description, level = @level, image_path = @image WHERE id = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", Id);
        command.Parameters.AddWithValue("@title", Input.Title.Trim());
        command.Parameters.AddWithValue("@category", Input.Category.Trim());
        command.Parameters.AddWithValue("@description", Input.Description.Trim());
        command.Parameters.AddWithValue("@level", Input.Level.Trim());
        command.Parameters.AddWithValue("@image", Input.ImagePath);

        if (command.ExecuteNonQuery() == 0 && Id != 0) return NotFound();

        TempData["Message"] = "Course saved.";
        return RedirectToPage("/Admin/Index");
    }
}
