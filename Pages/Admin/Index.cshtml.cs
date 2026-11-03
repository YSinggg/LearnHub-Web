namespace LearnHub.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<Course> Courses { get; set; } = new List<Course>();
    public AdminStats Stats { get; set; } = new AdminStats();
    public int MaxEnrollments
    {
        get
        {
            int max = 1;
            foreach (var c in Courses)
            {
                if (c.Enrollments > max)
                {
                    max = c.Enrollments;
                }
            }
            return max;
        }
    }

    public void OnGet()
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();

        string coursesSql = """
            SELECT c.id, c.title, c.category,
                   (SELECT COUNT(*) FROM lessons l WHERE l.course_id = c.id)   AS lesson_count,
                   (SELECT COUNT(*) FROM questions q WHERE q.course_id = c.id) AS question_count,
                   (SELECT COUNT(*) FROM enrollments e WHERE e.course_id = c.id) AS enrollments,
                   (SELECT COALESCE(ROUND(AVG(a.score / a.total) * 100), 0)
                    FROM quiz_attempts a WHERE a.course_id = c.id) AS average
            FROM courses c
            ORDER BY c.title
            """;
        using (var command = new MySqlCommand(coursesSql, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                Courses.Add(new Course
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Category = reader.GetString("category"),
                    LessonCount = reader.GetInt32("lesson_count"),
                    QuestionCount = reader.GetInt32("question_count"),
                    Enrollments = reader.GetInt32("enrollments"),
                    AveragePercent = Convert.ToInt32(reader["average"]),
                });
            }
        }

        string statsSql = """
            SELECT (SELECT COUNT(*) FROM users)       AS accounts,
                   (SELECT COUNT(*) FROM enrollments) AS enrollments,
                   COUNT(*)                           AS attempts,
                   COALESCE(ROUND(AVG(score / total) * 100), 0) AS average
            FROM quiz_attempts
            """;
        using (var command = new MySqlCommand(statsSql, connection))
        using (var reader = command.ExecuteReader())
        {
            reader.Read();
            Stats = new AdminStats
            {
                Accounts = reader.GetInt32("accounts"),
                Enrollments = reader.GetInt32("enrollments"),
                Attempts = reader.GetInt32("attempts"),
                AveragePercent = Convert.ToInt32(reader["average"]),
            };
        }
    }
}
