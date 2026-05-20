using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Models
{
    public interface INewsletterContentService
    {
        Task<List<Article>> GetFeaturedArticlesAsync(int count = 5);
        Task<List<Article>> GetEditorsChoiceArticlesAsync();
        Task<List<Article>> GetRecentArticlesAsync(int days = 7, int count = 5);
        Task<string> GenerateNewsletterHtmlAsync(List<Article> articles, string newsletterTitle);
    }
}
