namespace ASP_NET.Data.Entity;

public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string RealName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? EmailCode { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string? Avatar { get; set; } = null!;
    public DateTime RegisterDt { get; set; }
    public DateTime? LastEnterDt { get; set; }

    public Boolean IsEmailPublic { get; set; } = false;
    public Boolean IsRealNamePublic { get; set; } = false;
    public Boolean IsDatesPublic { get; set; } = false;


}