using Microsoft.AspNetCore.Mvc;

namespace ASP_NET.Models.Forum;

public class ForumTopicFormModel
{
    [FromForm(Name = "topic-title")]
    public String Title { get; set; } = null!;
    [FromForm(Name = "topic-description")]
    public String Description { get; set; } = null!;
    [FromForm(Name = "theme-id")]
    public String ThemeId { get; set; } = null!;
}