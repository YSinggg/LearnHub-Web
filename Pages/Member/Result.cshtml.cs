namespace LearnHub.Pages.Member;

public class ResultModel : PageModel
{
    private readonly IConfiguration _configuration;

    public ResultModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }

    public QuizAttempt Attempt { get; set; } = new QuizAttempt();
    public List<AnswerReview> Answers { get; set; } = new List<AnswerReview>();

    public int Percent { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int XpEarned { get; set; }
    public string RankTitle { get; set; } = "";
    public string RankBadge { get; set; } = "";

    public IActionResult OnGet(int id)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
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

        Percent = Attempt.Total > 0 ? (Attempt.Score * 100 / Attempt.Total) : 0;
        XpEarned = Attempt.Score * 100;

        CorrectCount = 0;
        IncorrectCount = 0;
        foreach (var a in Answers)
        {
            if (a.IsCorrect)
                CorrectCount++;
            else
                IncorrectCount++;
        }

        if (Percent == 100)
        {
            RankTitle = "S-RANK MASTER";
            RankBadge = "🏆";
        }
        else if (Percent >= 80)
        {
            RankTitle = "A-RANK SCHOLAR";
            RankBadge = "🌟";
        }
        else if (Percent >= 50)
        {
            RankTitle = "B-RANK APPRENTICE";
            RankBadge = "⚡";
        }
        else
        {
            RankTitle = "LEVELING UP";
            RankBadge = "🎯";
        }

        return Page();
    }
}
