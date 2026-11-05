namespace LearnHub.Pages.Member;

public class StudyModel : PageModel
{
    private readonly IConfiguration _configuration;

    public StudyModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }

    public Course Course { get; set; } = new Course();
    public HashSet<int> Completed { get; set; } = new HashSet<int>();
    public Dictionary<int, string> LessonNotes { get; set; } = new Dictionary<int, string>();

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!IsEnrolled(connection, id)) return RedirectToPage("/Courses/Details", new { id });

        using (var command = new MySqlCommand("SELECT title, category FROM courses WHERE id = @id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return NotFound();

            Course = new Course
            {
                Id = id,
                Title = reader.GetString("title"),
                Category = reader.GetString("category"),
            };
        }

        using (var command = new MySqlCommand(
            "SELECT id, title, content, position, video_url, image_path FROM lessons WHERE course_id = @id ORDER BY position, id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Course.Lessons.Add(new Lesson
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Content = reader.GetString("content"),
                    Position = reader.GetInt32("position"),
                    VideoUrl = reader.GetString("video_url"),
                    ImagePath = reader.GetString("image_path"),
                });
            }
        }

        using (var command = new MySqlCommand(@"
            SELECT p.lesson_id
            FROM lesson_progress p JOIN lessons l ON l.id = p.lesson_id
            WHERE p.user_id = @user AND l.course_id = @course
            ", connection))
        {
            command.Parameters.AddWithValue("@user", GetUserId());
            command.Parameters.AddWithValue("@course", id);
            using var reader = command.ExecuteReader();
            while (reader.Read()) Completed.Add(reader.GetInt32("lesson_id"));
        }

        // Load private notes for lessons in this course
        using (var noteCmd = new MySqlCommand(@"
            SELECT n.lesson_id, n.note_text
            FROM lesson_notes n JOIN lessons l ON l.id = n.lesson_id
            WHERE n.user_id = @user AND l.course_id = @course
            ", connection))
        {
            noteCmd.Parameters.AddWithValue("@user", GetUserId());
            noteCmd.Parameters.AddWithValue("@course", id);
            using var reader = noteCmd.ExecuteReader();
            while (reader.Read())
            {
                LessonNotes[reader.GetInt32("lesson_id")] = reader.GetString("note_text");
            }
        }

        return Page();
    }

    public IActionResult OnPost(int id, int lessonId, bool complete)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!IsEnrolled(connection, id)) return Forbid();

        using (var check = new MySqlCommand("SELECT COUNT(*) FROM lessons WHERE id = @lesson AND course_id = @course", connection))
        {
            check.Parameters.AddWithValue("@lesson", lessonId);
            check.Parameters.AddWithValue("@course", id);
            if (Convert.ToInt32(check.ExecuteScalar()) == 0) return NotFound();
        }

        string sql = complete
            ? "INSERT INTO lesson_progress (user_id, lesson_id) VALUES (@user, @lesson) ON DUPLICATE KEY UPDATE completed_utc = completed_utc"
            : "DELETE FROM lesson_progress WHERE user_id = @user AND lesson_id = @lesson";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user", GetUserId());
        command.Parameters.AddWithValue("@lesson", lessonId);
        command.ExecuteNonQuery();

        TempData["Message"] = complete ? "Lesson marked complete." : "Lesson marked incomplete.";
        return RedirectToPage("/Member/Study", null, new { id }, $"lesson-{lessonId}");
    }

    private bool IsEnrolled(MySqlConnection connection, int courseId)
    {
        using var command = new MySqlCommand("SELECT COUNT(*) FROM enrollments WHERE user_id = @user AND course_id = @course", connection);
        command.Parameters.AddWithValue("@user", GetUserId());
        command.Parameters.AddWithValue("@course", courseId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
