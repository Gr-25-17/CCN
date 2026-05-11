using AngleSharp.Dom;
using NewsSite.Mapping;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Implementations;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterPreferenceRepository _preferenceRepo;
    private readonly ICategoryService _categoryService;
    private readonly IUserRepository _userRepository;
    private readonly IArticleRepository _articleRepository;

    public NewsletterService(
        INewsletterPreferenceRepository preferenceRepo,
        ICategoryService categoryService,
        IUserRepository userRepository,
        IArticleRepository articleRepository)
    {
        _preferenceRepo = preferenceRepo;
        _categoryService = categoryService;
        _userRepository = userRepository;
        _articleRepository = articleRepository;
    }

    public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        => (await _categoryService.GetAllAsync()).ToList();

    public async Task<List<AuthorViewModel>> GetAllAuthorsAsync()
    {
        var allUsers = await _userRepository.GetAllUsersAsync();
        var authors = new List<ApplicationUser>();

        foreach (var user in allUsers)
        {
            var roles = await _userRepository.GetUserRolesAsync(user);
            if (roles.Contains("Author") || roles.Contains("Writer"))
            {
                authors.Add(user);
            }
        }

        var authorViewModels = new List<AuthorViewModel>();
        foreach (var author in authors)
        {
            var articles = await _articleRepository.GetByAuthorAsync(author.Id);
            authorViewModels.Add(new AuthorViewModel
            {
                Id = author.Id,
                Name = $"{author.FirstName} {author.LastName}",
                Email = author.Email ?? string.Empty,
                ArticleCount = articles.Count()
            });
        }

        return authorViewModels;
    }

    public async Task<NewsletterPreferencesViewModel> GetPreferencesAsync(string userId)
    {
        var preferences = await _preferenceRepo.GetByUserIdAsync(userId);
        var categories = await GetAllCategoriesAsync();
        var authors = await GetAllAuthorsAsync();

        var viewModel = preferences.ToNewsletterPreferencesViewModel(categories);
        viewModel.AvailableAuthors = authors;

       
        if (!string.IsNullOrEmpty(preferences?.SelectedAuthIds))
        {
            viewModel.SelectedAuthorIds = preferences.SelectedAuthIds;
        }

        return viewModel;
    }


    public async Task SavePreferencesAsync(string userId, NewsletterPreferencesViewModel preferences)
        => await _preferenceRepo.SaveAsync(preferences.ToNewsletterPreferenceEntity(userId));
}
