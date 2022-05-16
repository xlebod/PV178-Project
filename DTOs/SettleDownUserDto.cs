using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs
{
    public class SettleDownUserDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
