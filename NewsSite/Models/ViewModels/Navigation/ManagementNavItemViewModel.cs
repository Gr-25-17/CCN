namespace NewsSite.Models.ViewModels.Navigation;

public sealed class ManagementNavItemViewModel
{
    public required string Label { get; init; }
    public required string Controller { get; init; }
    public required string Action { get; init; }
    public bool IsCurrent { get; init; }
}

