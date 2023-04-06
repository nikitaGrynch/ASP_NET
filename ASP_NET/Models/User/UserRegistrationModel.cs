namespace ASP_NET.Models.User;

public class UserRegistrationModel
{
    public String Login { get; set; } = null!;
    public String Password { get; set; } = null!;
    public String RepeatPassword { get; set; } = null!;
    public String Email { get; set; } = null!;
    public String RealName { get; set; } = null!;
    public Boolean IsAgree { get; set; }
    public IFormFile Avatar { get; set; } = null!;
}