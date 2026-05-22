using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewsSite.Models.ViewModels
{
public class UserAdminViewModel
{
    public List<UserDisplayInfo> Users { get; set; } = new();
    public List<SelectListItem> AvailableRoles { get; set; } = new();
    public string? Search { get; set; }
    public string? RoleFilter { get; set; }
    public bool? IsDeletedFilter { get; set; }
}

    public class UserDisplayInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool HasActiveSubscription { get; set; }
    }
}
