namespace LearnHub.Pages.Member;

public class ResultModel : PageModel
{
    private readonly IConfiguration configuration;

    public ResultModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }

    public QuizAttempt Attempt { get; set; } = new QuizAttempt();
    public List<AnswerReview> Answers { get; set; } = new List<AnswerReview>();

    public int Percent => Attempt.Total == 0 ? 0 : Attempt.Score * 100 / Attempt.Total;
    public int CorrectCount => Answers.Count(a => a.IsCorrect);
    public int IncorrectCount => Answers.Count(a => !a.IsCorrect);
    public int XpEarned => Attempt.Score * 100;
    public string RankTitle => Percent switch
    {
        100 => "S-RANK MASTER",
        >= 80 => "A-RANK SCHOLAR",
        >= 50 => "B-RANK APPRENTICE",
        _ => "LEVELING UP"
    };
    public string RankBadge => Percent switch
    {
        100 => "🏆",
        >= 80 => "🌟",
        >= 50 => "⚡",
        _ => "🎯"
    };

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();

        using (var command = new MySqlCommand(@"
            SELECT a.course_id, a.score, a.total, a.taken_utc, c.title
            FROM quiz_attempts a JOIN courses c ON c.id = a.course_id
            WHERE a.id = @id AND a.user_id = @user
            ", connection))
        {
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@user", GetUserId());
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return NotFound();

            Attempt = new QuizAttempt
            {
                Id = id,
                CourseId = reader.GetInt32("course_id"),
                CourseTitle = reader.GetString("title"),
                Score = reader.GetInt32("score"),
                Total = reader.GetInt32("total"),
                TakenUtc = reader.GetDateTime("taken_utc"),
            };
        }

        using (var command = new MySqlCommand(@"
            SELECT q.prompt,
                   CASE qa.selected_option
                       WHEN 0 THEN q.option_a
                       WHEN 1 THEN q.option_b
                       WHEN 2 THEN q.option_c
                       ELSE '' END AS chosen_text,
                   CASE q.correct_option
                       WHEN 0 THEN q.option_a
                       WHEN 1 THEN q.option_b
                       WHEN 2 THEN q.option_c
                       ELSE '' END AS correct_text,
                   q.explanation,
                   qa.is_correct
            FROM quiz_answers qa
            JOIN questions q ON q.id = qa.question_id
            WHERE qa.attempt_id = @a
            ORDER BY q.id
            ", connection))
        {
            command.Parameters.AddWithValue("@a", id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Answers.Add(new AnswerReview
                {
                    Prompt = reader.GetString("prompt"),
                    ChosenText = reader.GetString("chosen_text"),
                    CorrectText = reader.GetString("correct_text"),
                    Explanation = reader.GetString("explanation"),
                    IsCorrect = Convert.ToBoolean(reader["is_correct"]),
                });
            }
        }
        return Page();
    }
}
