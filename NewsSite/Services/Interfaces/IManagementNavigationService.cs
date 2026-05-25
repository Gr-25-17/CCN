using NewsSite.Models.ViewModels.Navigation;
using System.Security.Claims;

namespace NewsSite.Services.Interfaces;

public interface IManagementNavigationService
{
    Task<ManagementNavigationViewModel> BuildAsync(ClaimsPrincipal user, PathString path);
}

