namespace LearnHub.Pages.Admin;

public class ModerationModel : PageModel
{
    private readonly IConfiguration configuration;

    public ModerationModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public List<DiscussionPost> Posts { get; set; } = new List<DiscussionPost>();

    public void OnGet()
    {
        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand(@"
            SELECT p.id, p.parent_id, p.body, u.email, c.title
            FROM discussion_posts p
            JOIN users u ON u.id = p.user_id
            JOIN courses c ON c.id = p.course_id
            ORDER BY p.created_utc DESC
            ", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Posts.Add(new DiscussionPost
            {
                Id = reader.GetInt32("id"),
                ParentId = reader.IsDBNull(reader.GetOrdinal("parent_id")) ? null : reader.GetInt32("parent_id"),
                Body = reader.GetString("body"),
                AuthorEmail = reader.GetString("email"),
                CourseTitle = reader.GetString("title"),
            });
        }
    }

    public IActionResult OnPostDelete(int id, bool confirmed)
    {
        if (!confirmed)
        {
            TempData["Message"] = "Tick the box to confirm before deleting a post.";
            return RedirectToPage();
        }

        using var connection = new MySqlConnection(configuration.GetConnectionString("LearningDb"));
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM discussion_posts WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        int removed = command.ExecuteNonQuery();

        TempData["Message"] = removed == 0 ? "That post was already removed." : "Post removed, together with any replies to it.";
        return RedirectToPage();
    }
}
