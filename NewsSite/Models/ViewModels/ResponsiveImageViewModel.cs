namespace NewsSite.Models.ViewModels;

public class ResponsiveImageViewModel
{
    public string? ImageUrl { get; set; }

    public string Alt { get; set; } = string.Empty;

    public string Size { get; set; } = "med";

    public string Loading { get; set; } = "lazy";

    public string CssClass { get; set; } = string.Empty;

    public string Style { get; set; } = string.Empty;
}
