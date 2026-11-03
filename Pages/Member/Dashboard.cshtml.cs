namespace LearnHub.Pages.Member;

public class DashboardModel : PageModel
{
    private readonly IConfiguration configuration;

    public DashboardModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }

    public List<EnrolledCourse> Courses { get; set; } = new List<EnrolledCourse>();
    public List<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();

    public void OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        string coursesSql = @"
            SELECT c.id, c.title, c.category,
                   COUNT(l.id)        AS total,
                   COUNT(p.lesson_id) AS done,
                   (SELECT l2.id
                    FROM lessons l2
                    LEFT JOIN lesson_progress p2 ON p2.lesson_id = l2.id AND p2.user_id = @u
                    WHERE l2.course_id = c.id AND p2.lesson_id IS NULL
                    ORDER BY l2.position, l2.id
                    LIMIT 1) AS next_lesson
            FROM enrollments e
            JOIN courses c ON c.id = e.course_id
            LEFT JOIN lessons l ON l.course_id = c.id
            LEFT JOIN lesson_progress p ON p.lesson_id = l.id AND p.user_id = e.user_id
            WHERE e.user_id = @u
            GROUP BY c.id, c.title, c.category
            ORDER BY c.title
            ";
        using (var command = new MySqlCommand(coursesSql, connection))
        {
            command.Parameters.AddWithValue("@u", GetUserId());
            using var reader = command.ExecuteReader();
            int nextOrdinal = reader.GetOrdinal("next_lesson");
            while (reader.Read())
            {
                Courses.Add(new EnrolledCourse
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Category = reader.GetString("category"),
                    TotalLessons = reader.GetInt32("total"),
                    DoneLessons = reader.GetInt32("done"),
                    NextLessonId = reader.IsDBNull(nextOrdinal) ? null : reader.GetInt32(nextOrdinal),
                });
            }
        }

        string attemptsSql = @"
            SELECT a.id, c.title, a.score, a.total, a.taken_utc
            FROM quiz_attempts a JOIN courses c ON c.id = a.course_id
            WHERE a.user_id = @u
            ORDER BY a.taken_utc DESC, a.id DESC
            LIMIT 10
            ";
        using (var command = new MySqlCommand(attemptsSql, connection))
        {
            command.Parameters.AddWithValue("@u", GetUserId());
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Attempts.Add(new QuizAttempt
                {
                    Id = reader.GetInt32("id"),
                    CourseTitle = reader.GetString("title"),
                    Score = reader.GetInt32("score"),
                    Total = reader.GetInt32("total"),
                    TakenUtc = reader.GetDateTime("taken_utc"),
                });
            }
        }
    }
}
