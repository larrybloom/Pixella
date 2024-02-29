using Microsoft.AspNetCore.Identity;

namespace Filmies_Data.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }

        // Navigation property for Favorites
        public ICollection<Favorites> Favorites { get; set; }

    }
}
