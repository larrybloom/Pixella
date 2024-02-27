using System.ComponentModel.DataAnnotations;

namespace Filmzie.Models.Dto
{
    public class FavoriteCreateModel
    {
        [Required]
        public string mediaId { get; set; }

        [Required]
        public string mediaTitle { get; set; }

        [Required]
        public string mediaType { get; set; }


        [Required]
        public string mediaPoster { get; set; }

        [Required]
        public double mediaRate { get; set; }
    }
}