using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using System.Text;

namespace NewsSite.Repositories.Implementations
{

        public class EmailTemplateBuilder
        {
            private readonly string _siteBaseUrl;

            public EmailTemplateBuilder(IConfiguration configuration)
            {

                _siteBaseUrl = configuration["NewsletterSettings:AppUrl"] ?? "https://localhost:7001";
            }

            public string GenerateNewsletterHtml(List<ArticleSummaryViewModel> articles, string newsletterTitle)
            {
                var articlesHtml = new StringBuilder();

                foreach (var article in articles)
                {

                    var articleUrl = $"{_siteBaseUrl}/articles/details/{article.Slug}";

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
                    </table>");
                }

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
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; letter-spacing: 1px;"">📰 CCN Newsletter</h1>
                            <p style=""margin: 10px 0 0; color: #cccccc; font-size: 14px;"">{DateTime.Now:dddd, d MMMM yyyy}</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px;"">
                            <p style=""font-size: 16px; line-height: 1.5; color: #333333; margin: 0 0 25px 0;"">
                                Hej!  Här är veckans mest lästa och rekommenderade artiklar samlade åt dig baserat på dina val.
                            </p>
                            {articlesHtml}
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f9f9f9; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""margin: 0 0 10px 0; color: #666666; font-size: 13px;"">Följ oss för mer nyheter:</p>
                            <p style=""margin: 0 0 20px 0;"">
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">🐦 Twitter/X</a>
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">📘 Facebook</a>
                                <a href=""#"" style=""margin: 0 8px; color: #1a1a2e; text-decoration: none; font-size: 14px;"">📸 Instagram</a>
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 11px; line-height: 1.4;"">
                                Du får detta mejl för att du prenumererar på CCNs nyhetsbrev.<br>
                                <a href=""{_siteBaseUrl}/newsletter/preferences"" style=""color: #999999; text-decoration: underline;"">Hantera inställningar eller avregistrera dig</a>
                            </p>
                            <p style=""margin: 10px 0 0 0; color: #cccccc; font-size: 10px;"">© {DateTime.Now.Year} CCN - Alla rättigheter förbehållna</p>
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

