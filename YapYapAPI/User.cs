using System.ComponentModel.DataAnnotations;

namespace YapYapAPI
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(500)]
        public string BIO { get; set; }

        public int status_id { get; set; }

        public DateTime created_at { get; set; }
    }
}