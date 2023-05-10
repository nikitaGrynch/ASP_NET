namespace ASP_NET.Models.Forum;

public class ForumTopicViewModel
{
    public String UrlIdString { get; set; } = null!;
    public String Title { get; set; } = null!;
    public String Description { get; set; } = null!;
    public String CreatedDtString { get; set; } = null!;
    public String AuthorName { get; set; } = null!;
}