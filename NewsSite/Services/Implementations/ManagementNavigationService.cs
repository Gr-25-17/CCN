using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NewsSite.Models.ViewModels.Navigation;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace NewsSite.Services.Implementations;

public class ManagementNavigationService(IAuthorizationService authorizationService) : IManagementNavigationService
{
    public async Task<ManagementNavigationViewModel> BuildAsync(ClaimsPrincipal user, PathString path)
    {
        var isAdmin = user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
        var canAccessWriterPanel = (await authorizationService.AuthorizeAsync(user, "ManagementOnly")).Succeeded;

        var isAdminPage = path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase);
        var isWriterPage = path.StartsWithSegments("/Writer", StringComparison.OrdinalIgnoreCase);
        var isAnalyticsPage = path.StartsWithSegments("/SubscriptionAnalytics", StringComparison.OrdinalIgnoreCase);

        if (isAdmin && canAccessWriterPanel)
        {
            var items = new List<ManagementNavItemViewModel>
            {
                new() { Label = "Admin", Controller = "Admin", Action = "Index", IsCurrent = isAdminPage },
                new() { Label = "Skribent", Controller = "Writer", Action = "Index", IsCurrent = isWriterPage },
                new() { Label = "Analytics", Controller = "SubscriptionAnalytics", Action = "Index", IsCurrent = isAnalyticsPage }
            };

            return new ManagementNavigationViewModel
            {
                ShowManagementDropdown = true,
                DesktopItems = items,
                MobileItems = items
            };
        }

        if (canAccessWriterPanel)
        {
            return new ManagementNavigationViewModel
            {
                DesktopItems = [new() { Label = "Skribent", Controller = "Writer", Action = "Index", IsCurrent = isWriterPage }],
                MobileItems = [new() { Label = "Skribent", Controller = "Writer", Action = "Index", IsCurrent = isWriterPage }]
            };
        }

        return new ManagementNavigationViewModel();
    }
}

