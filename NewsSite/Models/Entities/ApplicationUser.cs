using Microsoft.AspNetCore.Identity;

namespace NewsSite.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}