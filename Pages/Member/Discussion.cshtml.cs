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
        if (!Load(connection)) return RedirectToPage("/Courses/Details", new { id = Id });

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
        if (!Load(connection)) return RedirectToPage("/Courses/Details", new { id = Id });
        if (string.IsNullOrWhiteSpace(Body)) ModelState.AddModelError(nameof(Body), "Write a message.");
        if (!ModelState.IsValid) return Page();

        string sql = "";
        if (PostId != 0)
        {
            sql = "UPDATE discussion_posts SET body = @b, updated_utc = UTC_TIMESTAMP() WHERE id = @p AND user_id = @u";
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
        if (!Load(connection)) return RedirectToPage("/Courses/Details", new { id = Id });

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
        using (var command = new MySqlCommand(@"
            SELECT c.title,
                   (SELECT COUNT(*) FROM enrollments WHERE user_id = @u AND course_id = @c) AS enrolled
            FROM courses c WHERE c.id = @c
            ", connection))
        {
            command.Parameters.AddWithValue("@c", Id);
            command.Parameters.AddWithValue("@u", CurrentUserId);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return false;
            int enrolled = Convert.ToInt32(reader["enrolled"]);
            bool isAdmin = User.IsInRole("Admin");
            if (enrolled == 0 && !isAdmin) return false;
            CourseTitle = reader.GetString("title");
        }

        using (var command = new MySqlCommand(@"
            SELECT p.id, p.parent_id, p.user_id, p.body, p.created_utc, p.updated_utc, u.full_name
            FROM discussion_posts p
            JOIN users u ON u.id = p.user_id
            WHERE p.course_id = @c
            ORDER BY p.created_utc
            ", connection))
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
                    UpdatedUtc = reader.GetDateTime("updated_utc"),
                    AuthorName = reader.GetString("full_name")
                });
            }
        }
        return true;
    }
}