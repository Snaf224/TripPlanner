using Microsoft.AspNetCore.Identity;

namespace Diplom.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Gender { get; set; }
    }
}
