using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Models
{
    public class NewsletterContentService : INewsletterContentService
    {
        private readonly IConfiguration _configuration;

        public NewsletterContentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<List<Article>> GetFeaturedArticlesAsync(int count = 5)
        {
            var testArticles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Sverige lanserar ny digital strategi för 2030",
                    Slug = "sverige-digital-strategi-2030",
                    Summary = "Regeringen presenterar idag en omfattande satsning på digitalisering med fokus på AI, cybersäkerhet och grön teknik.",
                    CategoryName = "Nyheter",
                    AuthorName = "Anna Johansson",
                    ImageUrl = "https://picsum.photos/600/300?random=1",
                    IsPremium = false,
                    IsEditorsChoice = true,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    ViewsCount = 1542
                },
                new Article
                {
                    Id = 2,
                    Title = "AI-revolutionen: Så påverkas din arbetsplats",
                    Slug = "ai-revolutionen-arbetsplats",
                    Summary = "Experter spår att var tredje yrkesroll kommer att förändras radikalt inom fem år. Här är jobben som är säkrast.",
                    CategoryName = "Teknik",
                    AuthorName = "Erik Nilsson",
                    ImageUrl = "https://picsum.photos/600/300?random=2",
                    IsPremium = true,
                    IsEditorsChoice = true,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    ViewsCount = 2847
                },
                new Article
                {
                    Id = 3,
                    Title = "Bostadsmarknaden: Trenden som överraskar alla",
                    Slug = "bostadsmarknaden-trend",
                    Summary = "Priserna fortsätter stiga i mindre städer medan storstäderna svalnar. Mäklarna ser ett nytt mönster.",
                    CategoryName = "Ekonomi",
                    AuthorName = "Maria Berg",
                    ImageUrl = "https://picsum.photos/600/300?random=3",
                    IsPremium = false,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    ViewsCount = 932
                },
                new Article
                {
                    Id = 4,
                    Title = "Premium: Hemligheten bakom världens bästa ledarskap",
                    Slug = "hemligheten-basta-ledarskap",
                    Summary = "Exklusiv intervju med forskaren som kartlagt 1000+ framgångsrika ledare. Lär dig deras tre gemensamma drag.",
                    CategoryName = "Karriär",
                    AuthorName = "Lisa Wikander",
                    ImageUrl = "https://picsum.photos/600/300?random=4",
                    IsPremium = true,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    ViewsCount = 2156
                },
                new Article
                {
                    Id = 5,
                    Title = "Hälsotrenden som förändrar allt: Så börjar du",
                    Slug = "halsotrenden-sa-borjar-du",
                    Summary = "Forskningen bakom den nya rörelseformen som experter kallar 'den tysta revolutionen'.",
                    CategoryName = "Hälsa",
                    AuthorName = "Peter Lund",
                    ImageUrl = "https://picsum.photos/600/300?random=5",
                    IsPremium = false,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    ViewsCount = 743
                }
            };

            return Task.FromResult(testArticles.Take(count).ToList());
        }

        public Task<List<Article>> GetEditorsChoiceArticlesAsync()
        {
            return GetFeaturedArticlesAsync(3); // Just get top 3 for test
        }

        public Task<List<Article>> GetRecentArticlesAsync(int days = 7, int count = 5)
        {
            return GetFeaturedArticlesAsync(count);
        }

        public async Task<string> GenerateNewsletterHtmlAsync(List<Article> articles, string newsletterTitle)
        {
            var articlesHtml = new StringBuilder();

            foreach (var article in articles)
            {
                var articleUrl = $"https://yourdomain.com/article/{article.Slug}";

                var imageHtml = !string.IsNullOrEmpty(article.ImageUrl)
                    ? $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""padding-bottom: 15px;"">
                                    <a href=""{articleUrl}"" style=""text-decoration: none;"">
                                        <img src=""{article.ImageUrl}"" alt=""{article.Title}"" style=""width: 100%; max-width: 560px; height: auto; border-radius: 8px; display: block; border: 0;"" />
                                    </a>
                                </td>
                            </tr>
                          </table>"
                    : "";

                var premiumBadge = article.IsPremium
                    ? "<span style='background: #ffc107; color: #000; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; margin-left: 8px; display: inline-block;'>⭐ PREMIUM</span>"
                    : "";

                var editorsBadge = article.IsEditorsChoice && !article.IsPremium
                    ? "<span style='background: #e74c3c; color: #fff; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; margin-left: 8px; display: inline-block;'>🔥 REDAKTÖRENS VAL</span>"
                    : "";

                articlesHtml.Append($@"
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom: 40px;"">
                        <tr>
                            <td style=""padding-bottom: 30px; border-bottom: 1px solid #e0e0e0;"">
                                {imageHtml}
                                <h2 style=""margin: 0 0 10px 0; font-size: 22px; line-height: 1.3;"">
                                    <a href=""{articleUrl}"" style=""color: #1a1a2e; text-decoration: none;"">
                                        {article.Title}{premiumBadge}{editorsBadge}
                                    </a>
                                </h2>
                                <p style=""color: #666666; font-size: 13px; line-height: 1.4; margin: 0 0 12px 0;"">
                                    <strong>{article.CategoryName}</strong> • {article.CreatedAt:yyyy-MM-dd} • Av {article.AuthorName} • 👁️ {article.ViewsCount} visningar
                                </p>
                                <p style=""color: #444444; font-size: 15px; line-height: 1.5; margin: 0 0 15px 0;"">
                                    {article.Summary}
                                </p>
                                <a href=""{articleUrl}"" style=""color: #0066cc; text-decoration: none; font-weight: bold; font-size: 14px;"">
                                    Läs hela artikeln →
                                </a>
                            </td>
                        </tr>
                    </table>
                ");
            }

            // Complete HTML email template
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{newsletterTitle}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, Helvetica, sans-serif;"">
    
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f4f4;"">
        <tr>
            <td align=""center"" style=""padding: 20px 10px;"">
                
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);"">
                    
                    <!-- HEADER -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; letter-spacing: 1px;"">📰 CCN Newsletter</h1>
                            <p style=""margin: 10px 0 0; color: #cccccc; font-size: 14px;"">{DateTime.Now:dddd, d MMMM yyyy}</p>
                        </td>
                    </tr>
                    
                    <!-- CONTENT -->
                    <tr>
                        <td style=""padding: 30px;"">
                            <p style=""font-size: 16px; line-height: 1.5; color: #333333; margin: 0 0 25px 0;"">
                                Hej! 👋 Här är veckans mest lästa och rekommenderade artiklar samlade åt dig.
                            </p>
                            
                            {articlesHtml}
                        </td>
                    </tr>
                    
                    <!-- FOOTER -->
                    <tr>
                        <td style=""background-color: #f9f9f9; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""margin: 0 0 10px 0; color: #666666; font-size: 13px;"">
                                Följ oss för mer nyheter:
                            </p>
                            <p style=""margin: 0 0 20px 0;"">
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">🐦 Twitter/X</a>
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">📘 Facebook</a>
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">📸 Instagram</a>
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">💼 LinkedIn</a>
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 11px; line-height: 1.4;"">
                                Du får detta mejl för att du prenumererar på CCNs nyhetsbrev.<br>
                                <a href=""#"" style=""color: #999999; text-decoration: underline;"">Avsluta prenumeration</a>
                            </p>
                            <p style=""margin: 10px 0 0 0; color: #cccccc; font-size: 10px;"">
                                © 2024 CCN - Alla rättigheter förbehållna
                            </p>
                        </td>
                    </tr>
                    
                </table>
                
            </td>
        </tr>
    </table>
    
</body>
</html>";
        }
    }
}