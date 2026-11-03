namespace LearnHub.Pages.Admin;

public class LessonEditModel : PageModel
{
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;

    public LessonEditModel(IConfiguration configuration, IWebHostEnvironment environment)
    {
        this.configuration = configuration;
        this.environment = environment;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public int CourseId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = "";

        [Required, StringLength(12000)]
        public string Content { get; set; } = "";

        [Range(1, 1000)]
        public int Position { get; set; } = 1;

        [StringLength(400), Display(Name = "Video link (optional)")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Lesson diagram (optional)")]
        public string? ImagePath { get; set; }

        [StringLength(300), Display(Name = "Diagram description")]
        public string? ImageAlt { get; set; }
    }

    public List<string> LessonImages =>
    Directory.Exists(Path.Combine(environment.WebRootPath, "images"))
        ? Directory.GetFiles(Path.Combine(environment.WebRootPath, "images"))
            .Where(file => new[] { ".png", ".jpg" }.Contains(Path.GetExtension(file).ToLower()))
            .Select(file => "/images/" + Path.GetFileName(file))
            .OrderBy(file => file)
            .ToList()
        : new List<string>();

    public IActionResult OnGet(int id, int courseId)
    {
        Id = id;
        CourseId = courseId;

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!CourseExists(connection)) return NotFound();
        if (id == 0) return Page();

        using var command = new MySqlCommand(
            "SELECT title, content, position, video_url, image_path FROM lessons WHERE id = @id AND course_id = @course", connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@course", courseId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();

        Input = new InputModel
        {
            Title = reader.GetString("title"),
            Content = reader.GetString("content"),
            Position = reader.GetInt32("position"),
            VideoUrl = reader.GetString("video_url"),
            ImagePath = reader.GetString("image_path"),
        };
        return Page();
    }

    public IActionResult OnPost()
    {
        string? video = Lesson.NormalizeVideoUrl(Input.VideoUrl);
        string image = Input.ImagePath?.Trim() ?? "";
        string alt = Input.ImageAlt?.Trim() ?? "";

        if (video == null)
            ModelState.AddModelError("Input.VideoUrl", "YouTube link.");

        if (image != "" && !LessonImages.Contains(image))
            ModelState.AddModelError("Input.ImagePath", "Choose a diagram.");
        else if (image != "" && alt == "")
            ModelState.AddModelError("Input.ImageAlt", "Describe what the diagram shows.");
        if (image == "") alt = "";

        if (!ModelState.IsValid) return Page();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!CourseExists(connection)) return NotFound();

        string sql = Id == 0
            ? @"
              INSERT INTO lessons (course_id, title, content, position, video_url, image_path)
              VALUES (@course, @title, @content, @position, @video, @image)
              "
            : @"
              UPDATE lessons SET title = @title, content = @content, position = @position,
                  video_url = @video, image_path = @image
              WHERE id = @id AND course_id = @course
              ";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", Id);
        command.Parameters.AddWithValue("@course", CourseId);
        command.Parameters.AddWithValue("@title", Input.Title);
        command.Parameters.AddWithValue("@content", Input.Content);
        command.Parameters.AddWithValue("@position", Input.Position);
        command.Parameters.AddWithValue("@video", video);
        command.Parameters.AddWithValue("@image", image);
        if (command.ExecuteNonQuery() == 0 && Id != 0) return NotFound();

        TempData["Message"] = "Lesson saved.";
        return RedirectToPage("/Admin/Content", new { id = CourseId });
    }

    private bool CourseExists(MySqlConnection connection)
    {
        using var command = new MySqlCommand("SELECT COUNT(*) FROM courses WHERE id = @course", connection);
        command.Parameters.AddWithValue("@course", CourseId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
