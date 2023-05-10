namespace ASP_NET.Services.Display;

public interface IDisplayService
{
    String DateString(DateTime dateTime);
    String DaysAgoString(DateTime dateTime);
    String ReduceString(String source, int maxLength);
}