using System.ComponentModel.DataAnnotations;

namespace YapYapAPI.Models
{
    public class LoginDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(500)]
        public string BIO { get; set; }

        [Required]
        public int status_id { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BIO { get; set; }
        public int status_id { get; set; }
        public DateTime created_at { get; set; }
    }
}