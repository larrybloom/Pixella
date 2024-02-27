using System.ComponentModel.DataAnnotations;

namespace Filmies_Data.Models
{
    public class Favorites
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        // Navigation property for the associated user
        public User User { get; set; }

        [Required]
        public string MediaType { get; set; }

        [Required]
        public string MediaId { get; set; }

        [Required]
        public string MediaTitle { get; set; }

        [Required]
        public string MediaPoster { get; set; }

        [Required]
        public double MediaRate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}