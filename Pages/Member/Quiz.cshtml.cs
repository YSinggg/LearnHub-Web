namespace LearnHub.Pages.Member;

public class QuizModel : PageModel
{
    private readonly IConfiguration _configuration;

    public QuizModel(IConfiguration configuration)
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

    [BindProperty] public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        return Load(connection, id) ? Page() : RedirectToPage("/Courses/Details", new { id });
    }

    public IActionResult OnPost(int id)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection, id)) return RedirectToPage("/Courses/Details", new { id });
        if (Course.Questions.Count == 0) return BadRequest();

        bool allAnswered = Course.Questions.All(q => Answers.TryGetValue(q.Id, out int answer) && answer >= 0 && answer <= 2);
        if (!allAnswered) ModelState.AddModelError("", "Please answer every question.");
        if (!ModelState.IsValid) return Page();

        int score = Course.Questions.Count(q => Answers[q.Id] == q.CorrectOption);

        using var transaction = connection.BeginTransaction();

        long attemptId;
        using (var attempt = new MySqlCommand(
            "INSERT INTO quiz_attempts (user_id, course_id, score, total) VALUES (@user, @course, @score, @total)",
            connection, transaction))
        {
            attempt.Parameters.AddWithValue("@user", GetUserId());
            attempt.Parameters.AddWithValue("@course", id);
            attempt.Parameters.AddWithValue("@score", score);
            attempt.Parameters.AddWithValue("@total", Course.Questions.Count);
            attempt.ExecuteNonQuery();
            attemptId = attempt.LastInsertedId;
        }

        foreach (var question in Course.Questions)
        {
            int chosen = Answers[question.Id];
            using var answer = new MySqlCommand(@"
                INSERT INTO quiz_answers (attempt_id, question_id, selected_option, is_correct)
                VALUES (@a, @q, @selected, @ok)
                ", connection, transaction);
            answer.Parameters.AddWithValue("@a", attemptId);
            answer.Parameters.AddWithValue("@q", question.Id);
            answer.Parameters.AddWithValue("@selected", chosen);
            answer.Parameters.AddWithValue("@ok", chosen == question.CorrectOption);
            answer.ExecuteNonQuery();
        }

        transaction.Commit();
        return RedirectToPage("/Member/Result", new { id = attemptId });
    }

    private bool Load(MySqlConnection connection, int id)
    {
        using (var command = new MySqlCommand(@"
            SELECT c.title
            FROM enrollments e JOIN courses c ON c.id = e.course_id
            WHERE e.user_id = @u AND e.course_id = @c
            ", connection))
        {
            command.Parameters.AddWithValue("@u", GetUserId());
            command.Parameters.AddWithValue("@c", id);
            var title = command.ExecuteScalar() as string;
            if (title == null) return false;
            Course = new Course { Id = id, Title = title };
        }

        using (var command = new MySqlCommand(@"
            SELECT id, prompt, option_a, option_b, option_c, correct_option, explanation
            FROM questions WHERE course_id = @c ORDER BY id
            ", connection))
        {
            command.Parameters.AddWithValue("@c", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Course.Questions.Add(new Question
                {
                    Id = reader.GetInt32("id"),
                    Prompt = reader.GetString("prompt"),
                    OptionA = reader.GetString("option_a"),
                    OptionB = reader.GetString("option_b"),
                    OptionC = reader.GetString("option_c"),
                    CorrectOption = reader.GetInt32("correct_option"),
                    Explanation = reader.GetString("explanation"),
                });
            }
        }
        return true;
    }
}
