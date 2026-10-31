using Microsoft.AspNetCore.Authorization;

namespace LearnHub.Pages;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel : PageModel
{
    public void OnGet() { }
    public void OnPost() { }
}
