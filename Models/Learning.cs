using System.Text.RegularExpressions;

namespace LearnHub.Models;

public class UserAccount
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Member";
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Level { get; set; } = "Beginner";
    public string ImagePath { get; set; } = "";
    public string Description { get; set; } = "";
    public int LessonCount { get; set; }
    public int QuestionCount { get; set; }
    public int Enrollments { get; set; }
    public int AveragePercent { get; set; }
    public List<Lesson> Lessons { get; set; } = new List<Lesson>();
    public List<Question> Questions { get; set; } = new List<Question>();
}

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string VideoUrl { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public int Position { get; set; }

    public string? YouTubeId => GetYouTubeId(VideoUrl);
    public bool IsYouTubeVideo => !string.IsNullOrEmpty(YouTubeId);
    public string YouTubeEmbedUrl => $"https://www.youtube-nocookie.com/embed/{YouTubeId}";

    public static string? GetYouTubeId(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return null;
        var match = Regex.Match(link.Trim(), @"(?:v=|embed/|youtu\.be/)([A-Za-z0-9_-]{11})");
        return match.Success ? match.Groups[1].Value : null;
    }

    public static string? NormalizeVideoUrl(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        string? id = GetYouTubeId(input);
        return id == null ? null : $"https://www.youtube-nocookie.com/embed/{id}";
    }
}

public class Question
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Prompt { get; set; } = "";
    public string OptionA { get; set; } = "";
    public string OptionB { get; set; } = "";
    public string OptionC { get; set; } = "";
    public int CorrectOption { get; set; }
    public string Explanation { get; set; } = "";

    public string[] Options => new string[] { OptionA, OptionB, OptionC };
}

public class EnrolledCourse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public int TotalLessons { get; set; }
    public int DoneLessons { get; set; }
    public int? NextLessonId { get; set; }
    public int Percent => TotalLessons == 0 ? 0 : DoneLessons * 100 / TotalLessons;
}

public class QuizAttempt
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
    public DateTime TakenUtc { get; set; }
}

public class AnswerReview
{
    public string Prompt { get; set; } = "";
    public string ChosenText { get; set; } = "";
    public string CorrectText { get; set; } = "";
    public string Explanation { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class DiscussionPost
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int UserId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string AuthorEmail { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime UpdatedUtc { get; set; }
}

public class AdminStats
{
    public int Accounts { get; set; }
    public int Enrollments { get; set; }
    public int Attempts { get; set; }
    public int AveragePercent { get; set; }
}
