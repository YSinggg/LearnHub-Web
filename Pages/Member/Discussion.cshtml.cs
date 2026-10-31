namespace LearnHub.Pages.Member;

public class DiscussionModel : PageModel
{
    private readonly IConfiguration _configuration;

    public DiscussionModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public int PostId { get; set; }
    [BindProperty] public int ParentId { get; set; }
    [BindProperty, Required(ErrorMessage = "Write a message."), StringLength(2000), Display(Name = "Message")] public string Body { get; set; } = "";

    public string CourseTitle { get; set; } = "";
    public List<DiscussionPost> Posts { get; set; } = new List<DiscussionPost>();
    public int CurrentUserId
{
    get
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null) return 0;
        return int.Parse(claim);
    }
}
    public List<DiscussionPost> Threads => Posts.Where(p => p.ParentId == null).ToList();
    public List<DiscussionPost> RepliesTo(int postId) => Posts.Where(p => p.ParentId == postId).ToList();
    public DiscussionPost? ReplyingTo => Posts.SingleOrDefault(p => p.Id == ParentId && p.ParentId == null);
    public IActionResult OnGet(int edit = 0, int reply = 0)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection)) return Forbid();

        if (reply != 0)
        {
            ParentId = reply;
            if (ReplyingTo == null) return NotFound();
        }

        if (edit != 0)
        {
            var post = Posts.SingleOrDefault(p => p.Id == edit && p.UserId == CurrentUserId);
            if (post == null) return NotFound();
            PostId = edit;
            Body = post.Body;
        }
        return Page();
    }

    public IActionResult OnPostSave()
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection)) return Forbid();
        if (string.IsNullOrWhiteSpace(Body)) ModelState.AddModelError(nameof(Body), "Write a message.");
        if (!ModelState.IsValid) return Page();

        string sql = "";
        if (PostId != 0)
        {
            sql = "UPDATE discussion_posts SET body = @b WHERE id = @p AND user_id = @u";
        }
        else if (ParentId != 0)
        {
            sql = "INSERT INTO discussion_posts (user_id, course_id, parent_id, body) VALUES (@u, @c, @parent, @b)";
        }
        else
        {
            sql = "INSERT INTO discussion_posts (user_id, course_id, body) VALUES (@u, @c, @b)";
        }

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@u", CurrentUserId);
        command.Parameters.AddWithValue("@c", Id);
        command.Parameters.AddWithValue("@b", Body.Trim());
        command.Parameters.AddWithValue("@p", PostId);
        command.Parameters.AddWithValue("@parent", ParentId == 0 ? DBNull.Value : (object)ParentId);
        command.ExecuteNonQuery();

        TempData["Message"] = "Discussion post saved.";
        return RedirectToPage(new { id = Id });
    }

    public IActionResult OnPostDelete()
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("LearningDb"));
        connection.Open();
        if (!Load(connection)) return Forbid();

        using var command = new MySqlCommand(
            "DELETE FROM discussion_posts WHERE id = @p AND user_id = @u", connection);
        command.Parameters.AddWithValue("@p", PostId);
        command.Parameters.AddWithValue("@u", CurrentUserId);
        command.ExecuteNonQuery();

        TempData["Message"] = "Post deleted.";
        return RedirectToPage(new { id = Id });
    }

    private bool Load(MySqlConnection connection)
    {
        using (var command = new MySqlCommand("SELECT title FROM courses WHERE id = @c", connection))
        {
            command.Parameters.AddWithValue("@c", Id);
            var title = command.ExecuteScalar() as string;
            if (title == null) return false;
            CourseTitle = title;
        }

        using (var command = new MySqlCommand("SELECT id, parent_id, user_id, body, created_utc FROM discussion_posts WHERE course_id = @c ORDER BY created_utc", connection))
        {
            command.Parameters.AddWithValue("@c", Id);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Posts.Add(new DiscussionPost
                {
                    Id = reader.GetInt32("id"),
                    ParentId = reader.IsDBNull(reader.GetOrdinal("parent_id")) ? null : reader.GetInt32("parent_id"),
                    UserId = reader.GetInt32("user_id"),
                    Body = reader.GetString("body"),
                    UpdatedUtc = reader.GetDateTime("created_utc"),
                    AuthorName = "Student"
                });
            }
        }
        return true;
    }
}