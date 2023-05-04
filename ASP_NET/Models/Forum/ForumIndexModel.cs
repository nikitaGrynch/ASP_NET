namespace ASP_NET.Models.Forum;

public class ForumIndexModel
{
    public Boolean UserCanCreate { get; set; }
    public List<ForumSectionModel> Sections { get; set; } = null!;
    public String? CreateMessage { get; set; }
    
    public Boolean? IsMessagePositive { get; set; }
    public ForumSectionFormModel? FormModel { get; set; }

}