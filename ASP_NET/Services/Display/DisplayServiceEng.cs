namespace ASP_NET.Services.Display;

public class DisplayServiceEng : IDisplayService
{
    public string DateString(DateTime dateTime)
    {
        return DateTime.Today == dateTime.Date
            ? "Today " + dateTime.ToString("HH:mm")
            : dateTime.ToString("dd.MM.yyyy HH:mm");
    }

    public string DaysAgoString(DateTime dateTime)
    {
        DateTime dateTimeNow = DateTime.Now;
        return dateTime.Date == dateTimeNow.Date
            ? "Today"
            : (dateTimeNow.Date - dateTime).Days.ToString() + " day(s) ago";
    }

    public string ReduceString(string source, int maxLength)
    {
        if (source.Length <= maxLength) return source;
        source = source[..(maxLength - 3)];

        int lastSpaceIndex = source.LastIndexOf(' ');
        if (maxLength - 3 - lastSpaceIndex < 7
            && maxLength - 3 - lastSpaceIndex > 0)
        {
            source = source[..lastSpaceIndex];
        }

        return source + "...";
    }
}