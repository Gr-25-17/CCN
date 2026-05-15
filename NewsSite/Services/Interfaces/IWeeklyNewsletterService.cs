namespace NewsSite.Services.Interfaces;

public interface IWeeklyNewsletterService
{
    Task<int> SendWeeklyNewsletterAsync();
}
