namespace ASP_NET.Services.Email;

public interface IEmailService
{
    bool Send(String templateName, object model);
}