namespace NewsSite.Models.ViewModels
{
    public class UserAdminViewModel
    {
        public List<UserDisplayInfo> Users { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableRoles { get; set; } = new();
    }

    public class UserDisplayInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}