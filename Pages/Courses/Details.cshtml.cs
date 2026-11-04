namespace LearnHub.Pages.Courses;

public class DetailsModel : PageModel
{
    private readonly IConfiguration configuration;

    public DetailsModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Course Course { get; set; } = new Course();
    public bool Enrolled { get; set; }
    public List<UserLessonNote> UserNotes { get; set; } = new List<UserLessonNote>();

    public class UserLessonNote
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = "";
        public string NoteText { get; set; } = "";
        public DateTime UpdatedUtc { get; set; }
    }

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using (var command = new MySqlCommand(@"
            SELECT title, category, description, level, image_path,
                   (SELECT COUNT(*) FROM questions q WHERE q.course_id = c.id) AS question_count
            FROM courses c WHERE id = @id
            ", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return NotFound();

            Course = new Course
            {
                Id = id,
                Title = reader.GetString("title"),
                Category = reader.GetString("category"),
                Description = reader.GetString("description"),
                Level = reader.GetString("level"),
                ImagePath = reader.GetString("image_path"),
                QuestionCount = reader.GetInt32("question_count"),
            };
        }

        using (var command = new MySqlCommand("SELECT id, title FROM lessons WHERE course_id = @id ORDER BY position, id", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int lessonId = reader.GetInt32("id");
                string lessonTitle = reader.GetString("title");

                Lesson lesson = new Lesson();
                lesson.Id = lessonId;
                lesson.Title = lessonTitle;

                Course.Lessons.Add(lesson);
            }
        }

        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            using (var command = new MySqlCommand("SELECT COUNT(*) FROM enrollments WHERE user_id = @user AND course_id = @course", connection))
            {
                command.Parameters.AddWithValue("@user", userId);
                command.Parameters.AddWithValue("@course", id);
                Enrolled = Convert.ToInt32(command.ExecuteScalar()) > 0;
            }

            // Load user's private notes for this course to display directly on the course page
            using (var noteCmd = new MySqlCommand(@"
                SELECT l.id AS lesson_id, l.title AS lesson_title, n.note_text, n.updated_utc
                FROM lesson_notes n
                JOIN lessons l ON l.id = n.lesson_id
                WHERE n.user_id = @u AND l.course_id = @c
                ORDER BY l.position, l.id
                ", connection))
            {
                noteCmd.Parameters.AddWithValue("@u", userId);
                noteCmd.Parameters.AddWithValue("@c", id);
                using var reader = noteCmd.ExecuteReader();
                while (reader.Read())
                {
                    UserNotes.Add(new UserLessonNote
                    {
                        LessonId = reader.GetInt32("lesson_id"),
                        LessonTitle = reader.GetString("lesson_title"),
                        NoteText = reader.GetString("note_text"),
                        UpdatedUtc = reader.GetDateTime("updated_utc"),
                    });
                }
            }
        }
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)) return Challenge();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using (var check = new MySqlCommand("SELECT COUNT(*) FROM courses WHERE id = @id", connection))
        {
            check.Parameters.AddWithValue("@id", id);
            if (Convert.ToInt32(check.ExecuteScalar()) == 0) return NotFound();
        }

        bool alreadyEnrolled = false;
        using (var check = new MySqlCommand("SELECT COUNT(*) FROM enrollments WHERE user_id = @u AND course_id = @c", connection))
        {
            check.Parameters.AddWithValue("@u", userId);
            check.Parameters.AddWithValue("@c", id);
            int count = Convert.ToInt32(check.ExecuteScalar());
            if (count > 0)
            {
                alreadyEnrolled = true;
            }
        }

        if (!alreadyEnrolled)
        {
            using var insert = new MySqlCommand("INSERT INTO enrollments (user_id, course_id) VALUES (@u, @c)", connection);
            insert.Parameters.AddWithValue("@u", userId);
            insert.Parameters.AddWithValue("@c", id);
            insert.ExecuteNonQuery();
        }

        TempData["Message"] = "Enrolled successfully.";
        return RedirectToPage(new { id });
    }
}
