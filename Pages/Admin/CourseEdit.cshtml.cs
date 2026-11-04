namespace LearnHub.Pages.Admin;

public class CourseEditModel : PageModel
{
    private readonly IConfiguration configuration;

    public CourseEditModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new InputModel();
    public List<string> ExistingCategories { get; set; } = new List<string>();

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

        [Required, Display(Name = "Course image")]
        public string ImagePath { get; set; } = "";
    }

    private void LoadCategories(MySqlConnection connection)
    {
        ExistingCategories.Clear();
        using var cmd = new MySqlCommand("SELECT DISTINCT category FROM courses WHERE category != '' ORDER BY category", connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ExistingCategories.Add(reader.GetString("category"));
        }
    }

    public IActionResult OnGet(int id, string? category = null)
    {
        Id = id;

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        LoadCategories(connection);

        if (id == 0)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                Input.Category = category.Trim();
            }
            return Page();
        }

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
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        LoadCategories(connection);

        if (!ModelState.IsValid) return Page();

        using (var check = new MySqlCommand(
            "SELECT COUNT(*) FROM courses WHERE title = @title AND id != @id", connection))
        {
            check.Parameters.AddWithValue("@title", Input.Title.Trim());
            check.Parameters.AddWithValue("@id", Id);
            int sameName = Convert.ToInt32(check.ExecuteScalar());

            if (sameName > 0)
            {
                ModelState.AddModelError("Input.Title", "A course with this title already exists.");
                return Page();
            }
        }

        string sql;
        if (Id == 0)
        {
            sql = "INSERT INTO courses (title, category, description, level, image_path) " +
                  "VALUES (@title, @category, @description, @level, @image)";
        }
        else
        {
            sql = "UPDATE courses SET title = @title, category = @category, description = @description, " +
                  "level = @level, image_path = @image WHERE id = @id";
        }

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", Id);
        command.Parameters.AddWithValue("@title", Input.Title.Trim());
        command.Parameters.AddWithValue("@category", Input.Category.Trim());
        command.Parameters.AddWithValue("@description", Input.Description.Trim());
        command.Parameters.AddWithValue("@level", Input.Level.Trim());
        command.Parameters.AddWithValue("@image", Input.ImagePath);

        int rowsChanged = command.ExecuteNonQuery();

        if (Id != 0 && rowsChanged == 0)
        {
            return NotFound();
        }

        TempData["Message"] = "Course saved.";
        return RedirectToPage("/Admin/Index");
    }
}
