namespace NewsSite.Models.ViewModels.Navigation;

public sealed class ManagementNavigationViewModel
{
    public bool ShowManagementDropdown { get; init; }
    public IReadOnlyList<ManagementNavItemViewModel> DesktopItems { get; init; } = [];
    public IReadOnlyList<ManagementNavItemViewModel> MobileItems { get; init; } = [];
}

