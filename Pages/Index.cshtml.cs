namespace LearnHub.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true), StringLength(100)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public List<Course> Courses { get; set; } = new List<Course>();
    public List<string> Categories { get; set; } = new List<string>();

    public void OnGet()
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();

        string sql = @"
            SELECT c.id, c.title, c.category, c.description, c.level, c.image_path,
                   (SELECT COUNT(*) FROM lessons l WHERE l.course_id = c.id) AS lesson_count
            FROM courses c
            ORDER BY c.title
            ";
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var all = new List<Course>();
        while (reader.Read())
        {
            all.Add(new Course
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Category = reader.GetString("category"),
                Description = reader.GetString("description"),
                Level = reader.GetString("level"),
                ImagePath = reader.GetString("image_path"),
                LessonCount = reader.GetInt32("lesson_count"),
            });
        }

        string search = Q?.Trim() ?? "";

        Categories = new List<string>();
        foreach (var c in all)
        {
            if (!Categories.Contains(c.Category))
            {
                Categories.Add(c.Category);
            }
        }
        Categories.Sort();

        Courses = new List<Course>();
        foreach (var c in all)
        {
            bool matchSearch = string.IsNullOrEmpty(search) 
                || c.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 
                || c.Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            bool matchCategory = string.IsNullOrEmpty(Category) || c.Category == Category;

            if (matchSearch && matchCategory)
            {
                Courses.Add(c);
            }
        }
    }
}

