namespace LearnHub.Pages.Member;

public class HistoryModel : PageModel
{
    private readonly IConfiguration configuration;

    public HistoryModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public List<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();

    public int TotalAttempts => Attempts.Count;
    public int AveragePercent => Attempts.Count == 0 ? 0 : (int)Math.Round(Attempts.Average(a => a.Total == 0 ? 0 : (double)a.Score * 100 / a.Total));
    public int BestScorePercent => Attempts.Count == 0 ? 0 : Attempts.Max(a => a.Total == 0 ? 0 : a.Score * 100 / a.Total);

    public void OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand(@"
            SELECT a.id, c.title, a.score, a.total, a.taken_utc
            FROM quiz_attempts a JOIN courses c ON c.id = a.course_id
            WHERE a.user_id = @u
            ORDER BY a.taken_utc DESC, a.id DESC
            ", connection);
        command.Parameters.AddWithValue("@u", UserId);
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

    public IActionResult OnPostDelete(int id, bool confirmed)
    {
        if (!confirmed) return BadRequest();

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM quiz_attempts WHERE id = @a AND user_id = @u", connection);
        command.Parameters.AddWithValue("@a", id);
        command.Parameters.AddWithValue("@u", UserId);
        if (command.ExecuteNonQuery() == 0) return NotFound();

        TempData["Message"] = "Attempt and saved answers deleted.";
        return RedirectToPage();
    }
}
