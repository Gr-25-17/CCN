using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewsSite.Models.ViewModels
{
    public class UserAdminViewModel
    {
        public List<UserDisplayInfo> Users { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();
    }

    public class UserDisplayInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
    }
}