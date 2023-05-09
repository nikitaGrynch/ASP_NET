namespace ASP_NET.Models.Forum;

public class ForumSectionsModel
{
    public Boolean UserCanCreate { get; set; }
    public List<ForumThemeModel> Themes { get; set; } = null!;
    public String? CreateMessage { get; set; }
    
    public Boolean? IsMessagePositive { get; set; }
    
    public ForumThemeFormModel? FormModel { get; set; }
}