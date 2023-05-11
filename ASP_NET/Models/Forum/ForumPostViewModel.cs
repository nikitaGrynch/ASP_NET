namespace ASP_NET.Models.Forum;

public class ForumPostViewModel
{
    public String Id { get; set; } = null!;
    public String Content { get; set; } = null!;
    public String CreatedDtString { get; set; } = null!;
    public String AuthorAvatar { get; set; } = null!;
    public String AuthorName { get; set; } = null!;
    public String? ReplyPreview { get; set; }
}