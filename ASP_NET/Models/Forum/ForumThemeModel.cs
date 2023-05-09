namespace ASP_NET.Models.Forum;

public class ForumThemeModel
{
    public String UrlIdString { get; set; } = null!;
    public String Title { get; set; } = null!;
    public String Description { get; set; } = null!;
    public String CreatedDtString { get; set; } = null!;
    public DateTime CreatedDt { get; set; }
    public String AuthorName { get; set; } = null!;
    public String? AuthorAvatar { get; set; } = null!;
    public String AuthorRegisterDtString { get; set; } = null!;
}