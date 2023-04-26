namespace ASP_NET.Models.Email;

public class ConfirmEmailModel
{
    public String RealName { get; set; }
    public String Email { get; set; }
    public String EmailCode { get; set; }
    public String ConfirmUrl { get; set; }
}