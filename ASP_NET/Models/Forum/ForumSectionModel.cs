namespace ASP_NET.Models.Forum;

public class ForumSectionModel
{
    public Guid Id { get; set; }
    public String Title { get; set; } = null!;
    public String Description { get; set; } = null!;
    public String Logo { get; set; } = null!;
    public String CreatedDtString { get; set; } = null!;
    public String AuthorName { get; set; } = null!;
    public String? AuthorAvatar { get; set; } = null!;
}