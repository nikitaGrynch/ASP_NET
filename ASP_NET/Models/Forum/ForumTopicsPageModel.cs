namespace ASP_NET.Models.Forum;

public class ForumTopicsPageModel
{
    public Boolean UserCanCreate { get; set; }
    public List<ForumPostViewModel> Posts { get; set; } = null!;
    public String Title { get; set; } = null!;
    public String Description { get; set; } = null!;
    public String TopicIdString { get; set; } = null!;
    
    
    public String? CreateMessage { get; set; }
    public Boolean? IsMessagePositive { get; set; }
    public ForumPostFormModel? FormModel { get; set; }
}